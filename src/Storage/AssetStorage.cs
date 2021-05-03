using Neo;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace DevHawk.Contracts
{
    public static class AssetStorage
    {
        public static readonly string mapName = "asset";

        public static void Increase(UInt160 key, BigInteger value) => Put(key, Get(key) + value);

        public static void Enable() => new StorageMap(Storage.CurrentContext, mapName).Put("enable", 1);

        public static void Disable() => new StorageMap(Storage.CurrentContext, mapName).Put("enable", 0);

        public static void Reduce(UInt160 key, BigInteger value)
        {
            var oldValue = Get(key);
            if (oldValue == value)
                Remove(key);
            else
                Put(key, oldValue - value);
        }

        public static void Put(UInt160 key, BigInteger value) => new StorageMap(Storage.CurrentContext, mapName).Put(key, value);

        public static BigInteger Get(UInt160 key) => (BigInteger)new StorageMap(Storage.CurrentContext, mapName).Get(key);

        public static bool GetPaymentStatus()
        {
            return ((BigInteger)new StorageMap(Storage.CurrentContext, mapName).Get("enable")).Equals(1);
        }

        public static void Remove(UInt160 key) => new StorageMap(Storage.CurrentContext, mapName).Delete(key);
    }
}
