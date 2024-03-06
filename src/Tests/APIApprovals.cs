﻿namespace NServiceBus.UniformSession.Tests
{
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;
    using UniformSession;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        public void Approve()
        {
            var publicApi = typeof(IUniformSession).Assembly.GeneratePublicApi(new ApiGeneratorOptions
            {
                ExcludeAttributes = ["System.Runtime.Versioning.TargetFrameworkAttribute", "System.Reflection.AssemblyMetadataAttribute"]
            });
            Approver.Verify(publicApi);
        }
    }
}