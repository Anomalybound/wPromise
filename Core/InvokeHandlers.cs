using System;

namespace wLib.Promise
{
    public struct ResolveInvoker
    {
        public readonly IPromiser Rejector;
        public readonly Action ResolvedHandler;

        public ResolveInvoker(IPromiser rejector, Action resolvedHandler)
        {
            Rejector = rejector;
            ResolvedHandler = resolvedHandler;
        }
    }

    public struct NotifyInvoker
    {
        public readonly IPromiser Rejector;
        public readonly Action<float> NotifyHandler;

        public NotifyInvoker(IPromiser rejector, Action<float> notifyHandler)
        {
            Rejector = rejector;
            NotifyHandler = notifyHandler;
        }
    }
    
    public struct ResolveInvoker<TPromised>
    {
        public readonly IPromiser Rejector;
        public readonly Action<TPromised> ResolvedHandler;

        public ResolveInvoker(IPromiser rejector, Action<TPromised> resolvedHandler)
        {
            Rejector = rejector;
            ResolvedHandler = resolvedHandler;
        }
    }

    public struct RejectInvoker
    {
        public readonly IPromiser Rejector;
        public readonly Action<Exception> RejectHandler;

        public RejectInvoker(IPromiser rejector, Action<Exception> rejectHandler)
        {
            Rejector = rejector;
            RejectHandler = rejectHandler;
        }
    }

    public struct NotifyInvoker<TPromised>
    {
        public readonly IPromiser Rejector;
        public readonly Action<float, TPromised> NotifyHandler;

        public NotifyInvoker(IPromiser rejector, Action<float, TPromised> notifyHandler)
        {
            Rejector = rejector;
            NotifyHandler = notifyHandler;
        }
    }
}