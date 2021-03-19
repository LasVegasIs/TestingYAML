namespace Crey.Contracts
{
    public static class CreyNamingConvention
    {
        public static string GetVersionedStoragePath(string version, string name)
        {
            return $"{version}/{SanitizeName(name)}";
        }

        private static string SanitizeName(string name)
        {
            var sanitiztedName = name;
            if (sanitiztedName.StartsWith('/')) sanitiztedName = sanitiztedName.Substring(1, sanitiztedName.Length - 1);
            return sanitiztedName;
        }
    }
}
