﻿using Sample.Sdk.Data.Registration;

namespace SampleSdkRuntime.Providers.Registration
{
    internal interface IServiceRegistrationProvider
    {
        Task<(bool isValid, ServiceRegistration reg)> GetServiceRegistration(string appId, CancellationToken token);
    }
}