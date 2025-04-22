using System;

namespace XUtils
{
    public static class RandomGeneratorUInt64
    {
        private static Random _Random = new Random();
        public static void SetSeed(int seed)
        {
            _Random = new Random(seed);
        }

        public static UInt64 RandomUInt64()
        {
            // 生成两个 32 位整数，组合成一个 64 位整数
            UInt32 high = (UInt32)_Random.Next();
            UInt32 low = (UInt32)_Random.Next();

            // 将两个 UInt 组合成一个 ulong
            UInt64 result = ((ulong)high << 32) | low;
            return result;
        }
    }
}