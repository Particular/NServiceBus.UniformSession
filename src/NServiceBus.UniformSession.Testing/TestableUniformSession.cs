namespace NServiceBus.UniformSession.Testing
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Testing;

    public class TestableUniformSession : IUniformSession
    {
        TestableMessageHandlerContext inner;

        TestableMessageHandlerContext GetInnerContext() =>
            inner ??
            throw new Exception(
                "TestableUniformSession not initialized. Add a call to `Handle<T>.WithUniformSession()` or `Saga<T>.WithUniformSession()`.");

        internal void SetInner(TestableMessageHandlerContext context)
            => inner = context;

        Task IUniformSession.Send(object message, SendOptions options)
            => GetInnerContext().Send(message, options);

        Task IUniformSession.Send<T>(Action<T> messageConstructor, SendOptions options)
            => GetInnerContext().Send(messageConstructor, options);

        Task IUniformSession.Publish(object message, PublishOptions options)
            => GetInnerContext().Publish(message, options);

        Task IUniformSession.Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
            => GetInnerContext().Publish(messageConstructor, publishOptions);
    }
}
