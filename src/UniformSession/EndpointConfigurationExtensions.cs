namespace NServiceBus
{
    /// <summary>
    /// Extension methods for the uniform session that extend <see cref="EndpointConfiguration"/>.
    /// </summary>
    public static class UniformSessionExtensions
    {
        /// <summary>
        /// Enables the uniform session.
        /// </summary>
        public static void EnableUniformSession(this EndpointConfiguration endpointConfiguration)
        {
            endpointConfiguration.EnableFeature<UniformSessionFeature>();
        }
    }
}