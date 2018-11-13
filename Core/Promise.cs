using System;
using System.Collections.Generic;

namespace wLib.Promise
{
    /// <summary>
    /// Specifies the state of a promise.
    /// </summary>
    public enum PromiseState
    {
        // The promise is still processing.
        Pending,

        // The promise has been rejected.
        Rejected,

        // The promise has been resolved.
        Resolved
    }

    public class Promise : IPromise, IPromiser
    {
        public string Name { get; protected set; }

        public PromiseState CurrentState { get; protected set; }

        public IPromise SetName(string name)
        {
            Name = name;
            IsNamed = true;
            return this;
        }

        public Promise(int id)
        {
            Id = id;
            Name = "Promise-" + id;
            CurrentState = PromiseState.Pending;
            IsNamed = false;

            _processHandlers = new List<NotifyInvoker>();
            _resolveHandlers = new List<ResolveInvoker>();
            _rejectHandlers = new List<RejectInvoker>();

            _rejectedValue = null;
        }

        protected bool IsNamed;

        #region Runtime Variables

        private List<ResolveInvoker> _resolveHandlers;
        private List<RejectInvoker> _rejectHandlers;
        private List<NotifyInvoker> _processHandlers;

        private Exception _rejectedValue;

        #endregion

        #region IPromise

        public int Id { get; protected set; }

        public IPromise Then(Action onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public IPromise Then(Func<IPromise> onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public void Done()
        {
            Done(null);
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onNotified)
        {
            var resultPromise = IsNamed ? Deferred.Create(Name) : Deferred.Create();

            Action resolveHandle = () =>
            {
                try
                {
                    onResolved?.Invoke();

                    resultPromise.Resolve();
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onRejected?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<float> notifyHandle = (process) =>
            {
                try
                {
                    onNotified?.Invoke(process);

                    resultPromise.Notify(process);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddPromiseHandles(resultPromise, resolveHandle, rejectHandle, notifyHandle);

            return resultPromise;
        }

        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected, Action<float> onNotified)
        {
            var resultPromise = IsNamed ? Deferred.Create(Name) : Deferred.Create();

            Action resolveHandle = () =>
            {
                try
                {
                    if (onResolved != null)
                    {
                        onResolved().Then(
                            () => resultPromise.Resolve(),
                            ex => resultPromise.Reject(ex),
                            process => resultPromise.Notify(process)
                        );
                    }
                    else { resultPromise.Resolve(); }
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onRejected?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<float> notifyHandle = process =>
            {
                try
                {
                    onNotified?.Invoke(process);

                    resultPromise.Notify(process);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddPromiseHandles(resultPromise, resolveHandle, rejectHandle, notifyHandle);

            return resultPromise;
        }

        public IPromise<TPromised> Then<TPromised>(Func<IPromise<TPromised>> onResolved, Action<Exception> onRejected,
            Action<float, TPromised> onNotified)
        {
            var resultPromise = IsNamed ? Deferred.Create<TPromised>(Name) : Deferred.Create<TPromised>();

            Action resolveHandle = () =>
            {
                try
                {
                    onResolved?.Invoke().Then(
                        promised => resultPromise.Resolve(promised),
                        ex => resultPromise.Reject(ex),
                        (process, current) =>
                        {
                            onNotified?.Invoke(process, current);

                            resultPromise.Notify(process, current);
                        });
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onRejected?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddPromiseHandles(resultPromise, resolveHandle, rejectHandle, null);

            return resultPromise;
        }

        public IPromise Catch(Action<Exception> onCatched)
        {
            var resultPromise = IsNamed ? Deferred.Create(Name) : Deferred.Create();

            Action resolveHandle = () =>
            {
                try { resultPromise.Resolve(); }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onCatched?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<float> notifyHandle = process =>
            {
                try { resultPromise.Notify(process); }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddPromiseHandles(resultPromise, resolveHandle, rejectHandle, notifyHandle);

            return resultPromise;
        }

        public IPromise Step(Action<float> onNotified)
        {
            if (CurrentState == PromiseState.Pending) { _processHandlers.Add(new NotifyInvoker(this, onNotified)); }

            return this;
        }

        public void Done(Action doneAction)
        {
            Then(doneAction, null, null);
        }

        #endregion

        #region IPromiser

        public void Resolve()
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't resolve a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            CurrentState = PromiseState.Resolved;

            InvokeResolveHandlers();
        }

        public void Notify(float process)
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't notify a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            InvokeNotifyHandlers(process);
        }

        public void Reject(Exception exception)
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't reject a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            CurrentState = PromiseState.Rejected;

            _rejectedValue = exception;

            InvokeRejectHandlers(_rejectedValue);
        }

        #endregion

        #region Helpers

        private void AddPromiseHandles(IPromiser resultPromise, Action resolveHandle,
            Action<Exception> rejectHandle, Action<float> notifyHandle)
        {
            switch (CurrentState)
            {
                case PromiseState.Pending:
                    if (resolveHandle != null)
                    {
                        _resolveHandlers.Add(new ResolveInvoker(resultPromise, resolveHandle));
                    }

                    if (rejectHandle != null) { _rejectHandlers.Add(new RejectInvoker(resultPromise, rejectHandle)); }

                    if (notifyHandle != null) { _processHandlers.Add(new NotifyInvoker(resultPromise, notifyHandle)); }

                    break;
                case PromiseState.Rejected:
                    try { rejectHandle.Invoke(_rejectedValue); }
                    catch (Exception e) { resultPromise.Reject(e); }

                    break;
                case PromiseState.Resolved:
                    try { resolveHandle.Invoke(); }
                    catch (Exception e) { resultPromise.Reject(e); }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InvokeResolveHandlers()
        {
            if (_resolveHandlers != null)
            {
                foreach (var handle in _resolveHandlers)
                {
                    try { handle.ResolvedHandler.Invoke(); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }

                _resolveHandlers.Clear();
            }
        }

        private void InvokeRejectHandlers(Exception exception)
        {
            if (_rejectHandlers != null)
            {
                foreach (var handle in _rejectHandlers)
                {
                    try { handle.RejectHandler.Invoke(exception); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }

                _rejectHandlers.Clear();
            }
        }

        private void InvokeNotifyHandlers(float process)
        {
            if (_processHandlers != null)
            {
                foreach (var handle in _processHandlers)
                {
                    try { handle.NotifyHandler.Invoke(process); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }
            }
        }

        #endregion
    }

    public class Promise<TPromised> : Promise, IPromise<TPromised>, IPromiser<TPromised>
    {
        public Promise(int id) : base(id)
        {
            Id = id;
            Name = "Promise-" + id;
            CurrentState = PromiseState.Pending;
            IsNamed = false;

            _processHandlers = new List<NotifyInvoker<TPromised>>();
            _resolveHandlers = new List<ResolveInvoker<TPromised>>();
            _rejectHandlers = new List<RejectInvoker>();

            _resolvedValue = default(TPromised);
            _rejectedValue = null;
        }

        public new IPromise<TPromised> SetName(string name)
        {
            Name = name;
            IsNamed = true;
            return this;
        }

        #region Runtime Vaiables

        private List<ResolveInvoker<TPromised>> _resolveHandlers;
        private List<RejectInvoker> _rejectHandlers;
        private List<NotifyInvoker<TPromised>> _processHandlers;

        private TPromised _resolvedValue;
        private Exception _rejectedValue;

        #endregion

        #region IPromise<TPromised>

        public IPromise<TPromised> Step(Action<float, TPromised> onNotified)
        {
            if (onNotified != null && CurrentState == PromiseState.Pending)
            {
                _processHandlers.Add(new NotifyInvoker<TPromised>(this, onNotified));
            }

            return this;
        }

        public IPromise<TPromised> Then(Action<TPromised> onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise<TPromised> Then(Action<TPromised> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved)
        {
            return Then(resloved => Deferred.Resolved(onResolved(resloved)));
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved,
            Action<Exception> onRejected)
        {
            return Then(resloved => Deferred.Resolved(onResolved(resloved)), onRejected,
                (Action<float, TConverted>) null);
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, TConverted> onResolved,
            Action<Exception> onRejected, Action<float, TConverted> onNotified)
        {
            return Then(resloved => Deferred.Resolved(onResolved(resloved)), onRejected, onNotified);
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved)
        {
            return Then(onResolved, null, (Action<float, TConverted>) null);
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved,
            Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, (Action<float, TConverted>) null);
        }

        public IPromise<TPromised> Then(Action<TPromised> onResolved, Action<Exception> onRejected,
            Action<float, TPromised> onNotified)
        {
            var resultPromise = IsNamed ? Deferred.Create<TPromised>(Name) : Deferred.Create<TPromised>();

            Action<TPromised> resolveHandle = promised =>
            {
                try
                {
                    onResolved?.Invoke(promised);

                    resultPromise.Resolve(promised);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onRejected?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<float, TPromised> notifyHandle = (process, promised) =>
            {
                try
                {
                    onNotified?.Invoke(process, promised);

                    resultPromise.Notify(process, promised);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddGenericPromiseHandles(resultPromise, resolveHandle, rejectHandle, notifyHandle);

            return resultPromise;
        }

        public IPromise<TConverted> Then<TConverted>(Func<TPromised, IPromise<TConverted>> onResolved,
            Action<Exception> onRejected, Action<float, TConverted> onNotified)
        {
            var resultPromise = IsNamed ? Deferred.Create<TConverted>(Name) : Deferred.Create<TConverted>();

            Action<TPromised> resolveHandle = promised =>
            {
                try
                {
                    if (onResolved != null)
                    {
                        var convertedPromise = onResolved(promised);
                        convertedPromise.Then(converted => resultPromise.Resolve(converted));
                        convertedPromise.Step((process, current) =>
                        {
                            onNotified?.Invoke(process, current);

                            resultPromise.Notify(process, current);
                        });
                    }
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onRejected?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddGenericPromiseHandles(resultPromise, resolveHandle, rejectHandle, null);

            return resultPromise;
        }

        // Override the IRromise one
        public new IPromise<TPromised> Catch(Action<Exception> onCatched)
        {
            var resultPromise = IsNamed ? Deferred.Create<TPromised>(Name) : Deferred.Create<TPromised>();

            Action<TPromised> resolveHandle = promised =>
            {
                try { resultPromise.Resolve(promised); }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<Exception> rejectHandle = ex =>
            {
                try
                {
                    onCatched?.Invoke(ex);

                    resultPromise.Reject(ex);
                }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            Action<float, TPromised> notifyHandle = (process, promised) =>
            {
                try { resultPromise.Notify(process, promised); }
                catch (Exception e) { resultPromise.Reject(e); }
            };

            AddGenericPromiseHandles(resultPromise, resolveHandle, rejectHandle, notifyHandle);

            return resultPromise;
        }

        // Override the IRromise one
        public new void Done(Action doneAction)
        {
            Then(promised => doneAction());
        }

        public void Done(Action<TPromised> doneAction)
        {
            Then(doneAction);
        }

        #endregion

        #region IPromiser

        public void Resolve(TPromised resolveValue)
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't resolve a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            CurrentState = PromiseState.Resolved;

            _resolvedValue = resolveValue;

            InvokeResolveHandlers(_resolvedValue);
        }

        public void Notify(float process, TPromised current)
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't notify a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            InvokeNotifyHandlers(process, current);
        }

        public new void Reject(Exception exception)
        {
            if (CurrentState != PromiseState.Pending)
            {
                throw new ApplicationException(string.Format("Can't reject a promise([{0}]) @state:[{1}]", Name,
                    CurrentState));
            }

            CurrentState = PromiseState.Rejected;

            _rejectedValue = exception;

            InvokeRejectHandlers(_rejectedValue);
        }

        #endregion

        #region Helpers

        private void AddGenericPromiseHandles(IPromiser resultPromise, Action<TPromised> resolveHandle,
            Action<Exception> rejectHandle, Action<float, TPromised> notifyHandle)
        {
            switch (CurrentState)
            {
                case PromiseState.Pending:
                    if (resolveHandle != null)
                    {
                        _resolveHandlers.Add(new ResolveInvoker<TPromised>(resultPromise, resolveHandle));
                    }

                    if (rejectHandle != null) { _rejectHandlers.Add(new RejectInvoker(resultPromise, rejectHandle)); }

                    if (notifyHandle != null)
                    {
                        _processHandlers.Add(new NotifyInvoker<TPromised>(resultPromise, notifyHandle));
                    }

                    break;
                case PromiseState.Rejected:
                    try { rejectHandle.Invoke(_rejectedValue); }
                    catch (Exception e) { resultPromise.Reject(e); }

                    break;
                case PromiseState.Resolved:
                    try { resolveHandle.Invoke(_resolvedValue); }
                    catch (Exception e) { resultPromise.Reject(e); }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InvokeResolveHandlers(TPromised resolveValue)
        {
            if (_resolveHandlers != null)
            {
                foreach (var handle in _resolveHandlers)
                {
                    try { handle.ResolvedHandler.Invoke(resolveValue); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }

                _resolveHandlers.Clear();
            }
        }

        private void InvokeRejectHandlers(Exception exception)
        {
            if (_rejectHandlers != null)
            {
                foreach (var handle in _rejectHandlers)
                {
                    try { handle.RejectHandler.Invoke(exception); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }

                _rejectHandlers.Clear();
            }
        }

        private void InvokeNotifyHandlers(float process, TPromised current)
        {
            if (_processHandlers != null)
            {
                foreach (var handle in _processHandlers)
                {
                    try { handle.NotifyHandler.Invoke(process, current); }
                    catch (Exception e) { handle.Rejector.Reject(e); }
                }
            }
        }

        #endregion
    }
}