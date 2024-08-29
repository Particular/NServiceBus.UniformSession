namespace NServiceBus.UniformSession.Testing.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Testing;
    using NUnit.Framework;
    using Pipeline;

    [TestFixture]
    public class TestableUniformSessionTests
    {
        [Test]
        public async Task Shares_state_with_handler_context()
        {
            var handlerContext = new TestableMessageHandlerContext();
            var uniformSession = new TestableUniformSession(handlerContext);
            var handler = new SendingHandler(uniformSession);

            await handler.Handle(new SomeCommand(), handlerContext);

            Assert.That(uniformSession.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(uniformSession.SentMessages.Length, Is.EqualTo(2));
            Assert.That(handlerContext.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(handlerContext.SentMessages.Length, Is.EqualTo(2));

            Assert.That(handlerContext.DoNotContinueDispatchingCurrentMessageToHandlersWasCalled, Is.True);
        }

        [Test]
        public async Task Shares_state_with_pipeline_context()
        {
            var pipelineContext = new TestableIncomingLogicalMessageContext();
            var uniformSession = new TestableUniformSession(pipelineContext);
            var behavior = new SendingBehavior(uniformSession);

            await behavior.Invoke(pipelineContext, () => Task.CompletedTask);

            Assert.That(uniformSession.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(uniformSession.SentMessages.Length, Is.EqualTo(2));
            Assert.That(pipelineContext.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(pipelineContext.SentMessages.Length, Is.EqualTo(2));

            Assert.That(pipelineContext.Headers["testHeader"], Is.EqualTo("testValue"));
        }

        [Test]
        public async Task Shares_state_with_message_session()
        {
            var messageSession = new TestableMessageSession();
            var uniformSession = new TestableUniformSession(messageSession);
            var component = new MessageSessionComponent(uniformSession);

            await component.DoSomething(messageSession);

            Assert.That(uniformSession.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(uniformSession.SentMessages.Length, Is.EqualTo(2));
            Assert.That(messageSession.PublishedMessages.Length, Is.EqualTo(2));
            Assert.That(messageSession.SentMessages.Length, Is.EqualTo(2));
        }

        [Test]
        public async Task When_used_to_test_another_component_collects_outgoing_messages()
        {
            var uniformSession = new TestableUniformSession();
            var reusableComponent = new ReusableComponent(uniformSession);

            await reusableComponent.FireCommand();

            Assert.That(uniformSession.SentMessages.Length, Is.EqualTo(1));
        }

        [Test]
        public async Task When_used_to_test_another_component_collects_outgoing_events()
        {
            var uniformSession = new TestableUniformSession();
            var reusableComponent = new ReusableComponent(uniformSession);

            await reusableComponent.PublishEvent();

            Assert.That(uniformSession.PublishedMessages.Length, Is.EqualTo(1));
        }

        class ReusableComponent
        {
            readonly IUniformSession session;

            public ReusableComponent(IUniformSession session)
            {
                this.session = session;
            }

            public Task FireCommand(CancellationToken cancellationToken = default) => session.Send(new SomeCommand(), cancellationToken: cancellationToken);

            public Task PublishEvent(CancellationToken cancellationToken = default) => session.Publish(new SomeEvent(), cancellationToken: cancellationToken);
        }

        class SendingHandler : IHandleMessages<SomeCommand>
        {
            IUniformSession uniformSession;

            public SendingHandler(IUniformSession uniformSession)
            {
                this.uniformSession = uniformSession;
            }

            public async Task Handle(SomeCommand message, IMessageHandlerContext context)
            {
                await context.Send(new SomeCommand());
                await context.Publish(new SomeEvent());
                await uniformSession.Send(new SomeCommand());
                await uniformSession.Publish(new SomeEvent());

                context.DoNotContinueDispatchingCurrentMessageToHandlers();
            }
        }

        class SendingBehavior : Behavior<IIncomingLogicalMessageContext>
        {
            IUniformSession uniformSession;

            public SendingBehavior(IUniformSession uniformSession)
            {
                this.uniformSession = uniformSession;
            }

            public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
            {
                await uniformSession.Send(new SomeCommand());
                await uniformSession.Publish(new SomeEvent());
                await context.Send(new SomeCommand());
                await context.Publish(new SomeEvent());
                await next();

                context.Headers["testHeader"] = "testValue";
            }
        }

        class MessageSessionComponent
        {
            IUniformSession uniformSession;

            public MessageSessionComponent(IUniformSession uniformSession)
            {
                this.uniformSession = uniformSession;
            }

            public async Task DoSomething(IMessageSession messageSession)
            {
                await messageSession.Send<ISomeOtherCommand>(_ => { });
                await messageSession.Publish<ISomeOtherEvent>(_ => { });
                await uniformSession.Send<ISomeOtherCommand>(_ => { });
                await uniformSession.Publish<ISomeOtherEvent>(_ => { });
            }
        }

        public interface ISomeOtherCommand : ICommand
        {
        }

        public interface ISomeOtherEvent : IEvent
        {
        }

        class SomeCommand : ICommand
        {
            public Guid CustomerId { get; set; }
        }

        class SomeEvent : IEvent
        {
        }
    }
}
