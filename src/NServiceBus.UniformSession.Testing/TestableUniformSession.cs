namespace NServiceBus.UniformSession.Testing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Testing;

    /// <summary>
    /// A testable implementation of <see cref="IUniformSession" />.
    /// </summary>
    public class TestableUniformSession : IUniformSession
    {
        readonly TestablePipelineContext innerContext;
        readonly TestableMessageSession innerSession;

        /// <summary>
        /// Creates a new instance of <see cref="TestableUniformSession" />.
        /// </summary>
        public TestableUniformSession() : this(new TestablePipelineContext())
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TestableUniformSession" />.
        /// </summary>
        /// <param name="innerSession">The <see cref="TestableMessageSession" /> the session should use to send and publish messages.</param>
        public TestableUniformSession(TestableMessageSession innerSession)
        {
            this.innerSession = innerSession;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TestableUniformSession" />.
        /// </summary>
        /// <param name="innerContext">The <see cref="TestablePipelineContext" /> the session should use to send and publish messages.</param>
        public TestableUniformSession(TestablePipelineContext innerContext)
        {
            this.innerContext = innerContext;
        }

        /// <summary>
        /// The messages that have been sent from the session.
        /// </summary>
        public SentMessage<object>[] SentMessages => innerContext?.SentMessages ?? innerSession.SentMessages;

        /// <summary>
        /// The messages that have been published from the session.
        /// </summary>
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
