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
        ThrowIfUnusable();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfUnusable();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfUnusable();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfUnusable();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(messageConstructor, publishOptions);
    }

    public void MarkAsUnusable()
    {
        unusable = true;
    }

    void ThrowIfUnusable()
    {
        if (unusable)
        {
            throw new InvalidOperationException(AccessUnusableSessionExceptionMessage);
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

    bool unusable;
    static readonly string AccessUnusableSessionExceptionMessage = $"This session has been marked as no longer usable and can no longer send messages. Ensure to not cache instances {nameof(IUniformSession)}.";
    static readonly string SessionUsedInsidePipelineExceptionMessage = $"This session is being used inside the message handling pipeline which can lead to message loss. Ensure to not cache instances {nameof(IUniformSession)}.";
    CurrentSessionHolder sessionHolder;
}