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
            // �������� 32 λ��������ϳ�һ�� 64 λ����
            UInt32 high = (UInt32)_Random.Next();
            UInt32 low = (UInt32)_Random.Next();

            // ������ UInt ��ϳ�һ�� ulong
            UInt64 result = ((ulong)high << 32) | low;
            return result;
        }
    }
}