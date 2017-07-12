using System;

namespace Nancy.LeakyBucket.Internal
{
    internal interface ISystemClock
    {
        DateTime UtcNow { get; }
    }


    internal class SystemClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
