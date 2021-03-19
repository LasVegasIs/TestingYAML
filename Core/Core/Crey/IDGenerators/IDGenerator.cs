namespace Crey.IDGenerators
{
    public static class IDGenerator
    {
        public static IIDGenerator<int> CreateIntIDGenerator(int startID = 1)
        {
            return new IntIDGenerator(startID);
        }

        public static IIDGenerator<long> CreateLongIDGenerator()
        {
            return new LongIDGenerator();
        }
    }
}