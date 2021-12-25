using CCore.Net;
using CCore.Net.JsRt;
using CCore.Net.Managed;
using CCore.Net.Runtimes;
using NTSED.ProgramTypes.ScriptInner;
using System.Text;

namespace NTSED.ProgramTypes
{
    public class BaseProgram : IDisposable
    {
        public static readonly TimeSpan DEFAULT_SCRIPT_TIMEOUT = new TimeSpan(TimeSpan.TicksPerMillisecond * 2000);
        public static readonly TimeSpan DEFAULT_PROMISE_TIMEOUT = new TimeSpan(TimeSpan.TicksPerMillisecond * 2000);
        public static readonly TimeSpan DEFAULT_TCOM_TIMEOUT = new TimeSpan(TimeSpan.TicksPerMillisecond * 500);
        private static Random random = new Random();
        public const int CALLBACK_HASH_LEN = 12;
        protected ScheduledJsRuntime runtime;
        protected readonly Dictionary<string, WeakReference<JsFunction>> callbacks = new Dictionary<string, WeakReference<JsFunction>>();
        protected readonly ILogger logger;
        protected DateTime timelastidled = DateTime.Now;

        public BaseProgram(ILogger logger)
        {
            runtime = new ScheduledJsRuntime(JsRuntimeAttributes.AllowScriptInterrupt | JsRuntimeAttributes.EnableIdleProcessing);
            runtime.Scheduler.EnterIdle += Scheduler_EnterIdle;
            this.logger = logger;
        }

        private void Scheduler_EnterIdle()
        {
            if (timelastidled - DateTime.Now > new TimeSpan(0, 0, 10))
            {
                runtime.Do(() =>
                {
                    JsContext.Idle();
                }, JsTaskPriority.LOWEST);
            }
        }

        public async Task Initialize()
        {
            await runtime.Do(() =>
            {
                runtime.SetPromiseContinuationCallback(PromiseContinuationCallback, IntPtr.Zero);
                InstallInterfaces();
            }, JsTaskPriority.INITIALIZATION);
        }

        public void PromiseContinuationCallback(JsValueRef task, IntPtr callbackState)
        {
            var newTimeout = DEFAULT_PROMISE_TIMEOUT;
            if(runtime.Scheduler.GetCurrentTask() is IJsTaskTimed taskTimed)
            {
                newTimeout = taskTimed.RemainingTime - new TimeSpan(TimeSpan.TicksPerMillisecond * 2); // Add 2 ms penalty.
            }
            task.AddRef();
            DoTimed(() =>
            {
                task.CallFunction(JsObject.GlobalObject);
                task.Release();
            }, newTimeout, JsTaskPriority.PROMISE);
        }

        public virtual void InstallInterfaces()
        {
            JsObject.GlobalObject["btoa"] = JsManagedFunction.Obtain(StaticUtils.Btoa, "btoa");
            JsObject.GlobalObject["atob"] = JsManagedFunction.Obtain(StaticUtils.Atob, "atob");
        }


        public virtual void Dispose()
        {
            runtime?.Dispose();
        }

        private string GenerateCallbackHash()
        {
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(CALLBACK_HASH_LEN);
            for (int i = 0; i < CALLBACK_HASH_LEN; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            var finalResult = result.ToString();
            if (callbacks.ContainsKey(finalResult))
                return GenerateCallbackHash();
            return finalResult;
        }

        internal Tuple<string, JsFunction> RegisterCallback(JsFunction callback)
        {
            callback.AddRef();
            var hash = GenerateCallbackHash();
            callbacks[hash] = new WeakReference<JsFunction>(callback);
            return new Tuple<string, JsFunction>(hash, callback);
        }

        protected JsFunction? GetCallback(string hash)
        {
            var callbackRef = callbacks[hash];
            if (callbackRef == null)
                throw new Exception("Unknown callback.");
            if(callbackRef.TryGetTarget(out JsFunction? callback))
            {
                return callback;
            }
            return null;
        }

        internal JsTask<TResult?> DoTimed<TResult>(Func<TResult> func, TimeSpan timeout, JsTaskPriority priority = JsTaskPriority.LOWEST) => runtime.DoTimed(() =>
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (!HandleException(ex))
                    throw;
            }
            runtime.Enabled = true;
            return default;
        }, () => runtime.Enabled = false, timeout, priority);

        internal JsTask DoTimed(Action action, TimeSpan timeout, JsTaskPriority priority = JsTaskPriority.LOWEST) => runtime.DoTimed(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (!HandleException(ex))
                    throw;
            }
            runtime.Enabled = true;
        }, () => runtime.Enabled = false, timeout, priority);

        public JsTask<JsValue?> ExecuteScript(string script, string sourceName)
        {
            return DoTimed(() =>
            {
                return (JsValue)JsContext.RunScript(script, JsSourceContext.FromIntPtr((IntPtr)0), sourceName);
            }, DEFAULT_SCRIPT_TIMEOUT, JsTaskPriority.EXECUTION);
        }

        internal virtual bool HandleException(Exception exception)
        {
            logger.LogError(exception, "Unhandled js runtime exception.");
            return false;
        }
    }
}
