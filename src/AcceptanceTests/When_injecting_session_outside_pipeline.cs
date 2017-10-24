namespace NServiceBus.AcceptanceTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using NUnit.Framework;
    using UniformSession;

    public class When_injecting_session_outside_pipeline : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_resolve_IMessageSession_based_session()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithStartupTask>()
                .Done(c => c.handlerInvocationCounter >= 2)
                .Run();

            Assert.AreSame(ctx.StartupUniformSession, ctx.ShutdownUniformSession);
            Assert.AreEqual(2, ctx.handlerInvocationCounter);
        }

        class Context : ScenarioContext
        {
            public int handlerInvocationCounter;
            public IUniformSession StartupUniformSession { get; set; }
            public IUniformSession ShutdownUniformSession { get; set; }
        }

        public class EndpointWithStartupTask : EndpointConfigurationBuilder
        {
            public EndpointWithStartupTask()
            {
                EndpointSetup<DefaultServer>(e => e.EnableFeature<FeatureWithStartupTask>());
            }

            class FeatureWithStartupTask : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.Container.ConfigureComponent<FeatureStartupTaskUsingDependencyInjection>(DependencyLifecycle.InstancePerCall);
                    context.RegisterStartupTask(b => b.Build<FeatureStartupTaskUsingDependencyInjection>());
                }

                class FeatureStartupTaskUsingDependencyInjection : FeatureStartupTask
                {
                    IUniformSession uniformSession;
                    Context testContext;

                    public FeatureStartupTaskUsingDependencyInjection(IUniformSession uniformSession, Context testContext)
                    {
                        this.uniformSession = uniformSession;
                        this.testContext = testContext;
                    }

                    protected override async Task OnStart(IMessageSession session)
                    {
                        testContext.StartupUniformSession = uniformSession;
                        await session.SendLocal(new DemoMessage());
                        await uniformSession.SendLocal(new DemoMessage());
                    }

                    protected override Task OnStop(IMessageSession session)
                    {
                        testContext.ShutdownUniformSession = uniformSession;
                        return Task.CompletedTask;
                    }
                }
            }

            class DemoMessageHandler : IHandleMessages<DemoMessage>
            {
                Context testContext;

                public DemoMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(DemoMessage message, IMessageHandlerContext context)
                {
                    Interlocked.Increment(ref testContext.handlerInvocationCounter);
                    return Task.CompletedTask;
                }
            }

            public class DemoMessage : IMessage
            {
            }
        }
    }
}