using System.Threading;

namespace Crey.IDGenerators
{
    public class IntIDGenerator : IIDGenerator<int>
    {
        private int _id;

        public IntIDGenerator(int startID)
        {
            _id = startID;
        }

        public int GetNextID()
        {
            return Interlocked.Increment(ref _id);
        }
    }

    public class RequestIdGenerator : IntIDGenerator
    {
        public RequestIdGenerator(int startID) : base(startID) { }

        public uint NextId()
        {
            return (uint)(GetNextID() & 0x7fffffff);
        }
    }
}