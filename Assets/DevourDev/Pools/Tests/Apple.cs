using System;

namespace DevourDev.Pools.Tests
{
    public sealed class Apple
    {
        public const int DataSize = 512;

        private static readonly object _rngLock = new();
        private static readonly System.Random _rng = new();
        private readonly byte[] _data;


        public Apple()
        {
            _data = new byte[DataSize];
            byte fillValue = 0;

            lock (_rngLock)
            {
                fillValue = (byte)_rng.Next(0, 255);
            }

            _data.AsSpan().Fill(fillValue);
        }


        public string Name { get; set; }
        public double Weight { get; set; }
    }

}
