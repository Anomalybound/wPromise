namespace wLib.Promise
{
    public static partial class Deferred
    {
        private static int GetId()
        {
            return id++;
        }
    }
}