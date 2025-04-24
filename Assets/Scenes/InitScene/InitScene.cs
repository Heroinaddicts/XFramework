using System;
using System.Threading;
using UnityEngine;
using XEngine;
using XUtils;

public class InitScene : MonoBehaviour
{
    static int _LastValue = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Api.iNetwork.iTcpServer ts = Api.GetNetwork().LaunchTcpServer("0.0.0.0", 12345);
        ts.SetOnConnectionCallback((Api.iNetwork.iTcpServer server, Api.iNetwork.iTcpConnection con) =>
        {
            Debug.Log($"TcpConnection {con.Guid} OnConnection");
            con.SetReceiveCallback((Api.iNetwork.iTcpConnection c, byte[] data, int offset, int size) =>
            {
                Debug.Log("OnReceive");
                return 0;
            });

            con.SetDisconnectCallback((Api.iNetwork.iTcpConnection c) =>
            {
                Debug.Log($"TcpConnection {c.Guid} OnDisconnect");
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        Api.GetNetwork().CreateTcpConnection("127.0.0.1", 23456, (Api.iNetwork.iTcpConnection con) =>
        {
            if (null != con)
            {
                Debug.Log($"TcpConnection {con.Guid} OnConnect");
            }
        });
    }

    public void OnClick()
    {
        // Load the main scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
