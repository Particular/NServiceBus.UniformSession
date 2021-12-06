namespace NServiceBus.UniformSession.Testing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Testing;

    public class TestableUniformSession : IUniformSession
    {
        TestablePipelineContext innerContext;
        TestableMessageSession innerSession;

        public TestableUniformSession() : this(new TestablePipelineContext())
        {
        }

        public TestableUniformSession(TestableMessageSession innerSession)
        {
            this.innerSession = innerSession;
        }

        public TestableUniformSession(TestablePipelineContext innerContext)
        {
            this.innerContext = innerContext;
        }

        public SentMessage<object>[] SentMessages => innerContext?.SentMessages ?? innerSession.SentMessages;

        public PublishedMessage<object>[] PublishedMessages => innerContext?.PublishedMessages ?? innerSession.PublishedMessages;

        Task IUniformSession.Send(object message, SendOptions options, CancellationToken cancellationToken)
            => innerContext?.Send(message, options) ?? innerSession.Send(message, options, cancellationToken);

        Task IUniformSession.Send<T>(Action<T> messageConstructor, SendOptions options, CancellationToken cancellationToken)
            => innerContext?.Send(messageConstructor, options) ?? innerSession.Send(messageConstructor, options, cancellationToken);

        Task IUniformSession.Publish(object message, PublishOptions options, CancellationToken cancellationToken)
            => innerContext?.Publish(message, options) ?? innerSession.Publish(message, options, cancellationToken);

        Task IUniformSession.Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken)
            => innerContext?.Publish(messageConstructor, publishOptions) ?? innerSession.Publish(messageConstructor, publishOptions, cancellationToken);
    }
}
