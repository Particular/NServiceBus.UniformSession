using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.UniformSession;

class MessageSession : IUniformSession, IDisposable
{
    public MessageSession(IMessageSession messageSession, CurrentSessionHolder sessionHolder)
    {
        this.sessionHolder = sessionHolder;
        this.messageSession = messageSession;
    }

    public Task Send(object message, SendOptions options)
    {
        ThrowIfDisposed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfDisposed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfDisposed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfDisposed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(messageConstructor, publishOptions);
    }

    public void Dispose()
    {
        isDisposed = true;
    }

    void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new InvalidOperationException(AccessDisposedSessionExceptionMessage);
        }
    }

    void ThrowIfAbusedInsidePipeline()
    {
        if (sessionHolder.ActiveSessionFromPipeline)
        {
            throw new InvalidOperationException(SessionUsedInsidePipelineExceptionMessage);
        }
    }

    readonly IMessageSession messageSession;

    bool isDisposed;
    static readonly string AccessDisposedSessionExceptionMessage = $"This session has been disposed and can no longer send messages. Ensure to not cache instances {nameof(IUniformSession)}.";
    static readonly string SessionUsedInsidePipelineExceptionMessage = $"This session is being used inside the message handling pipeline which can lead to message loss. Ensure to not cache instances {nameof(IUniformSession)}.";
    CurrentSessionHolder sessionHolder;
}