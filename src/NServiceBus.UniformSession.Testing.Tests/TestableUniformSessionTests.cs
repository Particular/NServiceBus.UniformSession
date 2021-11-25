namespace NServiceBus.UniformSession.Testing.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class TestableUniformSessionTests
    {
        [Test]
        public async Task When_used_to_test_another_component_collects_outgoing_messages()
        {
            var uniformSession = new TestableUniformSession();
            var reusableComponent = new ReusableComponent(uniformSession);

            await reusableComponent.FireCommand().ConfigureAwait(false);

            Assert.AreEqual(1, uniformSession.SentMessages.Length);
        }

        [Test]
        public async Task When_used_to_test_another_component_collects_outgoing_events()
        {
            var uniformSession = new TestableUniformSession();
            var reusableComponent = new ReusableComponent(uniformSession);

            await reusableComponent.PublishEvent().ConfigureAwait(false);

            Assert.AreEqual(1, uniformSession.PublishedMessages.Length);
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

        class SomeCommand : ICommand
        {
            public Guid CustomerId { get; set; }
        }

        class SomeEvent : IEvent
        {

        }

        class SomeHandler : IHandleMessages<SomeCommand>
        {
            readonly IUniformSession session;

            public SomeHandler(IUniformSession session) => this.session = session;

            public Task Handle(SomeCommand message, IMessageHandlerContext context)
                => session.Publish(new SomeEvent());
        }

        class SomeSagaData : ContainSagaData
        {
            public Guid CustomerId { get; set; }
        }

        class SomeSaga : NServiceBus.Saga<SomeSagaData>, IAmStartedByMessages<SomeCommand>
        {
            readonly IUniformSession session;

            public SomeSaga(IUniformSession session) => this.session = session;

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SomeSagaData> mapper)
                => mapper.MapSaga(saga => saga.CustomerId)
                    .ToMessage<SomeCommand>(command => command.CustomerId);

            public Task Handle(SomeCommand message, IMessageHandlerContext context)
                => session.Publish<SomeEvent>();
        }
    }
}
