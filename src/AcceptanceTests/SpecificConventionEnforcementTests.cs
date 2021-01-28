namespace NServiceBus.AcceptanceTests
{
    using System.Linq;
    using System.Reflection;
    using AcceptanceTesting;
    using NUnit.Framework;

    [TestFixture]
    public class SpecificConventionEnforcementTests : NServiceBusAcceptanceTest
    {
        [Test]
        public void Ensure_scenario_context_uses_public_fields()
        {
            var testTypes = Assembly.GetExecutingAssembly().GetTypes();

            var contexts = (from context in testTypes
                            where typeof(ScenarioContext).IsAssignableFrom(context)
                            let properties = context.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                            where properties.Length > 0
                            select context)
                .ToList();

            CollectionAssert.IsEmpty(contexts, string.Join(",", contexts), "To avoid automatic property injection use public fields only.");
        }
    }
}