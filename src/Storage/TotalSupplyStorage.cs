using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace DevHawk.Contracts
{
    public static class TotalSupplyStorage
    {
        public static readonly string mapName = "contract";

        public static readonly string key = "totalSupply";

        public static void Increase(BigInteger value) => Put(Get() + value);

        public static void Reduce(BigInteger value) => Put(Get() - value);

        public static void Put(BigInteger value) => new StorageMap(Storage.CurrentContext, mapName).Put(key, value);

        public static BigInteger Get() => (BigInteger)new StorageMap(Storage.CurrentContext, mapName).Get(key);
    }
}
