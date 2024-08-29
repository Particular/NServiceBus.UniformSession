namespace NServiceBus.UniformSession.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using Pipeline;
    using UniformSession;

    public class When_sending_from_injected_session : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_participate_in_transaction_commits()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithMultipleMessages>(e => e
                    .When(s => s.SendLocal(new StartCommand())))
                .Done(c => c.FollowupCommandReceived)
                .Run();

            Assert.That(ctx.StartCommandReceived, Is.True);
            Assert.That(ctx.FollowupCommandReceived, Is.True);
        }

        [Test]
        public async Task Should_participate_in_transaction_rollbacks()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithMultipleMessages>(e => e
                    .CustomConfig(c => c.Pipeline.Register(typeof(EndpointWithMultipleMessages.FailPipelineBehavior), "cause the incoming transaction to rollback"))
                    .When(s =>
                    {
                        var options = new SendOptions();
                        options.SetHeader("rollback", bool.TrueString);
                        options.RouteToThisEndpoint();
                        return s.Send(new StartCommand(), options);
                    })
                    .DoNotFailOnErrorMessages())
                .Done(c => c.ExceptionThrown)
                .Run();

            Assert.That(ctx.StartCommandReceived, Is.True);
            Assert.That(ctx.FollowupCommandReceived, Is.False);
        }

        class Context : ScenarioContext
        {
            public bool StartCommandReceived;
            public bool FollowupCommandReceived;
            public bool ExceptionThrown;
        }

        class EndpointWithMultipleMessages : EndpointConfigurationBuilder
        {
            public EndpointWithMultipleMessages()
            {
                EndpointSetup<DefaultServer>(e => e.EnableUniformSession());
            }

            public class StartCommandHandler : IHandleMessages<StartCommand>
            {
                public StartCommandHandler(Context testContext, IUniformSession uniformSession)
                {
                    this.testContext = testContext;
                    this.uniformSession = uniformSession;
                }

                public async Task Handle(StartCommand message, IMessageHandlerContext context)
                {
                    await uniformSession.SendLocal(new FollowupCommand());
                    testContext.StartCommandReceived = true;
                }

                Context testContext;
                IUniformSession uniformSession;
            }

            public class FailPipelineBehavior : Behavior<ITransportReceiveContext>
            {
                public FailPipelineBehavior(Context testContext)
                {
                    this.testContext = testContext;
                }

                public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
                {
                    await next();

                    if (context.Message.Headers.ContainsKey("rollback"))
                    {
                        testContext.ExceptionThrown = true;
                        // throw an exception at the end as outgoing operations have been sent by now.
                        throw new Exception("test");
                    }
                }

                Context testContext;
            }

            public class FollowupCommandHandler : IHandleMessages<FollowupCommand>
            {
                public FollowupCommandHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(FollowupCommand message, IMessageHandlerContext context)
                {
                    testContext.FollowupCommandReceived = true;
                    return Task.CompletedTask;
                }

                Context testContext;
            }
        }

        public class StartCommand : ICommand
        {
        }

        public class FollowupCommand : ICommand
        {
        }
    }
}