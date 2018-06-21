using System;

namespace wLib.Promise
{
    public interface IPromiser
    {
        void Reject(Exception exception);
        void Resolve();
        void Notify(float process);
    }

    public interface IPromiser<in TPromised> : IPromiser
    {
        void Resolve(TPromised resolveValue);
        void Notify(float process, TPromised current);
    }
}