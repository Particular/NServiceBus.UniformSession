namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using UniformSession;

    public class When_sending_from_cached_pipeline_session_from_pipeline : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_throw_when_sending()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointCachingSession>(e => e
                    .When(s => s.SendLocal(new Message1())))
                .Done(c => c.SendException != null || c.Message3Received)
                .Run();

            Assert.IsTrue(ctx.Message1Received);
            Assert.IsTrue(ctx.Message2Received);
            Assert.IsFalse(ctx.Message3Received);
            Assert.IsNotNull(ctx.SendException);
            StringAssert.Contains("The message handling pipeline owning this 'IUniformSession' instance has been completed, it is no longer possible to execute message operations.", ctx.SendException.Message);
        }

        class Context : ScenarioContext
        {
            public bool Message1Received;
            public bool Message2Received;
            public bool Message3Received;
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
                });
            }

            public class Handler1 : IHandleMessages<Message1>
            {
                public Handler1(Context testContext, SingletonService service)
                {
                    this.testContext = testContext;
                    this.service = service;
                }

                public Task Handle(Message1 message, IMessageHandlerContext context)
                {
                    testContext.Message1Received = true;
                    return service.SendLocal(new Message2());
                }

                Context testContext;
                SingletonService service;
            }

            public class Handler2 : IHandleMessages<Message2>
            {
                public Handler2(Context testContext, SingletonService service)
                {
                    this.testContext = testContext;
                    this.service = service;
                }

                public async Task Handle(Message2 message, IMessageHandlerContext context)
                {
                    testContext.Message2Received = true;
                    try
                    {
                        await service.SendLocal(new Message3());
                    }
                    catch (InvalidOperationException e)
                    {
                        testContext.SendException = e;
                    }
                }

                Context testContext;
                SingletonService service;
            }

            public class Handler3 : IHandleMessages<Message3>
            {
                public Handler3(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(Message3 message, IMessageHandlerContext context)
                {
                    testContext.Message3Received = true;
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

        public class Message3 : IMessage
        {
        }
    }
}