﻿namespace NServiceBus.UniformSession.AcceptanceTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using UniformSession;

    public class When_injecting_session_outside_pipeline : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_resolve_IMessageSession_based_session()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithStartupTask>()
                .Done(c => c.HandlerInvocationCounter >= 2)
                .Run();

            Assert.Multiple(() =>
            {
                Assert.That(ctx.ShutdownUniformSession, Is.SameAs(ctx.StartupUniformSession));
                Assert.That(ctx.HandlerInvocationCounter, Is.EqualTo(2));
            });
        }

        class Context : ScenarioContext
        {
            public int HandlerInvocationCounter;
            public IUniformSession StartupUniformSession;
            public IUniformSession ShutdownUniformSession;
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

                    protected override async Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
                    {
                        testContext.StartupUniformSession = uniformSession;
                        await session.SendLocal(new DemoMessage(), cancellationToken: cancellationToken);
                        await uniformSession.SendLocal(new DemoMessage(), cancellationToken: cancellationToken);
                    }

                    protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default)
                    {
                        testContext.ShutdownUniformSession = uniformSession;
                        return Task.CompletedTask;
                    }

                    IUniformSession uniformSession;
                    Context testContext;
                }
            }

            class DemoMessageHandler : IHandleMessages<DemoMessage>
            {
                public DemoMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(DemoMessage message, IMessageHandlerContext context)
                {
                    Interlocked.Increment(ref testContext.HandlerInvocationCounter);
                    return Task.CompletedTask;
                }

                Context testContext;
            }

            public class DemoMessage : IMessage
            {
            }
        }
    }
}