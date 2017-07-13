using System;
using Nancy.Bootstrapper;

namespace Nancy.LeakyBucket
{
    public static class LeakyBucketRateLimiter
    {
        public static void Enable(IPipelines pipelines, LeakyBucketRateLimiterConfiguration configuration)
        {
            if (pipelines == null)
                throw new ArgumentNullException(nameof(pipelines));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            pipelines.BeforeRequest.AddItemToStartOfPipeline(CheckRequestCount(configuration));
        }

        public static void Enable(INancyModule module, LeakyBucketRateLimiterConfiguration configuration)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            module.Before.AddItemToStartOfPipeline(CheckRequestCount(configuration));
        }

        private static Func<NancyContext, Response> CheckRequestCount(LeakyBucketRateLimiterConfiguration configuration)
        {
            return context =>
            {
                var identifier = configuration.ClientIdentifierFunc(context);
                var store = configuration.RequestStore;
                var config = new LeakyBucketContainerConfiguration
                {
                    RefreshRate = configuration.RefreshRate,
                    Limit = configuration.MaxNumberOfRequests
                };
                var container = new LeakyBucketContainer(store, config);

                return container.RequestsRemaining(identifier) > 0
                    ? null
                    : new Response().WithStatusCode(HttpStatusCode.TooManyRequests);
            };
        }
    }
}
