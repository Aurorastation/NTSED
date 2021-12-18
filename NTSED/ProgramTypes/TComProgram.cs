using CCore.Net.JsRt;
using CCore.Net.Managed;
using NTSED.CCoreHelpers;
using NTSED.Models;
using NTSED.ProgramTypes.Interfaces;
using NTSED.ProgramTypes.ScriptInner;

namespace NTSED.ProgramTypes
{
    public class TComProgram : BaseProgram, IHasTerminal
    {
        private Terminal Terminal;
        private TCom Com;

        public TComProgram(ILogger logger) : base(logger)
        {
            Terminal = new Terminal(this);
            Com = new TCom();
        }

        public override void InstallInterfaces()
        {
            base.InstallInterfaces();
            JsObject.GlobalObject["Term"] = JsManagedObject.Obtain(Terminal, StrictMappingValidator.Instance);
            JsObject.GlobalObject["Com"] = JsManagedObject.Obtain(Com, StrictMappingValidator.Instance);
        }

        public string GetTerminalBuffer() => Terminal.Stringify();

        public async Task HandleTopic(string hash, string data)
        {
            var callback = GetCallback(hash);
            if (callback == null)
                return;
            await DoTimed(() =>
            {
                JsValue dataParam = data == null ? JsNull.Null : new JsString(data);
                callback.Invoke(JsObject.GlobalObject, dataParam);
            }, DEFAULT_SCRIPT_TIMEOUT, CCore.Net.JsTaskPriority.CALLBACK);
        }

        public void ProcessSignal(TComSignal signal)
        {
            var handler = Com.signalHandler;
            if (handler != null)
                DoTimed(() =>
                {
                    Com.Broadcast(signal);
                    handler.Invoke(JsObject.GlobalObject, JsManagedObject.Obtain(signal, StrictMappingValidator.Instance));
                }, DEFAULT_TCOM_TIMEOUT, CCore.Net.JsTaskPriority.CALLBACK);
        }

        public TComSignal[] GetSignals() => Com.GetSignals();

        internal override bool HandleException(Exception exception)
        {
            if (exception is JsTerminationException)
            {
                Terminal.PrintException(exception);
                return true;
            }
            if (exception is JsScriptException ex)
            {
                Terminal.PrintException(ex);
                return true;
            }
            return base.HandleException(exception);
        }
    }
}
