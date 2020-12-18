using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
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
using Xunit;
using ECPoint = Neo.Cryptography.ECC.ECPoint;
using ECCurve = Neo.Cryptography.ECC.ECCurve;

namespace NeoTestHarness
{
    public static class Extensions
    {
        public static SnapshotView CreateSnapshot(this CheckpointStore @this, Block? block = null)
        {
            var snapshot = new SnapshotView(@this);
            if (block != null) snapshot.SetPersistingBlock(new Block());
            return snapshot;
        }

        public static void SetPersistingBlock(this StoreView @this, Block block)
        {
            var set = typeof(StoreView).GetMethod("set_PersistingBlock", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty)
                ?? throw new Exception("Reflection error");
            set.Invoke(@this, new object[] { block });
        }

        public static string GetString(this StackItem item) 
            => Neo.Utility.StrictUTF8.GetString(item.GetSpan());

        public static VMState AssertScript<T>(this TestApplicationEngine engine, Expression<Action<T>> expression)
            where T : class
        {
            engine.LoadScript<T>(expression);
            return engine.AssertExecute();
        }

        public static VMState AssertScript<T>(this TestApplicationEngine engine, Expression<Action<T>> expression1, Expression<Action<T>> expression2)
            where T : class
        {
            engine.LoadScript<T>(expression1, expression2);
            return engine.AssertExecute();
        }

        public static VMState AssertScript<T>(this TestApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            engine.LoadScript<T>(expressions);
            return engine.AssertExecute();
        }

        public static void LoadScript<T>(this TestApplicationEngine engine, Expression<Action<T>> expression)
            where T : class
        {
            var script = engine.Snapshot.CreateScript<T>(expression);
            engine.LoadScript(script);
        }

        public static void LoadScript<T>(this TestApplicationEngine engine, Expression<Action<T>> expression1, Expression<Action<T>> expression2)
            where T : class
        {
            var script = engine.Snapshot.CreateScript<T>(expression1, expression2);
            engine.LoadScript(script);
        }

        public static void LoadScript<T>(this TestApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            var script = engine.Snapshot.CreateScript<T>(expressions);
            engine.LoadScript(script);
        }

        public static Script CreateScript<T>(this StoreView store, Expression<Action<T>> expression)
            where T : class
        {
            using var builder = new ScriptBuilder();
            builder.AddInvoke<T>(store, expression);
            return builder.ToArray();
        }

        public static Script CreateScript<T>(this StoreView store, Expression<Action<T>> expression1, Expression<Action<T>> expression2)
            where T : class
        {
            using var builder = new ScriptBuilder();
            builder.AddInvoke<T>(store, expression1);
            builder.AddInvoke<T>(store, expression2);
            return builder.ToArray();
        }

        public static Script CreateScript<T>(this StoreView store, params Expression<Action<T>>[] expressions)
            where T : class
        {
            using var builder = new ScriptBuilder();
            for (int i = 0; i < expressions.Length; i++)
            {
                builder.AddInvoke(store, expressions[i]);
            }
            return builder.ToArray();
        }

        public static void AddInvoke<T>(this ScriptBuilder builder, StoreView store, Expression<Action<T>> expression)
            where T : class
        {
            var methodCall = (MethodCallExpression)expression.Body;

            var scriptHash = store.GetContractAddress<T>();
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

        public static void AssertNotification<T>(this TestApplicationEngine engine, int index, Expression<Action<T>> expression) where T : class
        {
            Assert.True(engine.Notifications.Count > index);
            var notification = engine.Notifications[index];

            var contract = engine.Snapshot.GetContract<T>();
            Assert.Equal(contract.Hash, notification.ScriptHash);

            var methodCall = (MethodCallExpression)expression.Body;
            Assert.Equal(methodCall.Method.Name, notification.EventName);
            Assert.Equal(methodCall.Arguments.Count, notification.State.Count);

            for (var i = 0; i < methodCall.Arguments.Count; i++)
            {
                var obj = Expression.Lambda(methodCall.Arguments[i]).Compile().DynamicInvoke();
                var arg = notification.State[i];
                obj.AssertEqual(arg);
            }
        }

        public static void AssertEqual(this object? expected, StackItem actual)
        {
            if (expected == null)
            {
                Assert.True(actual.IsNull);
            }
            else
            {
                switch (expected)
                {
                    case UInt160 expectedHash160:
                        Assert.Equal(expectedHash160, new UInt160(actual.GetSpan()));
                        break;
                    case UInt256 expectedHash256:
                        Assert.Equal(expectedHash256, new UInt256(actual.GetSpan()));
                        break;
                    case BigInteger expectedInt:
                        Assert.Equal(expectedInt, actual.GetInteger());
                        break;
                    case bool expectedBool:
                        Assert.Equal(expectedBool, actual.GetBoolean());
                        break;
                    case string expectedStr:
                        Assert.Equal(expectedStr, actual.GetString());
                        break;
                    case byte[] expectedBytes:
                        Assert.True(expectedBytes.AsSpan().SequenceEqual(actual.GetSpan()));
                        break;
                    case object[] _:
                        Assert.Equal(StackItemType.Array, actual.Type);
                        break;
                    case ECPoint expectedECPoint:
                        Assert.Equal(expectedECPoint, ECPoint.DecodePoint(actual.GetSpan(), ECCurve.Secp256r1));
                        break;
                    default:
                        Assert.False(true);
                        break;
                }
            }
        }

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

        public static VMState AssertExecute(this Neo.SmartContract.ApplicationEngine engine)
        {
            var state = engine.Execute();

            if (state != Neo.VM.VMState.HALT)
            {
                if (engine.FaultException != null)
                    throw engine.FaultException;
                else
                    throw new Xunit.Sdk.EqualException(Neo.VM.VMState.HALT, state);
            }

            return state;
        }
    }
}
