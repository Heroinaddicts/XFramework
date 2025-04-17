using System;
using System.Net.Sockets;
using UnityEngine;

namespace XEngine
{
    class TcpConnection : Api.iNetwork.iTcpConnection
    {
        Socket _Socket; 
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, int offset, int size)
        {
            throw new NotImplementedException();
        }

        public void SetConnectCallback(Action<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void SetDisconnectCallback(Action callback)
        {
            throw new NotImplementedException();
        }

        public void SetReceiveCallback(Func<byte[], int, int, int> callback)
        {
            throw new NotImplementedException();
        }
    }
}

