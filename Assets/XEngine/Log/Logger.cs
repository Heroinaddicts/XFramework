namespace XEngine
{
    class Logger : Api.iLog
    {
        public void Debug(in string log)
        {
            UnityEngine.Debug.Log(log);
        }

        public void Trace(in string log)
        {
            UnityEngine.Debug.Log(log);
        }

        public void Error(in string log)
        {
            UnityEngine.Debug.LogError(log);
        }
    }
}