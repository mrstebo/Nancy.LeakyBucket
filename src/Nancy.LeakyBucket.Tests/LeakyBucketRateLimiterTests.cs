using System;
using FakeItEasy;
using Nancy.Bootstrapper;
using Nancy.LeakyBucket.Identifiers;
using Nancy.LeakyBucket.Store;
using Nancy.Testing;
using NUnit.Framework;

namespace Nancy.LeakyBucket.Tests
{
    [TestFixture]
    [Parallelizable]
    public class LeakyBucketRateLimiterTests
    {
        private static INancyBootstrapper CreateBootstrapper(LeakyBucketRateLimiterConfiguration configuration)
        {
            return new ConfigurableBootstrapper(config =>
            {
                config.ApplicationStartup((container, pipelines) =>
                {
                    LeakyBucketRateLimiter.Enable(pipelines, configuration);
                });
            });
        }

        [Test]
        public void Should_throw_with_null_pipelines_with_configuration()
        {
            var config = new LeakyBucketRateLimiterConfiguration();

            Assert.Throws<ArgumentNullException>(() => LeakyBucketRateLimiter.Enable((IPipelines) null, config));
        }

        [Test]
        public void Should_throw_with_null_module_with_configuration()
        {
            var config = new LeakyBucketRateLimiterConfiguration();

            Assert.Throws<ArgumentNullException>(() => LeakyBucketRateLimiter.Enable((INancyModule) null, config));
        }

        [Test]
        public void Should_throw_with_null_configuration()
        {
            var pipelines = A.Fake<IPipelines>();
            var module = A.Fake<INancyModule>();

            Assert.Throws<ArgumentNullException>(() => LeakyBucketRateLimiter.Enable(pipelines, null));
            Assert.Throws<ArgumentNullException>(() => LeakyBucketRateLimiter.Enable(module, null));
        }

        [Test]
        public void Should_add_before_request_hook_in_application_when_enabled()
        {
            var pipelines = A.Fake<IPipelines>();

            LeakyBucketRateLimiter.Enable(pipelines, new LeakyBucketRateLimiterConfiguration());

            A.CallTo(() => pipelines.BeforeRequest.AddItemToStartOfPipeline(A<Func<NancyContext, Response>>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Should_add_before_hook_in_module_when_enabled()
        {
            var module = A.Fake<INancyModule>();

            LeakyBucketRateLimiter.Enable(module, new LeakyBucketRateLimiterConfiguration());

            A.CallTo(() => module.Before.AddItemToStartOfPipeline(A<Func<NancyContext, Response>>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

        }

        [Test]
        public void Should_check_the_number_of_requests_using_the_request_store()
        {
            var store = A.Fake<IRequestStore>();
            var identifier = A.Fake<IClientIdentifier>();
            var config = new LeakyBucketRateLimiterConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                MaxNumberOfRequests = 10,
                RequestStore = store,
                ClientIdentifierFunc = _ => identifier
            };
            var bootstrapper = CreateBootstrapper(config);
            var browser = new Browser(bootstrapper);

            browser.Get("/", with => with.HttpRequest());

            A.CallTo(() => store.NumberOfRequestsFor(identifier))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Should_use_the_default_client_identifier_if_not_specified_in_config()
        {
            var store = A.Fake<IRequestStore>();
            var config = new LeakyBucketRateLimiterConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                MaxNumberOfRequests = 10,
                RequestStore = store
            };
            var bootstrapper = CreateBootstrapper(config);
            var browser = new Browser(bootstrapper);

            browser.Get("/", with =>
            {
                with.HttpRequest();
                with.UserHostAddress("test");
            });

            A.CallTo(() => store.NumberOfRequestsFor(
                    A<IClientIdentifier>.That.IsInstanceOf(typeof(DefaultClientIdentifier))))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => store.NumberOfRequestsFor(
                    A<IClientIdentifier>.That.Matches(x => ((DefaultClientIdentifier) x).UserAgentAddress == "test")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Should_not_return_too_many_requests_if_number_of_requests_is_below_configured_value()
        {
            var store = A.Fake<IRequestStore>();
            var config = new LeakyBucketRateLimiterConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                MaxNumberOfRequests = 10,
                RequestStore = store
            };
            var bootstrapper = CreateBootstrapper(config);
            var browser = new Browser(bootstrapper);

            A.CallTo(() => store.NumberOfRequestsFor(A<IClientIdentifier>.Ignored)).Returns(9);

            var response = browser.Get("/", with => with.HttpRequest());

            Assert.AreNotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        [Test]
        public void Should_return_too_many_requests_if_number_of_requests_exceeds_configured_value()
        {
            var identifier = A.Fake<IClientIdentifier>();
            var store = A.Fake<IRequestStore>();
            var config = new LeakyBucketRateLimiterConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(1),
                MaxNumberOfRequests = 10,
                RequestStore = store,
                ClientIdentifierFunc = _ => identifier
            };
            var bootstrapper = CreateBootstrapper(config);
            var browser = new Browser(bootstrapper);

            A.CallTo(() => store.NumberOfRequestsFor(A<IClientIdentifier>.Ignored)).Returns(11);

            var response = browser.Get("/", with => with.HttpRequest());

            Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
        
        [Test]
        public void Should_return_too_many_requests_when_request_limit_reached()
        {
            var config = new LeakyBucketRateLimiterConfiguration
            {
                RefreshRate = TimeSpan.FromSeconds(30),
                MaxNumberOfRequests = 4
            };
            var bootstrapper = CreateBootstrapper(config);
            var browser = new Browser(bootstrapper);
            var failedRequests = 0;

            for (var i = 0; i < 10; i++)
            {
                var response = browser.Get("/", with =>
                {
                    with.HttpRequest();
                    with.UserHostAddress("test");
                });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    failedRequests++;
            }
            
            Assert.AreEqual(7, failedRequests);
        }
    }
}
