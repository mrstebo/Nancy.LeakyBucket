# Nancy.LeakyBucket
A request limiter based on the Leaky Bucket algorithm

[![Build status](https://ci.appveyor.com/api/projects/status/nc82bmu7i0ac7cpf/branch/master?svg=true)](https://ci.appveyor.com/project/mrstebo/nancy-leakybucket/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/mrstebo/Nancy.LeakyBucket/badge.svg?branch=master)](https://coveralls.io/github/mrstebo/Nancy.LeakyBucket?branch=master)
[![NuGet](http://img.shields.io/nuget/v/Nancy.LeakyBucket.svg?style=flat)](https://www.nuget.org/packages/Nancy.LeakyBucket/)

This package is available via install the [NuGet](https://www.nuget.org/packages/Nancy.LeakyBucket):

```powershell
Install-Package Nancy.LeakyBucket
```

Then, you can enable LeakyBucket rate limiting by enabling the `LeakyBucketRateLimiter` in your `Bootstrapper` class:

```cs
public class Bootstrapper : DefaultNancyBootstrapper
{
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
        LeakyBucketRateLimiter.Enable(pipelines, new LeakyBucketRateLimiterConfiguration
        {
            MaxNumberOfRequests = 100,
            RefreshRate = TimeSpan.FromSeconds(1)
        });

        base.ApplicationStartup(container, pipelines);
    }
}
```

## How it works

1. When a request comes in it will extract the `IClientIdentifier` from the `NancyContext` with the function specified in the configuration.
2. It will check for any requests that have passed the specified `RefreshRate` and remove them from the `IRequestStore`.
3. It will then add the new request to the `IRequestStore`.
4. It will check how many requests are remaining.
5. If it exceeds the `MaxNumberOfRequests` then it will return a **429 Too Many Requests** status code, otherwise it will continue on with the request.


## Configuration

The `LeakyBucketRateLimiterConfiguration` can specifiy an `IRequestStore`, that is set to the `DefaultRequestStore` by default, which is responsible for holding the current number of requests for each client.

The `LeakyBucketRateLimiterConfiguration` can specifies a function that determines what a client is. By default it creates a `DefaultClientIdentifier` that holds a reference to the remote address.
