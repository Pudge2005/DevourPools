using System.Collections.Generic;
using UnityEngine;

namespace DevourDev.Pools.Tests
{
    public sealed class ThreadSafeLogger : MonoBehaviour
    {
        [SerializeField] private List<string> _logs;

        private static readonly Queue<string> _logsQ = new();
        private static readonly object _lockObj = new();


        private void Update()
        {
            lock (_lockObj)
            {
                while (_logsQ.TryDequeue(out var msg))
                {
                    _logs.Add(msg);
                }
            }
        }

        public static void EnqueueLog(string msg)
        {
            lock (_lockObj)
            {
                _logsQ.Enqueue(msg);
            }
        }
    }

}
