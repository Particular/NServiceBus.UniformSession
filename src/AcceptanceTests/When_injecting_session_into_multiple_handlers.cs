namespace NServiceBus.UniformSession.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using UniformSession;

    public class When_injecting_session_into_multiple_handlers : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_inject_same_pipeline_context_to_all_handlers()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithMultipleHandlers>(e => e
                    .When(s => s.SendLocal(new MessageHandledByMultipleHandlers())))
                .Done(c => c.Handler1Session != null && c.Handler2Session != null)
                .Run();

            Assert.AreSame(ctx.Handler1Session, ctx.Handler2Session);
        }

        class Context : ScenarioContext
        {
            public IUniformSession Handler1Session;
            public IUniformSession Handler2Session;
        }

        class EndpointWithMultipleHandlers : EndpointConfigurationBuilder
        {
            public EndpointWithMultipleHandlers()
            {
                EndpointSetup<DefaultServer>(e => e.EnableUniformSession());
            }

            public class Handler1 : IHandleMessages<MessageHandledByMultipleHandlers>
            {
                public Handler1(Context testContext, IUniformSession session)
                {
                    testContext.Handler1Session = session;
                }

                public Task Handle(MessageHandledByMultipleHandlers message, IMessageHandlerContext context)
                {
                    return Task.CompletedTask;
                }
            }

            public class Handler2 : IHandleMessages<MessageHandledByMultipleHandlers>
            {
                public Handler2(Context testContext, IUniformSession session)
                {
                    testContext.Handler2Session = session;
                }

                public Task Handle(MessageHandledByMultipleHandlers message, IMessageHandlerContext context)
                {
                    return Task.CompletedTask;
                }
            }
        }

        public class MessageHandledByMultipleHandlers : IMessage
        {
        }
    }
}