using System;
using Nancy.LeakyBucket.Identifiers;
using Nancy.LeakyBucket.Store;

namespace Nancy.LeakyBucket
{
    public class LeakyBucketRateLimiterConfiguration
    {
        public TimeSpan RefreshRate { get; set; } = TimeSpan.FromSeconds(1);
        public int MaxNumberOfRequests { get; set; } = 10;
        public IRequestStore RequestStore { get; set; }
        public Func<NancyContext, IClientIdentifier> ClientIdentifierFunc { get; set; } = DefaultClientIdentifierFunc();

        public LeakyBucketRateLimiterConfiguration()
        {
            RequestStore = RequestStore ?? new DefaultRequestStore(MaxNumberOfRequests);
        }

        private static Func<NancyContext, IClientIdentifier> DefaultClientIdentifierFunc()
        {
            return ctx => new DefaultClientIdentifier {UserAgentAddress = ctx.Request.UserHostAddress};
        }
    }
}
