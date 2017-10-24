using System;
using System.Threading.Tasks;
using NServiceBus.Features;

namespace NServiceBus.AmbientSession
{
    public class AmbietSessionFeature : Feature
    {
        CurrentSessionHolder sessionHolder = new CurrentSessionHolder();

        public AmbietSessionFeature()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register(new RegisterCurrentSessionBehavior(sessionHolder), "register the current session in the ambiet bus session.");
            context.RegisterStartupTask(new RegisterSessionStartupTask(sessionHolder));

            context.Container.ConfigureComponent<IBusSession>(() => sessionHolder.Current, DependencyLifecycle.InstancePerCall);
        }

        private class RegisterSessionStartupTask : FeatureStartupTask
        {
            private readonly CurrentSessionHolder sessionHolder;
            IDisposable scope;

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
        }
    }
}