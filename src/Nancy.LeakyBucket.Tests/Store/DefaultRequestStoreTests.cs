using System;
using FakeItEasy;
using Nancy.LeakyBucket.Identifiers;
using Nancy.LeakyBucket.Store;
using NUnit.Framework;

namespace Nancy.LeakyBucket.Tests.Store
{
    [TestFixture]
    [Parallelizable]
    public class DefaultRequestStoreTests
    {
        [SetUp]
        public void SetUp()
        {
            _requestStore = new DefaultRequestStore(10);
        }

        private IRequestStore _requestStore;

        [Test]
        public void Should_return_zero_if_no_requests_have_been_added_for_identifier()
        {
            var identifier = A.Fake<IClientIdentifier>();

            Assert.AreEqual(0, _requestStore.NumberOfRequestsFor(identifier));
        }

        [Test]
        public void Should_return_number_of_requests_for_identifier()
        {
            var identifier = A.Fake<IClientIdentifier>();

            _requestStore.AddRequest(identifier, DateTime.UtcNow);

            Assert.AreEqual(1, _requestStore.NumberOfRequestsFor(identifier));
        }

        [Test]
        public void Should_delete_requests_older_than_specified_period()
        {
            var identifier = A.Fake<IClientIdentifier>();
            var expiryDate = new DateTime(2017, 1, 2);

            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 1));
            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 2));
            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 3));

            _requestStore.DeleteRequestsOlderThan(identifier, expiryDate);

            Assert.AreEqual(2, _requestStore.NumberOfRequestsFor(identifier));
        }

        [Test]
        public void Should_delete_all_requests_if_they_have_all_expired()
        {
            var identifier = A.Fake<IClientIdentifier>();
            var expiryDate = new DateTime(2017, 1, 4);

            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 1));
            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 2));
            _requestStore.AddRequest(identifier, new DateTime(2017, 1, 3));

            _requestStore.DeleteRequestsOlderThan(identifier, expiryDate);

            Assert.AreEqual(0, _requestStore.NumberOfRequestsFor(identifier));
        }

        [Test]
        public void Should_not_surpass_the_max_number_of_requests_for_an_identifier()
        {
            const int maxNumberOfRequests = 5;
            var store = new DefaultRequestStore(maxNumberOfRequests);
            var identifier = A.Fake<IClientIdentifier>();

            for (var i = 0; i < maxNumberOfRequests + 5; i++)
                store.AddRequest(identifier, DateTime.UtcNow);

            Assert.AreEqual(maxNumberOfRequests, store.NumberOfRequestsFor(identifier));
        }
    }
}
