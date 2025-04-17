using System.Threading;
using UnityEngine;
using XUtils;

public class InitScene : MonoBehaviour
{
    static int _LastValue = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var queue = new SpscQueue<int>();

        // Producer
        new Thread(() =>
        {
            for (int i = 0; i < 1_000_000; i++)
                queue.Enqueue(i);
        }).Start();

        // Consumer
        new Thread(() =>
        {
            int val;
            while (true)
            {
                while (queue.TryDequeue(out val))
                {
                    if (val - 1 != _LastValue)
                    {
                        Debug.LogError($"error {val}:{_LastValue}");
                    }else
                    {
                        Debug.Log($"success {val}:{_LastValue}");
                    }

                        _LastValue = val;
                }
            }
        }).Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        // Load the main scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
