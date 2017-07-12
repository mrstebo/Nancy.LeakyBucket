using System;
using FakeItEasy;
using Nancy.LeakyBucket.Identifiers;
using Nancy.LeakyBucket.Internal;
using Nancy.LeakyBucket.Store;
using NUnit.Framework;

namespace Nancy.LeakyBucket.Tests
{
    [TestFixture]
    [Parallelizable]
    public class LeakyBucketContainerTests
    {
        [Test]
        public void Should_throw_with_null_request_store_with_configuration()
        {
            var config = new LeakyBucketContainerConfiguration();

            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new LeakyBucketContainer(null, config));
        }

        [Test]
        public void Should_throw_with_null_configuration()
        {
            var requestStore = A.Fake<IRequestStore>();

            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new LeakyBucketContainer(requestStore, null));
        }

        [Test]
        public void Should_return_the_current_requests_remaining()
        {
            var identifier = A.Fake<IClientIdentifier>();
            var requestStore = A.Fake<IRequestStore>();
            var config = new LeakyBucketContainerConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                Limit = 10
            };
            var systemClock = A.Fake<ISystemClock>();
            var container = new LeakyBucketContainer(requestStore, config, systemClock);

            A.CallTo(() => requestStore.NumberOfRequestsFor(identifier)).Returns(2);
            A.CallTo(() => systemClock.UtcNow).Returns(new DateTime(2017, 1, 1, 12, 0, 0));

            var result = container.RequestsRemaining(identifier);

            Assert.AreEqual(8, result);
        }

        [Test]
        public void Should_make_call_to_delete_old_requests()
        {
            var requestStore = A.Fake<IRequestStore>();
            var config = new LeakyBucketContainerConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                Limit = 10
            };
            var systemClock = A.Fake<ISystemClock>();
            var container = new LeakyBucketContainer(requestStore, config, systemClock);
            var identifier = A.Fake<IClientIdentifier>();
            var now = new DateTime(2017, 1, 1, 12, 0, 0);
            var expiryDate = now.Subtract(config.RefreshRate);

            A.CallTo(() => systemClock.UtcNow).Returns(now);

            container.RequestsRemaining(identifier);

            A.CallTo(() => requestStore.DeleteRequestsOlderThan(identifier, expiryDate))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Should_make_call_to_add_a_new_request()
        {
            var requestStore = A.Fake<IRequestStore>();
            var config = new LeakyBucketContainerConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                Limit = 10
            };
            var systemClock = A.Fake<ISystemClock>();
            var container = new LeakyBucketContainer(requestStore, config, systemClock);
            var identifier = A.Fake<IClientIdentifier>();
            var now = new DateTime(2017, 1, 1, 12, 0, 0);

            A.CallTo(() => systemClock.UtcNow).Returns(now);

            container.RequestsRemaining(identifier);

            A.CallTo(() => requestStore.AddRequest(identifier, now))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
