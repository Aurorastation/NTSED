using CCore.Net.Managed;
using NTSED.Attributes;
using NTSED.Models;

namespace NTSED.ProgramTypes.ScriptInner
{
    [JsObject]
    public class TCom
    {
        internal JsFunction? signalHandler;
        private List<TComSignal> pendingSignals = new List<TComSignal>();

        [JsCallable("setSignalHandler")]
        public void SetSignalHandler(JsValue handler)
        {
            if(signalHandler != null)
                signalHandler.Dispose();
            if(handler is JsNull)
            {
                signalHandler = null;
            }
            if(handler is JsFunction jsFunction)
            {
                signalHandler = jsFunction;
                jsFunction.AddRef();
            }
        }

        [JsCallable("createSignal")]
        public TComSignal CreateSignal()
        {
            return new TComSignal();
        }

        [JsCallable("broadcast")]
        public TComSignal Broadcast()
        {
            var n = CreateSignal();
            AddSignalOnce(n);
            return n;
        }

        [JsCallable("broadcast")]
        public TComSignal Broadcast(TComSignal signal)
        {
            AddSignalOnce(signal);
            return signal;
        }

        private void AddSignalOnce(TComSignal signal)
        {
            if (!pendingSignals.Contains(signal))
                pendingSignals.Add(signal);
        }

        public TComSignal[] GetSignals()
        {
            var signals = pendingSignals.ToArray();
            pendingSignals.Clear();
            return signals;
        }
    }
}
