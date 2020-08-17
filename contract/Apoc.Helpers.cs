using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace ApocSample
{
    public partial class ApocToken : SmartContract
    {
        private static bool ValidateAddress(byte[] address) => address.Length == 20 && address.ToBigInteger() != 0;

        private static bool IsPayable(byte[] address) => Blockchain.GetContract(address)?.IsPayable ?? true;
    }
}
