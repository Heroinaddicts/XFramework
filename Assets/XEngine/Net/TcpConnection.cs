using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Codice.CM.Common;
using UnityEngine;
using XUtils;
using static PlasticPipe.PlasticProtocol.Client.ConnectionCreator.PlasticProtoSocketConnection;
using static XEngine.Api.iNetwork;
using static XEngine.NetDefine;

namespace XEngine
{
    class TcpConnection : Api.iNetwork.iTcpConnection
    {
        static XPool<TcpConnection> s_Pool = new XPool<TcpConnection>(
            () => new TcpConnection(),
            (TcpConnection t) =>
            {
                t._Socket = null;
                t._Guid = 0;
            },
            16
        );

        static IDictionary<UInt64, TcpConnection> s_Dictionary = new Dictionary<UInt64, TcpConnection>();

        Socket _Socket;
        UInt64 _Guid;
        readonly byte[] _RecvTemp = new byte[1024 * 64];
        Network _Network;
        Flag _IsSending = new Flag();
        Flag _IsReving = new Flag();

        //callbacks
        Action<bool, iTcpConnection> _ConnectCallback = null;
        Action<iTcpConnection> _DisconnectCallback = null;
        Func<iTcpConnection, byte[], int, int, int> _RecvCallback = null;

        object _Context = null;

        SPSCBuffer _RecvBuffer = new SPSCBuffer();
        SPSCBuffer _SendBuffer = new SPSCBuffer();

        public static TcpConnection Create(Socket s, UInt64 guid, Network nw)
        {
            TcpConnection con = s_Pool.Get();
            con._Socket = s;
            con._Guid = guid;
            con._Network = nw;
            con._RecvBuffer.Clear();
            con._SendBuffer.Clear();

            if (con.Initialize())
            {
                s_Dictionary.Add(guid, con);
                return con;
            }
            s.Close();
            s_Pool.Return(con);
            return null;
        }

        public static void Release(TcpConnection con)
        {
            s_Dictionary.Remove(con.Guid);

            con.ReleaseSocket();
            con._Guid = 0;
            con._Network = null;
            s_Pool.Return(con);
        }

        public static TcpConnection Query(UInt64 guid) {
            return s_Dictionary[guid];
        }

        private TcpConnection() {}

        void ReleaseSocket()
        {
            if (_Socket != null)
            {
                _Socket.Close();
                _Socket = null;
            }
        }

        private bool Initialize()
        {
            bool ret = false;
            try
            {
                _Socket.BeginReceive(_RecvTemp, 0, _RecvTemp.Length, SocketFlags.None, this.ReceiveCallback, this);
                _IsReving._V = true;
                ret = true;
            }
            catch (Exception e)
            {
                ReleaseSocket();
                Api.Error(e.ToString());
            }
            return ret;
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            bool isDisconnect = false;
            try
            {
                int bytesRead = _Socket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    _RecvBuffer.Write(_RecvTemp, 0, bytesRead);
                    _Network.PushEvent(eTcpEvent.Recv, this, CODE_SUCCESS);
                }
                else
                {
                    isDisconnect = true;
                }
            } catch(SocketException e) {
                isDisconnect = true;
            }

            if (isDisconnect)
            {
                ReleaseSocket();
                _IsReving._V = false;
                lock (_IsSending)
                {
                    if (!_IsSending)
                    {
                        _Network.PushEvent(eTcpEvent.Disconnect, this, CODE_UNKNOWN);
                    }
                }
                Api.Trace($"Connection {_Guid} closed");
            }
        }

        public void Close()
        {
            ReleaseSocket();
        }

        public void Send(byte[] data, int offset, int size)
        {
            _SendBuffer.Write(data, offset, size);
            AsyncSend();
        }

        void AsyncSendCallback(IAsyncResult ar)
        {
            bool isClose = false;
            try
            {
                int bytesSend = _Socket.EndSend(ar);
                if (bytesSend > 0)
                {
                    _SendBuffer.Out(bytesSend);
                    if (_SendBuffer.GetLength() > 0)
                    {
                        AsyncSend(true);
                    }
                    else
                    {
                        lock (_IsSending)
                        {
                            _IsSending = false;
                        }
                    }
                }
                else
                {
                    isClose = true;
                }
            }
            catch (SocketException ex)
            {
                isClose = true;
            }
            catch (ObjectDisposedException)
            {
                isClose = true;
            }
            catch (Exception ex)
            {
                isClose = true;
            }

            if (isClose)
            {
                if (!Volatile.Read(ref _IsReving))
                {
                    ReleaseSocket();
                    _Network.PushEvent(eTcpEvent.Disconnect, this, CODE_UNKNOWN);
                }
            }
        }

        void AsyncSend(bool through = false)
        {
            lock (_IsSending)
            {
                if (!_IsSending || through)
                {
                    try
                    {
                        if (_SendBuffer.GetLength() > 0)
                        {
                            _SendBuffer.Read(
                                (byte[] data, int offset, int len) =>
                                {
                                    _Socket.BeginSend(data, offset, len, SocketFlags.None, this.AsyncSendCallback, this);
                                    return len;
                                }
                            );
                            _IsSending = true;
                        }
                    }
                    catch(SocketException e)
                    {
                        Api.Error(e.ToString());
                        _IsSending = false;
                        if (!Volatile.Read(ref _IsReving))
                        {
                            ReleaseSocket();
                            _Network.PushEvent(eTcpEvent.Disconnect, this, CODE_UNKNOWN);
                        }
                    }
                }
            }
        }

        public void SetConnectCallback(Action<bool, iTcpConnection> callback)
        {
            _ConnectCallback = callback;
        }

        public void SetDisconnectCallback(Action<iTcpConnection> callback)
        {
            _DisconnectCallback = callback;
        }

        public void SetReceiveCallback(Func<byte[], int, int, int> callback)
        {
            throw new NotImplementedException();
        }

        public void DealRecv()
        {
            Int64 tick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (_RecvBuffer.Read(
                (byte[] data, int offset, int len) =>
                {
                    return _RecvCallback(this, data, offset, len);
                }
            )) {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - tick > 5)
                {
                    break;
                }
            }
        }
        public void OnConnect(bool ret)
        {
            if (null != _ConnectCallback)
            {
                _ConnectCallback(ret, this);
            }
        }

        public void OnDisconnect()
        {
            if (null != _DisconnectCallback)
            {
                _DisconnectCallback(this);
            }
        }

        public void SetReceiveCallback(Func<iTcpConnection, byte[], int, int, int> callback)
        {
            _RecvCallback = callback;
        }

        public void SetContext<T>(in T context)
        {
            _Context = context;
        }

        public T GetContext<T>()
        {
            return (T)_Context;
        }

        public UInt64 Guid
        {
            get { return _Guid; }
        }
    }
}

