namespace NServiceBus.UniformSession.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;
    using UniformSession;

    [TestFixture]
    public class APIApprovals
    {
#if NETFRAMEWORK
        [Test]
        public void ApproveNServiceBus()
        {
            var publicApi = Filter(ApiGenerator.GeneratePublicApi(typeof(IUniformSession).Assembly));
            Approver.Verify(publicApi, scenario: "netframework");
        }
#endif

#if NETCOREAPP
        [Test]
        public void ApproveNServiceBus()
        {
            var publicApi = Filter(ApiGenerator.GeneratePublicApi(typeof(IUniformSession).Assembly));
            Approver.Verify(publicApi, scenario: "netstandard");
        }
#endif

        string Filter(string text)
        {
            return string.Join(Environment.NewLine, text.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.StartsWith("[assembly: ReleaseDateAttribute("))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                );
        }
    }
}