using System;
using System.Threading.Tasks;

namespace NServiceBus.AmbientSession
{
    class PipelineContextSession : IBusSession, IDisposable
    {
        private static readonly string AccessDisposedSessionExceptionMessage = $"This session has been disposed and can no longer send messages. Ensure to not cache instances {nameof(IBusSession)}.";

        private readonly IPipelineContext pipelineContext;

        private bool isDisposed;

        public PipelineContextSession(IPipelineContext pipelineContext)
        {
            ThrowIfDisposed();

            this.pipelineContext = pipelineContext;
        }

        public Task Send(object message, SendOptions options)
        {
            ThrowIfDisposed();

            return pipelineContext.Send(message, options);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            ThrowIfDisposed();

            return pipelineContext.Send(messageConstructor, options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            ThrowIfDisposed();

            return pipelineContext.Publish(message, options);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            ThrowIfDisposed();

            return pipelineContext.Publish(messageConstructor, publishOptions);
        }

        public void Dispose()
        {
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new InvalidOperationException(AccessDisposedSessionExceptionMessage);
        }
    }
}