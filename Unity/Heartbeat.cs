using System;
using System.Collections;
using UnityEngine;

namespace wLib.Promise
{
    public class Heartbeat : MonoBehaviour
    {
        private static Heartbeat _instance;

        public static Heartbeat Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Heartbeat");
                    _instance = go.AddComponent<Heartbeat>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        public static void RunCoroutine(IEnumerator target, Action onDone, Action<Exception> onFailed)
        {
            Instance.StartCoroutine(MonitorCoroutine(target, onDone, onFailed));
        }

        private static IEnumerator MonitorCoroutine(IEnumerator target, Action onDone, Action<Exception> onFailed)
        {
            while (true)
            {
                object current;
                try
                {
                    if (target.MoveNext() == false) { break; }

                    current = target.Current;
                }
                catch (Exception ex)
                {
                    onFailed.Invoke(ex);
                    yield break;
                }

                yield return current;
            }

            onDone.Invoke();
        }
    }
}