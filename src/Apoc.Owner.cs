using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;

namespace DevHawk.Contracts
{
    public partial class ApocToken : SmartContract
    {
        // TODO: Change name to _deploy once https://github.com/neo-project/neo/issues/2158 is fixed
        public static void Deploy(bool update)
        {
            if (update) return;

            if (!IsOwner()) throw new Exception("No authorization.");
            if (TotalSupplyStorage.Get() > 0) throw new Exception("Contract has been deployed.");

            TotalSupplyStorage.Increase(InitialSupply);
            AssetStorage.Increase(Owner, InitialSupply);

            OnTransfer(null, Owner, InitialSupply);
        }

        public static void Update(byte[] nefFile, string manifest)
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            ManagementContract.Update(nefFile, manifest);
        }

        public static void Destroy()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            ManagementContract.Destroy();
        }

        public static void EnablePayment()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            AssetStorage.Enable();
        }

        public static void DisablePayment()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            AssetStorage.Disable();
        }

        private static bool IsOwner() => Runtime.CheckWitness(Owner);
    }
}
