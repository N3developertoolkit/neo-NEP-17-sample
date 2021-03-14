using Neo;
using NeoTestHarness;
using Neo.Wallets;

namespace test
{
    static class Common
    {
        public readonly static UInt160 OWEN = "NM2BiALtwxBeCwC2vb5jeEiujwjvxifg4p".ToScriptHash(ProtocolSettings.Default.AddressVersion);
        public readonly static UInt160 ALICE = "NSniVsBpPepBDYHzzygTbAoz1V6bbuyWnF".ToScriptHash(ProtocolSettings.Default.AddressVersion);
    }
}
