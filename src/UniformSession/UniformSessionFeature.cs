using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;

class UniformSessionFeature : Feature
{
    public UniformSessionFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        context.Pipeline.Register(new RegisterCurrentSessionBehavior(sessionHolder), "Enables floating of uniform session.");
        context.RegisterStartupTask(new RegisterSessionStartupTask(sessionHolder));

        context.Container.ConfigureComponent(() => sessionHolder.Current, DependencyLifecycle.InstancePerCall);
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