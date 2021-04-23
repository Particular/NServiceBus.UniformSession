namespace NServiceBus.UniformSession.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class ConventionTests
    {
        [Test]
        [TestCase(typeof(MessageSession))]
        [TestCase(typeof(PipelineContextSession))]
        public void Should_not_implement_disposable_due_containers_tracking_disposables(Type typeThatShouldNotImplementIDisposable)
        {
            Assert.IsFalse(typeof(IDisposable).IsAssignableFrom(typeThatShouldNotImplementIDisposable), $"{typeThatShouldNotImplementIDisposable} cannot implement {nameof(IDisposable)}. Container might be tracking disposables.");
        }
    }
}