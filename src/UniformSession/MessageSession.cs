using System;
using System.Threading;
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

    public Task Send(object message, SendOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(message, options, cancellationToken);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Send(messageConstructor, options, cancellationToken);
    }

    public Task Publish(object message, PublishOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(message, options, cancellationToken);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        ThrowIfAbusedInsidePipeline();

        return messageSession.Publish(messageConstructor, publishOptions, cancellationToken);
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

    static readonly string NotCached = $@"Instances of '{nameof(IUniformSession)}' should not be cached, in example by 
 - Injecting into a singleton
 - Injecting into an instance with a custom container scope that exceeds the lifetime of an endpoint
 - Rebinding on another container that exceeds the lifetime of the endpoint
 - Assigning to a static or thread static field or property
";

    static readonly string AccessClosedSessionExceptionMessage = $@"The endpoint owning this '{nameof(IUniformSession)}' instance has been stopped, so it is no longer possible to execute message operations. {NotCached}";
    static readonly string SessionUsedInsidePipelineExceptionMessage = $"This '{nameof(IUniformSession)}' instance belongs to an endpoint and cannot be used in the message handling pipeline. Usage of this '{nameof(IUniformSession)}' instance within a pipeline can lead to message duplication. {NotCached}";
}