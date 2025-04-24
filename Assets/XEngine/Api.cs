using System;
using UnityEngine;

namespace XEngine
{
    public static class Api
    {
        public interface iNetwork
        {
            public interface iTcpSocket
            {
                public UInt64 Guid { get; }
                
                public void SetContext<T>(in T context);
                public T GetContext<T>();
            }

            public interface iTcpConnection : iTcpSocket
            {
                public void Send(byte[] data, int offset, int size);
                public void SetReceiveCallback(System.Func<iTcpConnection, byte[], int, int, int> callback);
                public void SetConnectCallback(System.Action<bool, iTcpConnection> callback);
                public void SetDisconnectCallback(System.Action<iTcpConnection> callback);
                public void Close();
            }

            public interface iTcpServer : iTcpSocket
            {
                public void SetOnConnectionCallback(System.Action<iTcpServer, iTcpConnection> callback);
            }

            public T Query<T>(int guid) where T : iTcpSocket;
            public void CreateTcpConnection(string host, int port, Action<iTcpConnection> ret);
            public iTcpServer LaunchTcpServer(string host, int port);

            public void Pause();
            public void Resume();
            public void Update();
        }

        static iNetwork s_NetworkInstance = null;
        public static iNetwork GetNetwork()
        {
            if (s_NetworkInstance == null)
            {
                s_NetworkInstance = new Network();
            }

            return s_NetworkInstance;
        }


        public interface iLog
        {
            public void Debug(in string log);
            public void Trace(in string log);
            public void Error(in string log);
        }

        static iLog s_LogInstance = null;
        public static iLog GetLog()
        {
            if (s_LogInstance == null)
            {
                s_LogInstance = new Logger();
            }
            return s_LogInstance;
        }

        public static void Debug(in string log) { GetLog().Debug(log); }
        public static void Trace(in string log) { GetLog().Trace(log); }
        public static void Error(in string log) { GetLog().Error(log); }
    }
}
