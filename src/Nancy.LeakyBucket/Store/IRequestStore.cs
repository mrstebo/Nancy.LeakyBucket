using System;
using Nancy.LeakyBucket.Identifiers;

namespace Nancy.LeakyBucket.Store
{
    public interface IRequestStore
    {
        int NumberOfRequestsFor(IClientIdentifier identifier);
        void AddRequest(IClientIdentifier identifier, DateTime dateTime);
        void DeleteRequestsOlderThan(IClientIdentifier identifier, DateTime expiryDate);
    }
}
