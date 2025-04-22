using System;
using System.Net.Sockets;
using UnityEngine;
using XEngine;
using XUtils;

public class TcpServer : Api.iNetwork.iTcpServer
{
    UInt64 _Guid;
    object _Context;
    Action<Api.iNetwork.iTcpServer, Api.iNetwork.iTcpConnection> _AcceptCallback;
    Socket _Socket;

    static XPool<TcpServer> s_Pool = new XPool<TcpServer>(
        () => new TcpServer(),
        (TcpServer t) =>
        {
            t._Guid = 0;
            t._Context = null;
            t._AcceptCallback = null;
        },
        16
    );

    TcpServer() { }

    public static TcpServer Create(UInt64 guid)
    {
        TcpServer server = s_Pool.Get();
        server._Guid = guid;
        server._Context = null;
        server._AcceptCallback = null;
        if (server.Launch())
        {
            return server;
        }

        s_Pool.Return(server);
        return null;
    }

    public bool Launch()
    {
        return false;
    }

    public UInt64 Guid
    {
        get { return _Guid; }
    }

    public T GetContext<T>()
    {
        return (T)_Context;
    }

    public void SetContext<T>(in T context)
    {
        _Context = context;
    }

    public void SetOnConnectionCallback(Action<Api.iNetwork.iTcpServer, Api.iNetwork.iTcpConnection> callback)
    {
        _AcceptCallback = callback;
    }


}
