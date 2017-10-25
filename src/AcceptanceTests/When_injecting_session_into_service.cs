namespace NServiceBus.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using UniformSession;

    public class When_injecting_session_into_service : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_inject_same_session_to_all_services_resolved_from_pipeline()
        {
            var ctx = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithServices>(e => e
                    .When(s => s.SendLocal(new DummyMessage())))
                .Done(c => c.MessageHandled)
                .Run();

            Assert.AreSame(ctx.HandlerSession, ctx.ServiceASession);
            Assert.AreSame(ctx.HandlerSession, ctx.ServiceBSession);
        }

        public class Context : ScenarioContext
        {
            public IUniformSession HandlerSession { get; set; }
            public IUniformSession ServiceASession { get; set; }
            public IUniformSession ServiceBSession { get; set; }
            public bool MessageHandled { get; set; }
        }

        public class EndpointWithServices : EndpointConfigurationBuilder
        {
            public EndpointWithServices()
            {
                EndpointSetup<DefaultServer>(e => e.RegisterComponents(r =>
                {
                    r.ConfigureComponent<ServiceA>(DependencyLifecycle.InstancePerCall);
                    r.ConfigureComponent<ServiceB>(DependencyLifecycle.InstancePerUnitOfWork);
                }));
            }

            public class DummyMessageHandler : IHandleMessages<DummyMessage>
            {
                Context testContext;
                IUniformSession session;
                ServiceA serviceA;

                public DummyMessageHandler(Context testContext, IUniformSession session, ServiceA serviceA)
                {
                    this.testContext = testContext;
                    this.session = session;
                    this.serviceA = serviceA;
                }

                public Task Handle(DummyMessage message, IMessageHandlerContext context)
                {
                    testContext.HandlerSession = session;
                    serviceA.InvokeService();
                    testContext.MessageHandled = true;
                    return Task.CompletedTask;
                }
            }

            public class ServiceA
            {
                Context testContext;
                IUniformSession session;
                ServiceB serviceB;

                public ServiceA(Context testContext, IUniformSession session, ServiceB serviceB)
                {
                    this.testContext = testContext;
                    this.session = session;
                    this.serviceB = serviceB;
                }

                public void InvokeService()
                {
                    testContext.ServiceASession = session;
                    serviceB.InvokeService();
                }
            }

            public class ServiceB
            {
                Context testContext;
                IUniformSession session;

                public ServiceB(Context testContext, IUniformSession session)
                {
                    this.testContext = testContext;
                    this.session = session;
                }

                public void InvokeService()
                {
                    testContext.ServiceBSession = session;
                }
            }
        }

        public class DummyMessage : IMessage
        {
        }
    }
}