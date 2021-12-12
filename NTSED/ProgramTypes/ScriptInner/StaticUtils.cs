using CCore.Net.Managed;
using System.Text;

namespace NTSED.ProgramTypes.ScriptInner
{
    /// <summary>
    /// Class for Utils that are static, do not have mutatable state
    /// </summary>
    public static class StaticUtils
    {
        public static string Btoa(JsValue value)
        {
            JsString stringValue;
            if (value is JsString jsString)
                stringValue = jsString;
            else
                stringValue = value.ConvertToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringValue));
        }

        public static string Atob(string base64)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
    }
}
