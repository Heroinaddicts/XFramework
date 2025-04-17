using UnityEngine;

namespace XEngine
{
    public static class Api
    {
        public interface iNetwork
        {
            public interface iTcpConnection
            {
                public void Send(byte[] data, int offset, int size);

                public void SetReceiveCallback(System.Func<byte[], int, int, int> callback);

                public void SetConnectCallback(System.Action<bool> callback);

                public void SetDisconnectCallback(System.Action callback);

                public void Close();
            }

            public interface iTcpServer
            {
                public void SetOnConnectionCallback(System.Action<iTcpConnection> callback);
            }

            public iTcpConnection CreateTcpConnection(string host, int port);
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
    }
}