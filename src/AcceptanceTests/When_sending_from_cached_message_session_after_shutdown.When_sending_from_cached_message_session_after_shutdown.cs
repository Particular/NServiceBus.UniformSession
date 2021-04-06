namespace NServiceBus.UniformSession.AcceptanceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using UniformSession;

    public class When_sending_from_cached_message_session_after_shutdown : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_throw_when_sending()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithStartupTask>()
                .Done(c => c.EndpointsStarted)
                .Run();

            Assert.NotNull(ctx.StartupUniformSession);
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => ctx.StartupUniformSession.SendLocal(new MyMessage()));
            StringAssert.Contains("The endpoint owning this 'IUniformSession' instance has been stopped, so it is no longer possible to execute message operations.", exception.Message);
        }

        class Context : ScenarioContext
        {
            public IUniformSession StartupUniformSession;
        }

        class EndpointWithStartupTask : EndpointConfigurationBuilder
        {
            public EndpointWithStartupTask()
            {
                EndpointSetup<DefaultServer>(e =>
                {
                    e.EnableUniformSession();

                    e.EnableFeature<FeatureWithStartupTask>();
                });
            }

            class FeatureWithStartupTask : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.Services.AddTransient<FeatureStartupTaskUsingDependencyInjection>();
                    context.RegisterStartupTask(b => b.GetRequiredService<FeatureStartupTaskUsingDependencyInjection>());
                }

                class FeatureStartupTaskUsingDependencyInjection : FeatureStartupTask
                {
                    public FeatureStartupTaskUsingDependencyInjection(IUniformSession uniformSession, Context testContext)
                    {
                        this.uniformSession = uniformSession;
                        this.testContext = testContext;
                    }

                    protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
                    {
                        testContext.StartupUniformSession = uniformSession;
                        return Task.CompletedTask;
                    }

                    protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default)
                    {
                        return Task.CompletedTask;
                    }

                    IUniformSession uniformSession;
                    Context testContext;
                }
            }
        }

        public class MyMessage : IMessage
        {
        }
    }
}