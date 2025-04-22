using System;
using System.Net.Sockets;
using XUtils;
using static XEngine.Api.iNetwork;

namespace XEngine
{
    static class NetGUID {
        static UInt64 _GuidOffset = RandomGeneratorUInt64.RandomUInt64();

        public static UInt64 Generator() {
            return _GuidOffset ++;
        }
    }

    static class NetDefine
    {
        public enum eTcpEvent
        {
            Unknown = -1,
            Accept,
            Connect,
            Recv,
            Send,
            Disconnect,
        }

        public const int CODE_UNKNOWN = -1;
        public const int CODE_SUCCESS = 0;

        public class NetEvent
        {
            public eTcpEvent _Type;
            public iTcpSocket _S;
            public Socket _Socket;
            public int _Code;
            public Action<iTcpConnection> _CRet;
            public Action<iTcpServer> _SRet;
        }

        public static readonly XPool<NetEvent> s_EventPool = new XPool<NetEvent>(
            () => new NetEvent(), 
            (NetEvent t) =>
            {
                t._Type = eTcpEvent.Unknown;
                t._S = null;
                t._Code = CODE_UNKNOWN;
                t._CRet = null;
                t._SRet = null;
            }, 
            1024
        );
        public class Flag
        {
            public bool _V = false;

            public static implicit operator Flag(bool v)
            {
                return new Flag { _V = v };
            }

            public static implicit operator bool(Flag f)
            {
                return f._V;
            }
        }
    }
}