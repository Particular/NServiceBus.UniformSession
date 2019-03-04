namespace NServiceBus.UniformSession.Tests
{
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;
    using UniformSession;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        public void ApproveNServiceBus()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(IUniformSession).Assembly, excludeAttributes: new[] { "System.Runtime.Versioning.TargetFrameworkAttribute" });
            Approver.Verify(publicApi);
        }
    }
}