using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace ApocSample
{
    public partial class ApocToken : SmartContract
    {
        private static BigInteger GetTransactionAmount(Notification notification)
        {
            // Only allow Transfer notifications
            if (notification.EventName != "Transfer") return 0;
            var state = notification.State;
            // Checks notification format
            if (state.Length != 3) return 0;
            // Check dest
            if ((byte[])state[1] != ExecutionEngine.ExecutingScriptHash) return 0;
            // Amount
            var amount = (BigInteger)state[2];
            if (amount < 0) return 0;
            return amount;
        }

        public static bool Mint()
        {
            if (Runtime.InvocationCounter != 1) throw new Exception("InvocationCounter must be 1.");

            var notifications = Runtime.GetNotifications();
            if (notifications.Length == 0) throw new Exception("Contribution transaction not found.");

            BigInteger neo = 0;
            BigInteger gas = 0;

            for (int i = 0; i < notifications.Length; i++)
            {
                var notification = notifications[i];

                if (notification.ScriptHash == NeoToken)
                {
                    neo += GetTransactionAmount(notification);
                }
                else if (notification.ScriptHash == GasToken)
                {
                    gas += GetTransactionAmount(notification);
                }
            }

            var totalSupply = TotalSupplyStorage.Get();
            if (totalSupply <= 0) throw new Exception("Contract not deployed.");

            var avaliable_supply = MaxSupply - totalSupply;

            var contribution = (neo * TokensPerNEO) + (gas * TokensPerGAS);
            if (contribution <= 0) throw new Exception("Contribution cannot be zero.");
            if (contribution > avaliable_supply) throw new Exception("Insufficient supply for mint tokens.");

            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            AssetStorage.Increase(tx.Sender, contribution);
            TotalSupplyStorage.Increase(contribution);

            OnTransfer(null, tx.Sender, contribution);
            return true;
        }
    }
}
