using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.UniformSession;

class PipelineContextSession : IUniformSession
{
    public PipelineContextSession(IPipelineContext pipelineContext)
    {
        ThrowIfClosed();

        this.pipelineContext = pipelineContext;
    }

    public Task Send(object message, SendOptions options)
    {
        ThrowIfClosed();

        return pipelineContext.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        ThrowIfClosed();

        return pipelineContext.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        ThrowIfClosed();

        return pipelineContext.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        ThrowIfClosed();

        return pipelineContext.Publish(messageConstructor, publishOptions);
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

    readonly IPipelineContext pipelineContext;

    bool closed;
    static readonly string AccessClosedSessionExceptionMessage = $"This session instance belongs to a message handling pipeline that has already completed. It is no longer possible to execute message operations on this instance. Ensure to not cache instances of {nameof(IUniformSession)}.";
}