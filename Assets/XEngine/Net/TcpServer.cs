using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using XEngine;
using XUtils;
using static XEngine.NetDefine;

class TcpServer : Api.iNetwork.iTcpServer
{
    string _Host;
    int _Port;
    UInt64 _Guid;
    object _Context;
    Action<Api.iNetwork.iTcpServer, Api.iNetwork.iTcpConnection> _AcceptCallback;
    Socket _Socket;
    Network _Network;

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

    public static TcpServer Create(UInt64 guid, in string host, in int port, Network nw)
    {
        TcpServer server = s_Pool.Get();
        server._Host = host;
        server._Port = port;
        server._Guid = guid;
        server._Context = null;
        server._AcceptCallback = null;
        server._Network = nw;
        if (server.Launch())
        {
            return server;
        }

        s_Pool.Return(server);
        return null;
    }

    public bool Launch()
    {
        try
        {
            _Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                DualMode = true,  // 启用 DualMode 支持 IPv4 和 IPv6
                NoDelay = true
            };
            IPAddress addr = IPAddress.Parse(_Host);

            _Socket.Bind(new IPEndPoint(addr, _Port));
            _Socket.Listen(100);
            
            return AsyncAccept();
        }
        catch (SocketException e)
        {
            Api.Error(e.Message);
            return false;
        }
    }

    bool AsyncAccept() {
        try {
            _Socket.BeginAccept(this.AsyncAcceptCallback, null);
            return true;
        } catch(SocketException e) {
            Api.Error(e.Message);
            return false;
        }
    }

    void AsyncAcceptCallback(IAsyncResult ar) {
        try {
            Socket sock = _Socket.EndAccept(ar);
            _Network.PushEvent(eTcpEvent.Accept, this, CODE_SUCCESS, sock);
        } catch(SocketException e) {
            Api.Error(e.Message);
        } finally {
            AsyncAccept();
        }
    }

    public void OnAccept(Socket sock)
    {
        if (sock == null)
        {
            Api.Error("TcpServer::OnAccept socket is null");
            return;
        }

        TcpConnection con = TcpConnection.Create(sock, NetGUID.Generator(), _Network);
        if (con != null && _AcceptCallback != null)
        {
            _AcceptCallback(this, con);
        } else
        {
            sock.Close();
        }
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
