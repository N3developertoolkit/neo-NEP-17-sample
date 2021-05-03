using Neo.VM;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Persistence;
using Neo.SmartContract.Native;
using Neo.SmartContract;
using System;

namespace ApocContractTests
{
    // TODO: move to Neo.Test.Harness
    static class Extensions
    {
        public static ContractState GetContract(this DataCache snapshot, string contractName)
        {
            foreach (var contractState in NativeContract.ContractManagement.ListContracts(snapshot))
            {
                var name = contractState.Id >= 0 ? contractState.Manifest.Name : "Neo.SmartContract.Native." + contractState.Manifest.Name;
                if (string.Equals(contractName, name))
                {
                    return contractState;
                }
            }

            throw new Exception($"couldn't find {contractName} contract");
        }

        public static VMState ExecuteScript(this TestApplicationEngine engine, Script script)
        {
            engine.LoadScript(script);
            return engine.Execute();
        }
    }
}
