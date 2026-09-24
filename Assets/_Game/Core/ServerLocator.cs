namespace ZombieGame.Core
{
    // Глобальный доступ к контексту сервера для сетевых компонентов (чтобы не нарушать asmdef зависимости)
    public static class ServerLocator
    {
        public static IServerContext Context;
    }
}
