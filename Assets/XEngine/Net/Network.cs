using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.tvOS;
using XUtils;
using static PlasticPipe.PlasticProtocol.Client.ConnectionCreator.PlasticProtoSocketConnection;
using static XEngine.Api.iNetwork;
using static XEngine.NetDefine;

namespace XEngine {
    class Network : Api.iNetwork
    {
        SPSCQueue<int> _Queue = new SPSCQueue<int>();
        UInt64 s_GuidOffset = RandomGeneratorUInt64.RandomUInt64();
        SPSCQueue<NetEvent> _EventQueue = new SPSCQueue<NetEvent>();

        public void PushEvent(eTcpEvent ev, iTcpSocket s, int code)
        {
            var e = NetDefine.s_EventPool.Get();
            e._Type = ev;
            e._S = s;
            e._Code = code;

            _EventQueue.Push(e);
        }

        public void CreateTcpConnection(string host, int port, Action<iTcpConnection> ret)
        {
            Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                DualMode = true,
                NoDelay = true,
                SendTimeout = 5000,
                ReceiveTimeout = 5000
            };
            try
            {
                IPAddress[] addrs = Dns.GetHostAddresses(host);

                foreach (var addr in addrs)
                {
                    // Ö»³¢ÊÔ InterNetwork (IPv4) »ò InterNetworkV6 (IPv6)
                    if (addr.AddressFamily != AddressFamily.InterNetwork &&
                        addr.AddressFamily != AddressFamily.InterNetworkV6)
                        continue;

                    var remoteEP = new IPEndPoint(addr, port);
                    try
                    {
                        s.BeginConnect(remoteEP, (ar) =>
                        {
                            var ev = NetDefine.s_EventPool.Get();
                            ev._Type = NetDefine.eTcpEvent.Connect;
                            ev._CRet = ret;
                            try
                            {
                                s.EndConnect(ar);
                                TcpConnection con = TcpConnection.Create(s, s_GuidOffset++, this);
                                if (null != con)
                                {
                                    ev._Code = NetDefine.CODE_SUCCESS;
                                    ev._S = con;
                                }
                                else
                                {
                                    ev._Code = NetDefine.CODE_UNKNOWN;
                                }
                            }
                            catch (SocketException e)
                            {
                                ev._Code = e.ErrorCode;
                                s.Close();
                                s = null;
                            }
                            _EventQueue.Push(ev);
                        }, s);
                        break;
                    }
                    catch(Exception ex)
                    {
                        Api.Trace($"BeginConnect µ½ {remoteEP} Ê§°Ü: {ex.Message}");
                    }
                }
            }catch (SocketException ex)
            {
                s.Close();
                s = null;
                Api.Error("Unexpected exception: " + ex.Message);
                ret(null);
            }
        }

        public void LaunchTcpServer(string host, int port, Action<iTcpServer> ret)
        {
            throw new System.NotImplementedException();
        }

        public void Pause()
        {
            throw new System.NotImplementedException();
        }

        public T Query<T>(int guid) where T : Api.iNetwork.iTcpSocket
        {
            throw new System.NotImplementedException();
        }

        public void Resume()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            NetEvent ev = null;
            while(_EventQueue.Pull(out ev))
            {
                switch(ev._Type)
                {
                    case eTcpEvent.Connect:
                        {
                            if (ev._CRet != null)
                            {
                                TcpConnection con = (ev._S as TcpConnection);
                                ev._CRet(con);
                                con.OnConnect(true);
                            }
                        }
                        break;
                    case eTcpEvent.Disconnect:
                        {
                            TcpConnection con = (ev._S as TcpConnection);
                            con.OnDisconnect();
                            TcpConnection.Release(con);
                        }
                        break;
                    case eTcpEvent.Recv:
                        {

                        }
                        break;
                    default:
                        break;
                }
                s_EventPool.Return(ev);
            }
        }
    }
}