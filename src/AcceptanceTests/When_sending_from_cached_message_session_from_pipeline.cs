namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using UniformSession;

    public class When_sending_from_cached_message_session_from_pipeline : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_throw_when_sending()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointCachingSession>(e => e
                    .When(s => s.SendLocal(new Message1())))
                .Done(c => c.SendException != null || c.Message2Received)
                .Run();

            Assert.IsTrue(ctx.Message1Received);
            Assert.IsFalse(ctx.Message2Received);
            Assert.IsNotNull(ctx.SendException);
            StringAssert.Contains("This 'IUniformSession' instance belongs to an endpoint and cannot be used in the message handling pipeline. Usage of this 'IUniformSession' instance within a pipeline can lead to message duplication.", ctx.SendException.Message);
        }

        class Context : ScenarioContext
        {
            public bool Message1Received;
            public bool Message2Received;
            public InvalidOperationException SendException;
        }

        class EndpointCachingSession : EndpointConfigurationBuilder
        {
            public EndpointCachingSession()
            {
                EndpointSetup<DefaultServer>(e =>
                {
                    e.EnableUniformSession();

                    e.RegisterComponents(c => c
                        // this will cause the resolved dependency to be cached across multiple pipeline invocations
                        .AddSingleton<SingletonService>());

                    e.EnableFeature<StartupTaskFeature>();
                });
            }

            public class StartupTaskFeature : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.Services.AddTransient<SessionStartupTask>();
                    context.RegisterStartupTask(b => b.GetRequiredService<SessionStartupTask>());
                }

                class SessionStartupTask : FeatureStartupTask
                {
                    public SessionStartupTask(SingletonService service)
                    {
                        // The service will get an IMessageSession based session which will be cached by the singleton
                        this.service = service;
                    }

                    protected override Task OnStart(IMessageSession session)
                    {
                        return Task.CompletedTask;
                    }

                    protected override Task OnStop(IMessageSession session)
                    {
                        return Task.CompletedTask;
                    }

                    SingletonService service;
                }
            }

            public class Handler1 : IHandleMessages<Message1>
            {
                public Handler1(Context testContext, SingletonService service)
                {
                    this.testContext = testContext;
                    this.service = service;
                }

                public async Task Handle(Message1 message, IMessageHandlerContext context)
                {
                    testContext.Message1Received = true;
                    try
                    {
                        await service.SendLocal(new Message2());
                    }
                    catch (InvalidOperationException e)
                    {
                        testContext.SendException = e;
                    }
                }

                Context testContext;
                SingletonService service;
            }

            public class Handler2 : IHandleMessages<Message2>
            {
                public Handler2(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(Message2 message, IMessageHandlerContext context)
                {
                    testContext.Message2Received = true;
                    return Task.CompletedTask;
                }

                Context testContext;
            }

            public class SingletonService
            {
                public SingletonService(IUniformSession session)
                {
                    this.session = session;
                }

                public Task SendLocal(object message)
                {
                    return session.SendLocal(message);
                }

                IUniformSession session;
            }
        }

        public class Message1 : IMessage
        {
        }

        public class Message2 : IMessage
        {
        }
    }
}