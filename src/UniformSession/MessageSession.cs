using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.UniformSession;

class MessageSession : IUniformSession
{
    public MessageSession(IMessageSession messageSession, CurrentSessionHolder sessionHolder)
    {
        this.sessionHolder = sessionHolder;
        this.messageSession = messageSession;
    }

    public Task Send(object message, SendOptions options)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(messageConstructor, publishOptions);
    }

    public void Close()
    {
        closed = true;
    }

    void ThrowIfClosed()
    {
        if (closed)
        {
            throw new InvalidOperationException(AccessClosedSessionExceptionMessage);
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

    bool closed;
    CurrentSessionHolder sessionHolder;
    static readonly string AccessClosedSessionExceptionMessage = $"The endpoint owning this session instance has been stopped. It is no longer possible to execute message operations on this instance. Ensure to not cache instances of {nameof(IUniformSession)}.";
    static readonly string SessionUsedInsidePipelineExceptionMessage = $"This session is being used inside the message handling pipeline this can lead to message duplication. Ensure to not cache instances of {nameof(IUniformSession)}.";
}