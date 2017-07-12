using System;
using Nancy.LeakyBucket.Identifiers;
using Nancy.LeakyBucket.Internal;
using Nancy.LeakyBucket.Store;

namespace Nancy.LeakyBucket
{
    public interface ILeakyBucketContainer
    {
        int RequestsRemaining(IClientIdentifier identifier);
    }

    internal class LeakyBucketContainer : ILeakyBucketContainer
    {
        private readonly IRequestStore _requestStore;
        private readonly LeakyBucketContainerConfiguration _configuration;
        private readonly ISystemClock _systemClock;
        

        public LeakyBucketContainer(
            IRequestStore requestStore, 
            LeakyBucketContainerConfiguration configuration,
            ISystemClock systemClock = null)
        {
            if (requestStore == null)
                throw new ArgumentNullException(nameof(requestStore));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _requestStore = requestStore;
            _configuration = configuration;
            _systemClock = systemClock ?? new SystemClock();
        }

        public int RequestsRemaining(IClientIdentifier identifier)
        {
            UpdateRequestCount(identifier);

            var numberOfRequests = GetNumberOfRequests(identifier);
            var remaining = _configuration.Limit - numberOfRequests;

            return remaining;
        }
        
        private void UpdateRequestCount(IClientIdentifier identifier)
        {
            var expiryDate = _systemClock.UtcNow.Subtract(_configuration.RefreshRate);

            _requestStore.DeleteRequestsOlderThan(identifier, expiryDate);
            _requestStore.AddRequest(identifier, _systemClock.UtcNow);
        }

        private int GetNumberOfRequests(IClientIdentifier identifier)
        {
            return _requestStore.NumberOfRequestsFor(identifier);
        }
    }
}
