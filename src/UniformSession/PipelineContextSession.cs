using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.UniformSession;

class PipelineContextSession : IUniformSession
{
    public PipelineContextSession(IPipelineContext pipelineContext)
    {
        ThrowIfUnusable();

        this.pipelineContext = pipelineContext;
    }

    public void MarkAsUnusable()
    {
        unusable = true;
    }

    public Task Send(object message, SendOptions options)
    {
        ThrowIfUnusable();

        return pipelineContext.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfUnusable();

        return pipelineContext.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfUnusable();

        return pipelineContext.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfUnusable();

        return pipelineContext.Publish(messageConstructor, publishOptions);
    }

    void ThrowIfUnusable()
    {
        if (unusable)
        {
            throw new InvalidOperationException(AccessUnusableSessionExceptionMessage);
        }
    }

    readonly IPipelineContext pipelineContext;

    bool unusable;
    static readonly string AccessUnusableSessionExceptionMessage = $"This session has been marked as no longer usable and can no longer send messages. Ensure to not cache instances {nameof(IUniformSession)}.";
}