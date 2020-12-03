using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace ApocSample
{
    public partial class ApocToken : SmartContract
    {
        private static bool ValidateAddress(UInt160 address) => !address.IsZero;
        private static bool IsContract(UInt160 address) => Blockchain.GetContract(address) != null;
    }
}
