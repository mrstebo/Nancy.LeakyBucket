using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nancy.LeakyBucket.Identifiers;

namespace Nancy.LeakyBucket.Store
{
    public class DefaultRequestStore : IRequestStore
    {
        private readonly int _maxNumberOfRequests;
        private readonly object _syncObject = new object();
        private readonly IDictionary<IClientIdentifier, Queue<DateTime>> _requests =
            new ConcurrentDictionary<IClientIdentifier, Queue<DateTime>>();

        public DefaultRequestStore(int maxNumberOfRequests)
        {
            _maxNumberOfRequests = maxNumberOfRequests;
        }

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
                    _requests.Add(identifier, new Queue<DateTime>(_maxNumberOfRequests));
                if (_requests[identifier].Count == _maxNumberOfRequests)
                    _requests[identifier].Dequeue();
                _requests[identifier].Enqueue(dateTime);
            }
        }

        public void DeleteRequestsOlderThan(IClientIdentifier identifier, DateTime expiryDate)
        {
            lock (_syncObject)
            {
                if (_requests.ContainsKey(identifier))
                {
                    while (HasExpired(identifier, expiryDate))
                    {
                        _requests[identifier].Dequeue();
                    }
                }
                DeleteEmptyRequestsData(identifier);
            }
        }
        
        private void DeleteEmptyRequestsData(IClientIdentifier identifier)
        {
            if (_requests.ContainsKey(identifier) && _requests[identifier].Count == 0)
                _requests.Remove(identifier);
        }

        private bool HasExpired(IClientIdentifier identifier, DateTime expiryDate)
        {
            return _requests[identifier].Count > 0 && _requests[identifier].Peek() < expiryDate;
        }
    }
}
