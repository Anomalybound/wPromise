using System;

namespace wLib.Promise
{
    public static partial class Deferred
    {
        private static int id;
        
        public static Promise Create()
        {
            var promise = new Promise(GetId());
            return promise;
        }
        
        public static Promise Create(string name)
        {
            var promise = new Promise(GetId());
            promise.SetName(name);
            return promise;
        }

        public static Promise<TPromised> Create<TPromised>()
        {
            var promise = new Promise<TPromised>(GetId());
            return promise;
        }

        public static Promise<TPromised> Create<TPromised>(string name)
        {
            var promise = new Promise<TPromised>(GetId());
            promise.SetName(name);
            return promise;
        }

        #region Static Promise

        public static IPromise<TPromised> Resolved<TPromised>(TPromised value)
        {
            var promise = new Promise<TPromised>(GetId());
            promise.Resolve(value);
            return promise;
        }

        public static IPromise<TPromised> Rejected<TPromised>(Exception exception)
        {
            var promise = new Promise<TPromised>(GetId());
            promise.Reject(exception);
            return promise;
        }

        #endregion
    }
}