using UnityEngine;

namespace DevourDev.Unity.Pools.Tests
{
    public sealed class BallsSpawner : MonoBehaviour
    {
        [SerializeField] private BallsPool _pool;
        [SerializeField] private float _coolDown = 2f;

        [SerializeField] private Transform _min;
        [SerializeField] private Transform _max;

        private float _cdLeft;


        private void Update()
        {
            if ((_cdLeft -= Time.deltaTime) > 0)
                return;

            _cdLeft = _coolDown;
            var ball = _pool.Rent();
            ball.transform.position = GetSpawnPosition();
        }


        private Vector3 GetSpawnPosition()
        {
            Vector3 pos = default;
            var min = _min.position;
            var max = _max.position;

            for (int i = 0; i < 3; i++)
            {
                pos[i] = UnityEngine.Random.Range(min[i],
                    max[i]);
            }

            return pos;
        }
    }
}
