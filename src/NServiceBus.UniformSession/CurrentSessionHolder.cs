using System;
using System.Threading;
using NServiceBus.Pipeline;

namespace NServiceBus.AmbientSession
{
    class CurrentSessionHolder
    {
        private MessageSession messageSession;
        private readonly AsyncLocal<PipelineContextSession> pipelineContext = new AsyncLocal<PipelineContextSession>();

        public IBusSession Current
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

        public IDisposable SetMessageSession(IMessageSession messageSession)
        {
            if (this.messageSession != null)
            {
                throw new InvalidOperationException("Attempt to overwrite an existing message session in BusSession.Current.");
            }

            this.messageSession = new MessageSession(messageSession);
            return new SessionScope(this);
        }

        class ContextSCope : IDisposable
        {
            private readonly CurrentSessionHolder sessionHolder;

            public ContextSCope(CurrentSessionHolder sessionHolder)
            {
                this.sessionHolder = sessionHolder;
            }

            public void Dispose()
            {
                sessionHolder.pipelineContext.Value.Dispose();
                sessionHolder.pipelineContext.Value = null;
            }
        }

        class SessionScope : IDisposable
        {
            private readonly CurrentSessionHolder sessionHolder;

            public SessionScope(CurrentSessionHolder sessionHolder)
            {
                this.sessionHolder = sessionHolder;
            }

            public void Dispose()
            {
                sessionHolder.messageSession.Dispose();
                sessionHolder.messageSession = null;
            }
        }
    }
}