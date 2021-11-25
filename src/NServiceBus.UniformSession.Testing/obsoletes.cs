namespace NServiceBus.UniformSession.Testing

{
    using System;
    using NServiceBus.Testing;

    public static class TestableUnifoSrmSessionExtensions
    {
        [Obsolete("Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for" +
                         " more details. Will be removed in version 4.0.0.", true)]
        public static Handler<T> WithUniformSession<T>(this Handler<T> handler, TestableUniformSession uniformSession)
            => throw new NotImplementedException();

        [Obsolete("Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for" +
                  " more details. Will be removed in version 4.0.0.", true)]
        public static Saga<T> WithUniformSession<T>(this Saga<T> saga, TestableUniformSession uniformSession) where T : Saga
            => throw new NotImplementedException();
    }
}