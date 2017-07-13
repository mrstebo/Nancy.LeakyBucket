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
