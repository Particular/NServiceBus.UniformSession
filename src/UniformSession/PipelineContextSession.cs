using System;
using System.Threading;
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

    public Task Send(object message, SendOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();

        return pipelineContext.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();

        return pipelineContext.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();

        return pipelineContext.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken = default)
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

    static readonly string NotCached = $@"Instances of '{nameof(IUniformSession)}' should not be cached, in example by 
 - Injecting into a singleton
 - Injecting into an instance with a custom container scope that exceeds the lifetime of a message handling pipeline
 - Rebinding on another container that exceeds the lifetime of a message handling pipeline
 - Assigning to a static or thread static field or property
";

    static readonly string AccessClosedSessionExceptionMessage = $"The message handling pipeline owning this '{nameof(IUniformSession)}' instance has been completed, it is no longer possible to execute message operations. {NotCached}";
}