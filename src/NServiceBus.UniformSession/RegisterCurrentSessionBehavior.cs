using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

namespace NServiceBus.AmbientSession
{
    class RegisterCurrentSessionBehavior : IBehavior<IIncomingPhysicalMessageContext, IIncomingPhysicalMessageContext>
    {
        private readonly CurrentSessionHolder sessionHolder;

        public RegisterCurrentSessionBehavior(CurrentSessionHolder sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        public async Task Invoke(IIncomingPhysicalMessageContext context, Func<IIncomingPhysicalMessageContext, Task> next)
        {
            using (sessionHolder.SetCurrentPipelineContext(context))
            {
                await next(context).ConfigureAwait(false);
            }
        }
    }
}