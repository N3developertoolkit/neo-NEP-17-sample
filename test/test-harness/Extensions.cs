using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo;
using Neo.BlockchainToolkit.Persistence;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;

namespace NeoTestHarness
{
    public static class Extensions
    {
        // TestHarness replacement for Neo.Wallets.Helper.ToAddress that doesn't load protocol settings
        public static string ToAddress(this UInt160 scriptHash, byte addressVersion = (byte)0x35)
        {
            Span<byte> data = stackalloc byte[21];
            data[0] = addressVersion;
            Neo.IO.Helper.ToArray(scriptHash).CopyTo(data[1..]);
            return Neo.Cryptography.Base58.Base58CheckEncode(data);
        }

        // TestHarness replacement for Neo.Wallets.Helper.ToScriptHash that doesn't load protocol settings
        public static UInt160 FromAddress(this string address, byte addressVersion = (byte)0x35)
        {
            byte[] data = Neo.Cryptography.Base58.Base58CheckDecode(address);
            if (data.Length != 21)
                throw new FormatException();
            if (data[0] != addressVersion)
                throw new FormatException();
            return new UInt160(data.AsSpan(1));
        }

        public static SnapshotView CreateSnapshot(this CheckpointStore @this, Block? block = null)
        {
            var snapshot = new SnapshotView(@this);
            if (block != null) SetPersistingBlock(snapshot, block);
            return snapshot;


            static void SetPersistingBlock(StoreView @this, Block block)
            {
                var set = typeof(StoreView).GetMethod("set_PersistingBlock", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty)
                    ?? throw new Exception("Reflection error");
                set.Invoke(@this, new object[] { block });
            }
        }

        public static VMState ExecuteScript<T>(this TestApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            engine.LoadScript<T>(expressions);
            return engine.Execute();
        }

        public static void LoadScript<T>(this TestApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            var script = engine.Snapshot.CreateScript<T>(expressions);
            engine.LoadScript(script);
        }

        public static Script CreateScript<T>(this StoreView store, params Expression<Action<T>>[] expressions)
            where T : class
        {
            var scriptHash = store.GetContractAddress<T>();
            using var builder = new ScriptBuilder();
            for (int i = 0; i < expressions.Length; i++)
            {
                var methodCall = (MethodCallExpression)expressions[i].Body;
                var operation = methodCall.Method.Name;

                for (var x = methodCall.Arguments.Count - 1; x >= 0; x--)
                {
                    var obj = Expression.Lambda(methodCall.Arguments[x]).Compile().DynamicInvoke();
                    builder.EmitPush(obj);
                }
                builder.EmitPush(methodCall.Arguments.Count);
                builder.Emit(OpCode.PACK);
                builder.EmitPush(operation);
                builder.EmitPush(scriptHash);
                builder.EmitSysCall(ApplicationEngine.System_Contract_Call);
            }
            return builder.ToArray();
        }

        public static IEnumerable<(byte[] key, StorageItem item)> GetContractStorages<T>(this StoreView store)
            where T : class
        {
            var contract = store.GetContract<T>();
            var prefix = StorageKey.CreateSearchPrefix(contract.Id, default);
            return store.Storages.Find(prefix)
                .Select(s => (s.Key.Key, s.Value));
        }

        public static StorageItem GetContractStorageItem<T>(this StoreView store, ReadOnlyMemory<byte> key)
            where T : class
            => store.GetContractStorages<T>().Single(s => s.key.AsSpan().SequenceEqual(key.Span)).item;

        public static UInt160 GetContractAddress<T>(this StoreView store)
            where T : class
            => store.GetContract<T>().Hash;

        public static ContractState GetContract<T>(this StoreView store)
            where T : class
        {
            var contractName = GetContractName(typeof(T));

            foreach (var contractState in NativeContract.Management.ListContracts(store))
            {
                var name = contractState.Id >= 0 ? contractState.Manifest.Name : "Neo.SmartContract.Native." + contractState.Manifest.Name;
                if (string.Equals(contractName, name))
                {
                    return contractState;
                }
            }

            throw new Exception($"couldn't find {contractName} contract");

            static string GetContractName(Type type)
            {
                if (type.IsNested)
                {
                    return GetContractName(type.DeclaringType ?? throw new Exception("reflection"));
                }

                var attrib = ContractAttribute.GetCustomAttribute(type, typeof(ContractAttribute));
                if (attrib is ContractAttribute contractAttrib)
                {
                    return contractAttrib.Name;
                }

                return type.FullName ?? throw new Exception("reflection");
            }
        }
    }
}
