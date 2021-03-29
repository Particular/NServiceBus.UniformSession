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
        public void Approve()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(IUniformSession).Assembly, excludeAttributes: new[]
            {
                "System.Runtime.Versioning.TargetFrameworkAttribute", "System.Reflection.AssemblyMetadataAttribute"
            });
            Approver.Verify(publicApi);
        }
    }
}