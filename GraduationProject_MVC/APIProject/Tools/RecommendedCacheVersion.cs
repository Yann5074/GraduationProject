namespace ApiProject.Tools
{
    public static class RecommendedCacheVersion
    {
        private static int _version = 1;
        public static int Current => Volatile.Read(ref _version);

        public static void Bump() => Interlocked.Increment(ref _version); // #TODO
    }
}
