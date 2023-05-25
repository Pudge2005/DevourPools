using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DevourDev.Pools.Tests
{
    public class ThreadSafePoolTests : MonoBehaviour
    {
        [SerializeField] private List<string> _logs;

        private static readonly Queue<string> _logsQ = new();
        private static readonly object _lockObj = new();


        private void Start()
        {
            var task = Task.Run(async () =>
            {
                await GarbageCollectorTest();
            });

            task.ContinueWith(HandleTaskFinished);
        }

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
            return;
            lock (_lockObj)
            {
                _logsQ.Enqueue(msg);
            }
        }

        private void HandleTaskFinished(Task task)
        {
            EnqueueLog($"Task finished!!!");
        }

        private async Task GarbageCollectorTest()
        {
            const int tasksCount = 500;
            const int itemsCount = 50_000;

            var tasks = new Task[tasksCount];

            for (int i = 0; i < tasksCount; i++)
            {
                tasks[i] = new Task(() => AllocateGarbage(itemsCount));
            }

            foreach (var t in tasks)
            {
                t.Start();
            }

            await Task.WhenAll(tasks);
            EnqueueLog($"from {nameof(GarbageCollectorTest)}: finished!");
        }

        private static void AllocateGarbage(int count)
        {
            List<Apple> list = new(count);
            ManipulateWithList(list);
        }

        private static void ManipulateWithList(List<Apple> list)
        {
            var arr = new Apple[list.Capacity];
            var span = new Span<Apple>(arr);

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = new();
            }

            list.AddRange(arr);
        }

        private static void AllocateGarbage(ConcurrentBag<Apple> bag, int count)
        {
            List<Apple> list = new(count);
            ManipulateWithList(list);

            foreach (var item in list)
            {
                bag.Add(item);
            }
        }





        public async Task ThreadSafePoolTest()
        {
            const int iterationsCount = 5_000_000;
            const int tasksCount = 500;
            const int timeoutDelay = 20_000;

            var pool = new ApplesPool(iterationsCount * 10, 0);

            await TestRentReturnAsync(pool, iterationsCount, tasksCount, timeoutDelay);
            EnqueueLog($"Total Rents: {pool.TotalRents}");
            pool.Clear();
        }


        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            EnqueueLog($"from unobserved: observed: {e.Observed}, msg: {e.Exception.Message}");
        }

        private async Task TestRentReturnAsync<T>(IPool<T> pool, int iterationsCount, int tasksCount,
            int timeoutDelay)
            where T : class
        {
            var tasks = new Task[tasksCount];
            using var cts = new CancellationTokenSource();

            for (int i = 0; i < tasksCount; i++)
            {
                tasks[i] = new Task(() => DoWork(pool, iterationsCount, cts.Token));
            }

            cts.CancelAfter(timeoutDelay);

            foreach (var t in tasks)
            {
                t.Start();
            }

            var allTasksFinishedBeforeCancellation = await WaitForAllOrCancellationAsync(cts.Token, tasks);

            EnqueueLog($"from {nameof(TestRentReturnAsync)}: " +
                $"{nameof(allTasksFinishedBeforeCancellation)}: " +
                $"{allTasksFinishedBeforeCancellation}");
        }


        /// <returns>False if was canceled before all tasks finished</returns>
        public static async Task<bool> WaitForAllOrCancellationAsync(CancellationToken token, params Task[] tasks)
        {
            try
            {
                var waitingForAll = Task.WhenAll(tasks);
                var waitingForCancellation = Task.Delay(Timeout.Infinite, token);

                Task first = await Task.WhenAny(waitingForAll, waitingForCancellation);

                if (first != waitingForCancellation)
                {
                    foreach (var t in tasks)
                    {
                        if (t.IsCanceled)
                            break;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                EnqueueLog("from wait for all or cancellation async: " + ex.Message);
            }

            return false;
        }


        private static void DoWork<T>(IPool<T> pool, int iterationsCount, CancellationToken token)
             where T : class
        {
            try
            {
                token.ThrowIfCancellationRequested();
                List<T> list = new(iterationsCount);
                RentRange(pool, list, iterationsCount, token);
                ReturnRange(pool, list, iterationsCount, token);
                RentRange(pool, list, iterationsCount, token);
                ReturnRange(pool, list, iterationsCount, token);
            }
            catch (Exception ex)
            {
                EnqueueLog("from do work: " + ex.Message);
            }
        }

        private static void Rent<T>(IPool<T> pool, List<T> list)
            where T : class
        {
            list.Add(pool.Rent());
        }

        private static void ReturnRange<T>(IPool<T> pool, List<T> list, int count, CancellationToken token)
            where T : class
        {
            for (int i = 0; !token.IsCancellationRequested && i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                Return(pool, list);
            }
        }

        private static void Return<T>(IPool<T> pool, List<T> list)
            where T : class
        {
            pool.Return(list[^1]);
            list.RemoveAt(list.Count - 1);
        }

        private static void RentRange<T>(IPool<T> pool, List<T> list, int count, CancellationToken token)
            where T : class
        {
            for (int i = 0; !token.IsCancellationRequested && i < count; i++)
            {
                Rent(pool, list);
            }
        }


    }

}
