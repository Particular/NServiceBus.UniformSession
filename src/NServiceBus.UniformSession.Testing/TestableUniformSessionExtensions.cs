namespace NServiceBus.UniformSession.Testing
{
    using NServiceBus.Testing;

    public static class TestableUniformSessionExtensions
    {
        public static Handler<T> WithUniformSession<T>(this Handler<T> handler, TestableUniformSession uniformSession)
            => handler.ConfigureHandlerContext(uniformSession.SetInner);

        public static Saga<T> WithUniformSession<T>(this Saga<T> saga, TestableUniformSession uniformSession) where T : Saga
            => saga.ConfigureHandlerContext(uniformSession.SetInner);
    }
}