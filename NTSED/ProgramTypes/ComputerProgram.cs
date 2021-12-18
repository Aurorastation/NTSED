using CCore.Net.JsRt;
using CCore.Net.Managed;
using NTSED.CCoreHelpers;
using NTSED.ProgramTypes.ScriptInner;

namespace NTSED.ProgramTypes
{
    public class ComputerProgram : BaseProgram
    {
        private Terminal Terminal;

        public ComputerProgram(ILogger logger) : base(logger)
        {
            Terminal = new Terminal(this);
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

        public override void InstallInterfaces()
        {
            base.InstallInterfaces();
            JsObject.GlobalObject["Term"] = JsManagedObject.Obtain(Terminal, StrictMappingValidator.Instance);
        }

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
