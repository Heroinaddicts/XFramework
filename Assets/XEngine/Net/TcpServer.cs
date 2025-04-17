using System;
using UnityEngine;
using XEngine;

public class TcpServer : Api.iNetwork.iTcpServer
{
    public void SetOnConnectionCallback(Action<Api.iNetwork.iTcpConnection> callback)
    {
        throw new NotImplementedException();
    }
}
