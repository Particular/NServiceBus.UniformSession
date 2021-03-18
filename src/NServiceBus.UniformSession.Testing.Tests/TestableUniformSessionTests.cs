namespace NServiceBus.UniformSession.Testing.Tests
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class TestableUniformSessionTests
    {
        [Test]
        public void Can_be_used_in_handler_test()
        {
            var uniformSession = new TestableUniformSession();
            var handler = new SomeHandler(uniformSession);
            Test.Handler(handler)
                .WithUniformSession(uniformSession)
                .ExpectPublish<SomeEvent>()
                .OnMessage<SomeCommand>();
        }

        [Test]
        public void When_not_initialized_in_handler_test_should_throw()
        {
            var uniformSession = new TestableUniformSession();
            var handler = new SomeHandler(uniformSession);
            var exception = Assert.Throws<Exception>(
                () => Test.Handler(handler)
                            .ExpectPublish<SomeEvent>()
                            .OnMessage<SomeCommand>());
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("WithUniformSession"));
        }

        [Test]
        public void Can_be_used_in_saga_test()
        {
            var uniformSession = new TestableUniformSession();
            var saga = new SomeSaga(uniformSession);
            Test.Saga(saga)
                .WithUniformSession(uniformSession)
                .ExpectPublish<SomeEvent>()
                .WhenHandling<SomeCommand>()
                ;
        }

        [Test]
        public void When_not_initialized_should_throw()
        {
            var uniformSession = new TestableUniformSession();
            var saga = new SomeSaga(uniformSession);
            var exception = Assert.Throws<Exception>(
                () => Test.Saga(saga)
                    .ExpectPublish<SomeEvent>()
                    .WhenHandling<SomeCommand>());
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("WithUniformSession"));
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
