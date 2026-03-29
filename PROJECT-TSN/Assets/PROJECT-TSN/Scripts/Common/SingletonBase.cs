using UnityEngine;

namespace TST
{
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

    public class SingletonBase<T> : MonoBehaviour where T : class
    {
        public static T Singleton
        {
            get
            {
                return _instance.Value;
            }
        }

        // 지연 객체 초기화 방식
        private static readonly Lazy<T> _instance =
           new Lazy<T>(() =>
           {
               T instance = FindFirstObjectByType(typeof(T)) as T;

               if (instance == null)
               {
                   GameObject obj = new GameObject(typeof(T).ToString());
                   instance = obj.AddComponent(typeof(T)) as T;

#if UNITY_EDITOR
                   if (EditorApplication.isPlaying)
                   {
                       DontDestroyOnLoad(obj);
                   }
#else
                   DontDestroyOnLoad(obj);
#endif
               }

               return instance;
           });

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
