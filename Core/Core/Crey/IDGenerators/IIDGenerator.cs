namespace Crey.IDGenerators
{
    public interface IIDGenerator<out T>
    {
        T GetNextID();
    }
}