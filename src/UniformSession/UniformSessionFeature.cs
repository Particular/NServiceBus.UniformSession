using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Features;

class UniformSessionFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        context.Pipeline.Register(new RegisterCurrentSessionBehavior(sessionHolder), "Enables floating of uniform session.");
        context.RegisterStartupTask(new RegisterSessionStartupTask(sessionHolder));

        context.Services.AddTransient(_ => sessionHolder.Current);
    }

    CurrentSessionHolder sessionHolder = new CurrentSessionHolder();

    class RegisterSessionStartupTask : FeatureStartupTask
    {
        public RegisterSessionStartupTask(CurrentSessionHolder sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        protected override Task OnStart(IMessageSession session)
        {
            scope = sessionHolder.SetMessageSession(session);
            return Task.CompletedTask;
        }

        protected override Task OnStop(IMessageSession session)
        {
            scope.Dispose();
            return Task.CompletedTask;
        }

        readonly CurrentSessionHolder sessionHolder;
        IDisposable scope;
    }
}
