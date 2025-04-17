using UnityEngine;

namespace XEngine {
    class Network : Api.iNetwork
    {
        public Api.iNetwork.iTcpConnection CreateTcpConnection(string host, int port)
        {
            throw new System.NotImplementedException();
        }

        public Api.iNetwork.iTcpServer LaunchTcpServer(string host, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Pause()
        {
            throw new System.NotImplementedException();
        }

        public void Resume()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}