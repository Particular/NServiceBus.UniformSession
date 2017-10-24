namespace NServiceBus.UniformSession.Tests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using NUnit.Framework;
    using PublicApiGenerator;
    using UniformSession;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveNServiceBus()
        {
            var publicApi = Filter(ApiGenerator.GeneratePublicApi(typeof(IUniformSession).Assembly));
            TestApprover.Verify(publicApi);
        }

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