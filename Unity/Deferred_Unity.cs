using System.Collections;

namespace wLib.Promise
{
    public static partial class Deferred
    {
        public static IPromise FromCoroutine(IEnumerator coroutine)
        {
            var resultPromise = Create();
            Heartbeat.RunCoroutine(coroutine, () => resultPromise.Resolve(), e => resultPromise.Reject(e));
            return resultPromise;
        }
    }
}