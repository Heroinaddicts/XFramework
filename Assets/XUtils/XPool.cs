using System;
using System.Collections.Concurrent;
using System.Threading;

namespace XUtils
{
    public class XPool<T>
    {
        public delegate void ResetRefDelegate<T>(ref T item);

        private readonly ConcurrentBag<T> _Objects;
        private readonly Func<T> _ObjectGenerator;
        private readonly Action<T> _ObjectReset;
        private readonly int _MaxSize;
        private int _Count;


        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="objectGenerator">����������ʵ����ί�У�����Ϊ��</param>
        /// <param name="objectReset">����ʱִ�е������߼�����Ϊ null</param>
        /// <param name="maxSize">�������������Ĭ��Ϊ int.MaxValue</param>
        public XPool(Func<T> objectGenerator, Action<T> objectReset = null, int maxSize = int.MaxValue)
        {
            _ObjectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _ObjectReset = objectReset;
            _MaxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));
            _Objects = new ConcurrentBag<T>();
            _Count = 0;
        }

        /// <summary>
        /// ��ȡһ�������п������ò����ã������½�
        /// </summary>
        public T Get()
        {
            if (_Objects.TryTake(out var item))
            {
                // ���ټ�������������
                Interlocked.Decrement(ref _Count);
                _ObjectReset?.Invoke(item);
                return item;
            }
            // ��Ϊ��ʱ������ʵ��
            return _ObjectGenerator();
        }

        /// <summary>
        /// ����һ���������δ������������������У�������
        /// </summary>
        public void Return(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // ���Ӽ������ж��Ƿ񳬳�����
            if (Interlocked.Increment(ref _Count) <= _MaxSize)
            {
                _Objects.Add(item);
            }
            else
            {
                // �����������ָ���������������
                Interlocked.Decrement(ref _Count);
            }
        }

        /// <summary>
        /// ��ǰ���п��ö�������������ֵ��
        /// </summary>
        public int Count => Volatile.Read(ref _Count);
    }
}
