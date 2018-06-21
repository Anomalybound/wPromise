using System;

namespace wLib
{
    public interface IPromise
    {
        int Id { get; }
        
        IPromise Then(Action onResolved);
        
        IPromise Then(Action onResolved, Action<Exception> onRejected);
        
        IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onNotified);

        IPromise Then(Func<IPromise> onResolved);
        
        IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected);
        
        IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected, Action<float> onNotified);

        IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved);
        
        IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved, Action<Exception> onRejected);
        
        IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved, Action<Exception> onRejected, Action<float, TPromised> onNotified);

        IPromise Catch(Action<Exception> onCatched);

        IPromise Step(Action<float> onNotified);

        void Done();

        void Done(Action doneAction);
    }

    public interface IPromise<out TPromised> : IPromise
    {
        IPromise<TPromised> Then(Action<TPromised> onResolved);

        IPromise<TPromised> Then(Action<TPromised> onResolved, Action<Exception> onRejected);

        IPromise<TPromised> Then(Action<TPromised> onResolved, Action<Exception> onRejected, Action<float, TPromised> onNotified);

        IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved);
        
        IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved, Action<Exception> onRejected);

        IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved, Action<Exception> onRejected, Action<float, TConverted> onNotified);

        IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved);

        IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved, Action<Exception> onRejected);

        IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved, Action<Exception> onRejected, Action<float, TConverted> onNotified);

        IPromise<TPromised> Step(Action<float, TPromised> onNotified);

        new IPromise<TPromised> Catch(Action<Exception> onCatched);

        void Done(Action<TPromised> doneAction);
    }
}