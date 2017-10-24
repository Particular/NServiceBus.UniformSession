using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.UniformSession;

class MessageSession : IUniformSession, IDisposable
{
    public MessageSession(IMessageSession messageSession)
    {
        this.messageSession = messageSession;
    }

    public Task Send(object message, SendOptions options)
    {
        ThrowIfDisposed();
        return messageSession.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfDisposed();
        return messageSession.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfDisposed();
        return messageSession.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfDisposed();
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

    readonly IMessageSession messageSession;

    bool isDisposed;
    static readonly string AccessDisposedSessionExceptionMessage = $"This session has been disposed and can no longer send messages. Ensure to not cache instances {nameof(IUniformSession)}.";
}