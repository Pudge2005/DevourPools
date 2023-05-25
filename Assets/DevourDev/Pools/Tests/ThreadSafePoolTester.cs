using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DevourDev.Pools.Tests
{
    internal sealed class ThreadSafePoolTester : MonoBehaviour
    {
        [SerializeField] private float _countDownTime = 20f;
        [SerializeField] private int _tasksCount = 50;
        [SerializeField] private int _iterationsCount = 50_000;


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);


        private async void Start()
        {
            await StartTest();
        }

        private void Update()
        {
            if (_countDownTime > 0)
                _countDownTime -= Time.deltaTime;

            //GC.Collect();
        }

        private async Task StartTest()
        {
            var pool = new ApplesPool(_iterationsCount * 10, 0);

            var tasks = new Task[_tasksCount];

            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.Register(OnCancelled);

            System.Action action = () => DoWork(pool, _iterationsCount, token);


            for (int i = 0; i < _tasksCount; i++)
            {
                tasks[i] = new Task(action);
            }

            foreach (var t in tasks)
            {
                t.Start();
            }

            cts.CancelAfter((int)(_countDownTime * 1000));

            ThreadSafeLogger.EnqueueLog($"tasks started");

            var waitingForAllTasksFinished = Task.WhenAll(tasks);
            _ = waitingForAllTasksFinished.ContinueWith(OnAllTasksFinished);
            await waitingForAllTasksFinished;

            foreach (var t in tasks)
            {
                t.Dispose();
            }
        }

        private void OnAllTasksFinished(Task task)
        {
            ThreadSafeLogger.EnqueueLog($"all tasks finished");
        }

        private void OnCancelled()
        {
            MessageBox(IntPtr.Zero, "Cancelled хуй", "жопа", 0);
        }

        private static void DoWork<T>(IPool<T> pool, int iterationsCount, CancellationToken token)
            where T : class
        {
            var factory = Task.Factory;

            ThreadSafeLogger.EnqueueLog($"Factory: {factory}, " +
                $"continuation options: {factory.ContinuationOptions}, " +
                $"creation options: {factory.CreationOptions}," +
                $"token: {factory.CancellationToken}; " +
                $"task id: {Task.CurrentId}, " +
                $"thread id: {Environment.CurrentManagedThreadId}, " +
                $"processor id: {Thread.GetCurrentProcessorId()}");

            var thread = Thread.CurrentThread;

            ThreadSafeLogger.EnqueueLog($"Thread: {thread}, " +
                $"ManagedThreadId: {thread.ManagedThreadId}," +
                $"Name: {thread.Name}," +
                $"Priority: {thread.Priority}," +
                $"ThreadState: {thread.ThreadState}");

            if (token.IsCancellationRequested)
                goto Cancelled;

            List<T> list = new(iterationsCount);

            for (int i = 0; i < iterationsCount; i++)
            {
                if (token.IsCancellationRequested)
                    goto Cancelled;

                list.Add(pool.Rent());
            }

            for (int i = 0; i < iterationsCount; i++)
            {
                if (token.IsCancellationRequested)
                    goto Cancelled;

                pool.Return(list[^1]);
                list.RemoveAt(list.Count - 1);
            }

            ThreadSafeLogger.EnqueueLog($"Word finished: {Task.CurrentId}, " +
                $"{Environment.CurrentManagedThreadId}, " +
                $"{Thread.GetCurrentProcessorId()}");
            return;

        Cancelled:
            ThreadSafeLogger.EnqueueLog($"Word cancelled: {Task.CurrentId}, " +
                $"{Environment.CurrentManagedThreadId}, " +
                $"{Thread.GetCurrentProcessorId()}");
        }
    }
}
