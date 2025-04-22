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
    }

    // Update is called once per frame
    void Update()
    {
        Api.GetNetwork().CreateTcpConnection("baidu.com", 80, (con) =>
        {
            if (null != con)
            {
                con.SetConnectCallback((bool success, Api.iNetwork.iTcpConnection c) =>
                {
                    if (success)
                    {
                        _LastValue++;
                        Api.GetLog().Debug($"CreateTcpConnection {c.Guid} success, connection count {_LastValue}");
                    }
                    else
                    {
                        Api.GetLog().Error("CreateTcpConnection failed");
                    }
                });

                con.SetDisconnectCallback((Api.iNetwork.iTcpConnection c) =>
                {
                    _LastValue--;
                    Api.GetLog().Trace($"Connection {c.Guid} Disconnected, connection count {_LastValue}");
                });
            }
            else
            {
                Api.GetLog().Error("CreateTcpConnection failed");
            }
        });
    }

    public void OnClick()
    {
        // Load the main scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
