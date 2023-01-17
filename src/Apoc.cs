using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;
using System.Numerics;

// explicitly enabling #nullable to eliminate nccs warning
#nullable enable

namespace DevHawk.Contracts
{
    [DisplayName("ApocToken")]
    [ManifestExtra("Author", "Harry Pierson")]
    [ManifestExtra("Email", "harrypierson@hotmail.com")]
    [ManifestExtra("Description", "This is a NEP17 example")]
    [SupportedStandards("NEP-17")]
    public class ApocToken : SmartContract
    {
        const string SYMBOL = "APOC";
        const byte DECIMALS = 8;
        const long INITIAL_SUPPLY = 1_000_000;

        public delegate void OnTransferDelegate(UInt160 from, UInt160 to, BigInteger amount);

        [DisplayName("Transfer")]
        public static event OnTransferDelegate OnTransfer = default!;

        const byte Prefix_TotalSupply = 0x00;
        const byte Prefix_Balance = 0x01;
        const byte Prefix_ContractOwner = 0xFF;

        [Safe]
        public static string Symbol()
        {
            return SYMBOL;
        }

        [Safe]
        public static byte Decimals()
        {
            return DECIMALS;
        }

        [Safe]
        public static BigInteger TotalSupply() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_TotalSupply });

        [Safe]
        public static BigInteger BalanceOf(UInt160 owner)
        {
            if (owner is null || !owner.IsValid)
                throw new Exception("The argument \"owner\" is invalid.");
            StorageMap balanceMap = new(Storage.CurrentContext, Prefix_Balance);
            return (BigInteger)balanceMap[owner];
        }

        public static bool Transfer(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            if (from is null || !from.IsValid)
                throw new Exception("The argument \"from\" is invalid.");
            if (to is null || !to.IsValid)
                throw new Exception("The argument \"to\" is invalid.");
            if (amount < 0)
                throw new Exception("The amount must be a positive number.");
            if (!Runtime.CheckWitness(from)) return false;
            if (amount != 0)
            {
                if (!UpdateBalance(from, -amount))
                    return false;
                UpdateBalance(to, +amount);
            }
            PostTransfer(from, to, amount, data);
            return true;
        }

        public static void Mint(UInt160 account, BigInteger amount)
        {
            if (amount.IsZero) return;
            if (amount.Sign < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!Runtime.CheckWitness(GetOwner()))
                throw new Exception("contract owner must sign mint operations");
            CreateTokens(account, amount);
        }

        public static void Burn(UInt160 account, BigInteger amount)
        {
            if (amount.IsZero) return;
            if (amount.Sign < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            var owner = GetOwner();
            if (!Runtime.CheckWitness(owner))
                throw new Exception("contract owner must sign burn operations");

            if (!UpdateBalance(account, -amount))
                throw new InvalidOperationException();
            UpdateTotalSupply(-amount);
            PostTransfer(account, null, amount, null);
        }

        static void PostTransfer(UInt160? from, UInt160? to, BigInteger amount, object? data)
        {
            from ??= UInt160.Zero;
            to ??= UInt160.Zero;
            OnTransfer(from, to, amount);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, "onNEP17Payment", CallFlags.All, from, amount, data);
        }

        static void UpdateTotalSupply(BigInteger increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_TotalSupply };
            BigInteger totalSupply = (BigInteger)Storage.Get(context, key);
            totalSupply += increment;
            Storage.Put(context, key, totalSupply);
        }

        static bool UpdateBalance(UInt160 owner, BigInteger increment)
        {
            StorageMap balanceMap = new(Storage.CurrentContext, Prefix_Balance);
            BigInteger balance = (BigInteger)balanceMap[owner];
            balance += increment;
            if (balance < 0) return false;
            if (balance.IsZero)
                balanceMap.Delete(owner);
            else
                balanceMap.Put(owner, balance);
            return true;
        }

        [DisplayName("_deploy")]
        public static void Deploy(object _ /*data*/, bool update)
        {
            if (update) return;

            byte[] key = new byte[] { Prefix_ContractOwner };
            var tx = (Transaction)Runtime.ScriptContainer;
            Storage.Put(Storage.CurrentContext, key, tx.Sender);

            CreateTokens(tx.Sender, INITIAL_SUPPLY * BigInteger.Pow(10, DECIMALS));
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            var owner = GetOwner();
            if (!Runtime.CheckWitness(owner))
                throw new Exception("contract owner must sign update operations");
            ContractManagement.Update(nefFile, manifest, null);
        }

        static void CreateTokens(UInt160 account, BigInteger amount)
        {
            UpdateBalance(account, +amount);
            UpdateTotalSupply(+amount);
            PostTransfer(null, account, amount, null);
        }

        static UInt160 GetOwner()
        {
            byte[] key = new byte[] { Prefix_ContractOwner };
            return (UInt160)Storage.Get(Storage.CurrentContext, key);
        }
    }
}
