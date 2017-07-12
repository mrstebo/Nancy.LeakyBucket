using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nancy.LeakyBucket.Identifiers;

namespace Nancy.LeakyBucket.Store
{
    public class DefaultRequestStore : IRequestStore
    {
        private readonly object _syncObject = new object();
        private readonly IDictionary<IClientIdentifier, List<DateTime>> _requests =
            new ConcurrentDictionary<IClientIdentifier, List<DateTime>>();

        public int NumberOfRequestsFor(IClientIdentifier identifier)
        {
            lock (_syncObject)
            {
                return _requests.ContainsKey(identifier)
                    ? _requests[identifier].Count
                    : 0;
            }
        }

        public void AddRequest(IClientIdentifier identifier, DateTime dateTime)
        {
            lock (_syncObject)
            {
                if (!_requests.ContainsKey(identifier))
                    _requests.Add(identifier, new List<DateTime>());
                _requests[identifier].Add(dateTime);
            }
        }

        public void DeleteRequestsOlderThan(IClientIdentifier identifier, DateTime expiryDate)
        {
            lock (_syncObject)
            {
                if (_requests.ContainsKey(identifier))
                    _requests[identifier].RemoveAll(x => CanBeRemoved(x, expiryDate));
                DeleteEmptyRequestsData(identifier);
            }
        }
        
        private void DeleteEmptyRequestsData(IClientIdentifier identifier)
        {
            if (_requests.ContainsKey(identifier) && _requests[identifier].Count == 0)
                _requests.Remove(identifier);
        }

        private static bool CanBeRemoved(DateTime createdAt, DateTime expiryDate)
        {
            return createdAt.Subtract(expiryDate).TotalMilliseconds < 0;
        }
    }
}
