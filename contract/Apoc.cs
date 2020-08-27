using Neo.SmartContract.Framework;
using System;
using System.ComponentModel;
using System.Numerics;

namespace ApocSample
{
    [ManifestExtra("Author", "Harry Pierson")]
    [ManifestExtra("Email", "hpierson@ngd.neo.org")]
    [ManifestExtra("Description", "This is a NEP5 example")]
    [SupportedStandards("NEP5", "NEP10")]
    [Features(ContractFeatures.HasStorage | ContractFeatures.Payable)]
    public partial class ApocToken : SmartContract
    {
        #region Token Settings
        static readonly ulong MaxSupply = 10_000_000_000_000_000;
        static readonly ulong InitialSupply = 2_000_000_000_000_000;
        static readonly byte[] Owner = "Nc2TJmEh7oM2wrXKdAQH5gHpy8HnyztcME".ToScriptHash();
        static readonly ulong TokensPerNEO = 1_000_000_000;
        static readonly ulong TokensPerGAS = 1;

        // TODO: Preview 3 doesn't provide a way to convert hex string to script hash correctly
        //       https://github.com/neo-project/neo-devpack-dotnet/issues/342
        //       temporary workaround to manually reverse the hex representation.

        // static readonly byte[] NeoToken = "0xde5f57d430d3dece511cf975a8d37848cb9e0525".HexToBytes();
        // static readonly byte[] GasToken = "0x668e0c1f9d7b70a99dd9e06eadd4c784d641afbc".HexToBytes();

        static readonly byte[] NeoToken = "0x25059ecb4878d3a875f91c51ceded330d4575fde".HexToBytes();
        static readonly byte[] GasToken = "0xbcaf41d684c7d4ad6ee0d99da9707b9d1f0c8e66".HexToBytes();
        
        #endregion

        #region Notifications
        [DisplayName("Transfer")]
        public static event Action<byte[], byte[], BigInteger> OnTransfer;
        #endregion

        // When this contract address is included in the transaction signature,
        // this method will be triggered as a VerificationTrigger to verify that the signature is correct.
        // For example, this method needs to be called when withdrawing token from the contract.
        public static bool Verify() => IsOwner();

        public static string Name() => "Apoc Sample Token";

        public static string Symbol() => "APOC";

        public static ulong Decimals() => 8;
    }
}
