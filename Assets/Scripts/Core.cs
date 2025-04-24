using UnityEngine;
using XEngine;
using XUtils;

public class Core : MonoSingleton<Core>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.runInBackground = true;
    }
    // Update is called once per frame
    void Update()
    {
        Api.GetNetwork().Update();
    }
}
