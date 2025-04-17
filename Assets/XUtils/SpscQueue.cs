using System;
using System.Threading;

namespace XUtils
{
    public class SpscQueue<T>
    {
        private const int ConstNoData = 0;
        private const int ConstHasData = 1;
        private const int ConstExpanding = 2;

        private class QueueSlot
        {
            public T _Data;
            public volatile int _Sign;
        }

        private class Buffer
        {
            public QueueSlot[] _Slots;
            public int _Size;
            public int _Mask;

            public Buffer(int size)
            {
                _Size = size;
                _Mask = size - 1;
                _Slots = new QueueSlot[size];
                for (int i = 0; i < size; i++)
                    _Slots[i] = new QueueSlot();
            }
        }

        private volatile Buffer _Buffer;

        private int _ReadIndex;
        private int _WriteIndex;

        public SpscQueue(int initialSize = 16)
        {
            if ((initialSize & (initialSize - 1)) != 0)
                throw new ArgumentException("Initial size must be a power of 2");

            _Buffer = new Buffer(initialSize);
        }

        public void Enqueue(T item)
        {
            Buffer buffer = _Buffer;
            int index = _WriteIndex & buffer._Mask;

            if (buffer._Slots[index]._Sign != ConstNoData)
            {
                Expand(buffer);
                buffer = _Buffer;
                index = _WriteIndex & buffer._Mask;
            }

            buffer._Slots[index]._Data = item;
            Volatile.Write(ref buffer._Slots[index]._Sign, ConstHasData);
            _WriteIndex++;
        }

        public bool TryDequeue(out T item)
        {
            Buffer buffer = _Buffer;
            int index = _ReadIndex & buffer._Mask;

            if (Volatile.Read(ref buffer._Slots[index]._Sign) != ConstHasData)
            {
                item = default;
                return false;
            }

            item = buffer._Slots[index]._Data;
            Volatile.Write(ref buffer._Slots[index]._Sign, ConstNoData);
            _ReadIndex++;
            return true;
        }

        private void Expand(Buffer oldBuffer)
        {
            // 只允许单生产者调用，因此不需要锁
            int oldSize = oldBuffer._Size;
            int newSize = oldSize * 2;
            Buffer newBuffer = new Buffer(newSize);

            for (int i = _ReadIndex; i < _WriteIndex; i++)
            {
                int oldIndex = i & oldBuffer._Mask;
                int newIndex = i & newBuffer._Mask;

                newBuffer._Slots[newIndex]._Data = oldBuffer._Slots[oldIndex]._Data;
                newBuffer._Slots[newIndex]._Sign = oldBuffer._Slots[oldIndex]._Sign;
            }

            // 替换 buffer，消费者看到新 buffer 后自然继续读取
            _Buffer = newBuffer;
        }

        public int Count => _WriteIndex - _ReadIndex;

        public bool IsEmpty => _WriteIndex == _ReadIndex;
    }
}