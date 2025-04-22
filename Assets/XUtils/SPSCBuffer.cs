using System;
using System.Threading;
using UnityEditor;

namespace XUtils
{
    public interface IXBuffer
    {
        void Write(in byte[] data, in int offset, in int len);
        int GetLength();
        bool Read(Func<byte[], int, int, int> func);
        bool Out(int length);

        void Clear();

        void SetLogFun(Action<string> logFun);
    }

    public class SPSCBuffer : IXBuffer
    {
        byte[] _Buffer;
        readonly int _Capacity;
        int _Size;
        int _In;
        int _Out;

        Action<string> _Logger;

        object _Lock = new object();

        public void SetLogFun(Action<string> logFun)
        {
            _Logger = logFun;
        }

        void Log(in string log)
        {
            if (_Logger != null)
            {
                _Logger(log);
            }
        }

        public SPSCBuffer(in int size = 1024, in int capacity = 512)
        {
            _Buffer = new byte[size];
            _Capacity = capacity;
            _Size = size;
            _In = 0;
            _Out = 0;
            _Logger = null;
        }

        public void Clear()
        {
            lock(_Lock)
            {
                _In = 0;
                _Out = 0;
            }
        }

        public int GetLength()
        {
            return _In - _Out;
        }

        public bool Out(int length)
        {
            if (length > GetLength())
            {
                Log($"Out length {length} > GetLength {GetLength()}");
                return false;
            }

            Volatile.Write(ref _Out, _Out + length);
            return true;
        }

        public bool Read(Func<byte[], int, int, int> func)
        {
            int ret = 0;

            lock(_Lock)
            {
                ret = func(_Buffer, _Out, _In - _Out);
                if (ret > 0) { 
                    Volatile.Write(ref _Out, _Out + ret);
                }
            }

            return ret > 0;
        }

        int GetDataSize => _In - _Out;
        int GetTailSize => _Size - _In;
        int GetFreeSize => _Size - GetDataSize;

        public void Write(in byte[] data, in int offset, in int len)
        {
            if (GetTailSize < len)
            {
                lock (_Lock)
                {
                    if (GetTailSize < len)
                    {
                        if (GetFreeSize < len)
                        {
                            byte[] old = _Buffer;
                            do
                            {
                                _Size += _Capacity;
                            } while (GetFreeSize < len);

                            _Buffer = new byte[_Size];
                            Array.Copy(old, _Out, _Buffer, 0, GetDataSize);
                        } else
                        {
                            if (_In > _Out)
                            {
                                Array.Copy(_Buffer, _Out, _Buffer, 0, GetDataSize);
                            }
                        }

                        Volatile.Write(ref _In, _In - _Out);
                        Volatile.Write(ref _Out, 0);
                    }
                }
            }

            Array.Copy(data, offset, _Buffer, _In, len);
            Volatile.Write(ref _In, _In + len);
        }
    }
}
