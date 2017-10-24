using System;
using System.Threading;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.UniformSession;

class CurrentSessionHolder
{
    public IUniformSession Current
    {
        get
        {
            if (pipelineContext.Value != null)
            {
                return pipelineContext.Value;
            }
            return messageSession;
        }
    }

    public IDisposable SetCurrentPipelineContext(IIncomingPhysicalMessageContext context)
    {
        if (pipelineContext.Value != null)
        {
            throw new InvalidOperationException("Attempt to overwrite an existing pipeline context in BusSession.Current.");
        }

        pipelineContext.Value = new PipelineContextSession(context);
        return new ContextSCope(this);
    }

    public IDisposable SetMessageSession(IMessageSession session)
    {
        if (messageSession != null)
        {
            throw new InvalidOperationException("Attempt to overwrite an existing message session in BusSession.Current.");
        }

        messageSession = new MessageSession(session);
        return new SessionScope(this);
    }

    readonly AsyncLocal<PipelineContextSession> pipelineContext = new AsyncLocal<PipelineContextSession>();
    MessageSession messageSession;

    class ContextSCope : IDisposable
    {
        public ContextSCope(CurrentSessionHolder sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        public void Dispose()
        {
            sessionHolder.pipelineContext.Value.Dispose();
            sessionHolder.pipelineContext.Value = null;
        }

        readonly CurrentSessionHolder sessionHolder;
    }

    class SessionScope : IDisposable
    {
        public SessionScope(CurrentSessionHolder sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        public void Dispose()
        {
            sessionHolder.messageSession.Dispose();
            sessionHolder.messageSession = null;
        }

        readonly CurrentSessionHolder sessionHolder;
    }
}