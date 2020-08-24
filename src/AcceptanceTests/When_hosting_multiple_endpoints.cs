namespace NServiceBus.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using UniformSession;

    public class When_hosting_multiple_endpoints : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_inject_different_session_per_endpoint()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint1>(e => e
                    .When(s => s.SendLocal(new MessageForEndpoint1())))
                .WithEndpoint<Endpoint2>(e => e
                    .When(s => s.SendLocal(new MessageForEndpoint2())))
                .Done(c => c.Endpoint1HandlerSession != null && c.Endpoint2HandlerSession != null)
                .Run();

            Assert.AreNotSame(ctx.Endpoint1StartupSession, ctx.Endpoint2StartupSession);
            Assert.AreNotSame(ctx.Endpoint1HandlerSession, ctx.Endpoint2HandlerSession);
            Assert.AreNotSame(ctx.Endpoint1HandlerSession, ctx.Endpoint1StartupSession);
            Assert.AreNotSame(ctx.Endpoint2HandlerSession, ctx.Endpoint2StartupSession);
        }

        class Context : ScenarioContext
        {
            public IUniformSession Endpoint1StartupSession;
            public IUniformSession Endpoint1HandlerSession;
            public IUniformSession Endpoint2HandlerSession;
            public IUniformSession Endpoint2StartupSession;
        }

        class Endpoint1 : EndpointConfigurationBuilder
        {
            public Endpoint1()
            {
                EndpointSetup<DefaultServer>(e =>
                {
                    e.EnableUniformSession();
                    e.EnableFeature<Endpoint1FeatureWithStartupTask>();
                });
            }

            class Endpoint1FeatureWithStartupTask : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.Container.ConfigureComponent<Endpoint1FeatureStartupTaskUsingDependencyInjection>(DependencyLifecycle.InstancePerCall);
                    context.RegisterStartupTask(b => b.GetRequiredService<Endpoint1FeatureStartupTaskUsingDependencyInjection>());
                }

                class Endpoint1FeatureStartupTaskUsingDependencyInjection : FeatureStartupTask
                {
                    public Endpoint1FeatureStartupTaskUsingDependencyInjection(IUniformSession uniformSession, Context testContext)
                    {
                        this.uniformSession = uniformSession;
                        this.testContext = testContext;
                    }

                    protected override Task OnStart(IMessageSession session)
                    {
                        testContext.Endpoint1StartupSession = uniformSession;
                        return Task.CompletedTask;
                    }

                    protected override Task OnStop(IMessageSession session)
                    {
                        return Task.CompletedTask;
                    }

                    IUniformSession uniformSession;
                    Context testContext;
                }
            }

            public class Handler : IHandleMessages<MessageForEndpoint1>
            {
                public Handler(Context testContext, IUniformSession session)
                {
                    testContext.Endpoint1HandlerSession = session;
                }

                public Task Handle(MessageForEndpoint1 message, IMessageHandlerContext context)
                {
                    return Task.CompletedTask;
                }
            }
        }

        class Endpoint2 : EndpointConfigurationBuilder
        {
            public Endpoint2()
            {
                EndpointSetup<DefaultServer>(e =>
                {
                    e.EnableUniformSession();
                    e.EnableFeature<Endpoint2FeatureWithStartupTask>();
                });
            }

            class Endpoint2FeatureWithStartupTask : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.Container.ConfigureComponent<Endpoint2FeatureStartupTaskUsingDependencyInjection>(DependencyLifecycle.InstancePerCall);
                    context.RegisterStartupTask(b => b.GetRequiredService<Endpoint2FeatureStartupTaskUsingDependencyInjection>());
                }

                class Endpoint2FeatureStartupTaskUsingDependencyInjection : FeatureStartupTask
                {
                    public Endpoint2FeatureStartupTaskUsingDependencyInjection(IUniformSession uniformSession, Context testContext)
                    {
                        this.uniformSession = uniformSession;
                        this.testContext = testContext;
                    }

                    protected override Task OnStart(IMessageSession session)
                    {
                        testContext.Endpoint2StartupSession = uniformSession;
                        return Task.CompletedTask;
                    }

                    protected override Task OnStop(IMessageSession session)
                    {
                        return Task.CompletedTask;
                    }

                    IUniformSession uniformSession;
                    Context testContext;
                }
            }

            public class Handler : IHandleMessages<MessageForEndpoint2>
            {
                public Handler(Context testContext, IUniformSession session)
                {
                    testContext.Endpoint2HandlerSession = session;
                }

                public Task Handle(MessageForEndpoint2 message, IMessageHandlerContext context)
                {
                    return Task.CompletedTask;
                }
            }
        }

        public class MessageForEndpoint1 : IMessage
        {
        }

        public class MessageForEndpoint2 : IMessage
        {
        }
    }
}