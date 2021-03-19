namespace NServiceBus.UniformSession.Testing
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Testing;

    public class TestableUniformSession : TestablePipelineContext, IUniformSession
    {
        TestableMessageHandlerContext inner;

        internal void SetInner(TestableMessageHandlerContext context)
            => inner = context;

        async Task IUniformSession.Send(object message, SendOptions options)
        {
            if (inner != null)
            {
                await inner.Send(message, options).ConfigureAwait(false);
            }
            await base.Send(message, options).ConfigureAwait(false);
        }

        async Task IUniformSession.Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            if (inner != null)
            {
                await inner.Send(messageConstructor, options).ConfigureAwait(false);
            }
            await base.Send(messageConstructor, options).ConfigureAwait(false);
        }

        async Task IUniformSession.Publish(object message, PublishOptions options)
        {
            if (inner != null)
            {
                await inner.Publish(message, options).ConfigureAwait(false);
            }
            await base.Publish(message, options).ConfigureAwait(false);
        }

        async Task IUniformSession.Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            if (inner != null)
            {
                await inner.Publish(messageConstructor, publishOptions).ConfigureAwait(false);
            }
            await base.Publish(messageConstructor, publishOptions).ConfigureAwait(false);
        }
    }
}
