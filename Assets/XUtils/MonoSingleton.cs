using UnityEngine;

namespace XUtils
{
    using UnityEngine;

    // ���͵����࣬�̳��� MonoBehaviour
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // ��̬˽��ʵ����ȷ��ֻ��һ��ʵ��
        private static T instance;

        // ���о�̬���ԣ����ڷ��ʵ���ʵ��
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    // �����ڳ����в��Ҹ����͵�ʵ��
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        // ���������û�У�����һ���µ� GameObject ����Ӹ����
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                // ȷ���ڳ����л�ʱ�������󲻻ᱻ����
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // ����Ѿ�����ʵ�������ٵ�ǰ����
                if (this != instance)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
