using System;
using System.Collections.Generic;
using UnityEngine;

namespace RudderStack.Unity
{
    internal class UnityMainThread : MonoBehaviour
    {
        internal static  UnityMainThread _Wkr;
        private readonly Queue<Action>   _jobs = new();

        private void Awake()
        {
            _Wkr = this;
        }

        private void Update()
        {
            while (_jobs.Count > 0)
                _jobs.Dequeue().Invoke();
        }

        internal void AddJob(Action newJob)
        {
            _jobs.Enqueue(newJob);
        }
    }
}