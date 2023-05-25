using Unity.Jobs;
using UnityEngine;

namespace DevourDev.Pools.Tests
{
    public sealed class ThreadSafePoolWithJobsTests : MonoBehaviour
    {
        private struct CreateApplesJob : IJob
        {
            public int IterationsCount;


            public readonly void Execute()
            {
                var pool = _pool;

                for (int i = 0; i < IterationsCount; i++)
                {
                    var rentedApple = pool.Rent();
                    pool.Return(rentedApple);
                }
            }
        }


        [SerializeField] private int _jobsCount = 5000;
        [SerializeField] private int _iterationsCount = 10_000;
        [SerializeField] private bool _useJobs;


        private static ApplesPool _pool;


        private void Start()
        {
            _pool = new ApplesPool(_iterationsCount * 100, 0);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (_useJobs)
            {
                DoWithJobs();
            }
            else
            {
                DoSync();
            }

            sw.Stop();

            UnityEngine.Debug.Log("all finished");
            UnityEngine.Debug.Log($"{_pool.TotalRents}");
            UnityEngine.Debug.Log($"{sw.Elapsed.TotalMilliseconds} ms ({sw.Elapsed.Ticks} ticks)");
        }

        private void DoSync()
        {
            var pool = _pool;

            for (int i = 0; i < _jobsCount; i++)
            {
                for (int j = 0; j < _iterationsCount; j++)
                {
                    var rented = pool.Rent();
                    pool.Return(rented);
                }
            }
        }

        private void DoWithJobs()
        {
            JobHandle[] handles = new JobHandle[_jobsCount];

            for (int i = 0; i < _jobsCount; i++)
            {
                handles[i] = CreateJob().Schedule();
            }

            foreach (var handle in handles)
            {
                handle.Complete();
            }
        }

        private CreateApplesJob CreateJob()
        {
            CreateApplesJob job = new()
            {
                IterationsCount = _iterationsCount
            };

            return job;
        }
    }

}
