using System.Threading;

namespace Crey.IDGenerators
{
    internal class LongIDGenerator : IIDGenerator<long>
    {
        private long _id;

        public long GetNextID()
        {
            return Interlocked.Increment(ref _id);
        }
    }
}