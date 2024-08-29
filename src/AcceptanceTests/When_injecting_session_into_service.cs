namespace NServiceBus.UniformSession.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
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

            Assert.That(ctx.ServiceASession, Is.SameAs(ctx.HandlerSession));
            Assert.That(ctx.ServiceBSession, Is.SameAs(ctx.HandlerSession));
        }

        class Context : ScenarioContext
        {
            public IUniformSession HandlerSession;
            public IUniformSession ServiceASession;
            public IUniformSession ServiceBSession;
            public bool MessageHandled;
        }

        class EndpointWithServices : EndpointConfigurationBuilder
        {
            public EndpointWithServices()
            {
                EndpointSetup<DefaultServer>(e =>
                {
                    e.EnableUniformSession();

                    e.RegisterComponents(r =>
                    {
                        r.AddTransient<ServiceA>();
                        r.AddScoped<ServiceB>();
                    });
                });
            }

            public class DummyMessageHandler : IHandleMessages<DummyMessage>
            {
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

                Context testContext;
                IUniformSession session;
                ServiceA serviceA;
            }

            public class ServiceA
            {
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

                Context testContext;
                IUniformSession session;
                ServiceB serviceB;
            }

            public class ServiceB
            {
                public ServiceB(Context testContext, IUniformSession session)
                {
                    this.testContext = testContext;
                    this.session = session;
                }

                public void InvokeService()
                {
                    testContext.ServiceBSession = session;
                }

                Context testContext;
                IUniformSession session;
            }
        }

        public class DummyMessage : IMessage
        {
        }
    }
}