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
            await runtime.Do(() =>
            {
                JsValue dataParam = data == null ? JsNull.Null : new JsString(data);
                callback.Invoke(JsObject.GlobalObject, dataParam);
            }, CCore.Net.JsTaskPriority.CALLBACK);
        }

        public override void InstallInterfaces()
        {
            base.InstallInterfaces();
            JsObject.GlobalObject["Term"] = JsManagedObject.Obtain(Terminal, StrictMappingValidator.Instance);
        }

        internal override bool HandleException(Exception exception)
        {
            var ex = exception as JsScriptException;
            if (ex != null)
            {
                Terminal.PrintException(ex);
                return true;
            }
            return base.HandleException(exception);
        }
    }
}
