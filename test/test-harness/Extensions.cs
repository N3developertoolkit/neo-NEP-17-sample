using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Neo;
using Neo.Ledger;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;

namespace NeoTestHarness
{
    public static class Extensions
    {

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

        public static UInt160 FromAddress(this string address)
        {
            return Neo.Wallets.Helper.ToScriptHash(address);
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
            var typeName = typeof(T).FullName;
            foreach (var contractState in NativeContract.Management.ListContracts(store))
            {
                var name = contractState.Id >= 0 ? contractState.Manifest.Name : "Neo.SmartContract.Native." + contractState.Manifest.Name;
                if (string.Equals(typeName, name))
                {
                    return contractState;
                }
            }

            throw new Exception($"couldn't find {typeName} contract");
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
