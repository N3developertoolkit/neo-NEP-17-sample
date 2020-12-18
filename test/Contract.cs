using System;

namespace test
{
    [NeoTestHarness.Contract("DevHawk.Contracts.ApocToken")]
    interface ApocToken
    {
        System.Numerics.BigInteger balanceOf(Neo.UInt160 account);
        System.Numerics.BigInteger decimals();
        void deploy(bool update);
        void destroy();
        void disablePayment();
        void enablePayment();
        void onPayment(Neo.UInt160 from, System.Numerics.BigInteger amount, object? data);
        string symbol();
        System.Numerics.BigInteger totalSupply();
        bool transfer(Neo.UInt160 from, Neo.UInt160 to, System.Numerics.BigInteger amount, object? data);
        void update(byte[] nefFile, string manifest);
        bool verify();
        interface Events
        {
            void Transfer(Neo.UInt160 arg1, Neo.UInt160 arg2, System.Numerics.BigInteger arg3);
        }
    }
}
