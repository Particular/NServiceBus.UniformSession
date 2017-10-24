using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

class RegisterCurrentSessionBehavior : IBehavior<IIncomingPhysicalMessageContext, IIncomingPhysicalMessageContext>
{
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

    readonly CurrentSessionHolder sessionHolder;
}