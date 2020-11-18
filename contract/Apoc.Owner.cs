using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;

namespace ApocSample
{
    public partial class ApocToken : SmartContract
    {
        public static bool Deploy()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            if (TotalSupplyStorage.Get() > 0) throw new Exception("Contract has been deployed.");

            TotalSupplyStorage.Increase(InitialSupply);
            AssetStorage.Increase(Owner, InitialSupply);

            OnTransfer(null, Owner, InitialSupply);
            return true;
        }

        public static void Update(byte[] script, string manifest)
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            Contract.Update(script, manifest);
        }

        public static void Destroy()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            Contract.Destroy();
        }

        private static bool IsOwner() => Runtime.CheckWitness(Owner);
    }
}
