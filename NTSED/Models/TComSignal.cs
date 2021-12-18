using NTSED.Attributes;
using NTSED.JsonHelpers;
using System.Text.Json.Serialization;

namespace NTSED.Models
{
    [JsObject]
    public class TComSignal
    {
        [JsMapped]
        public string Content { get; set; } = "*beep*";
        [JsMapped]
        public int Freq { get; set; } = 1459;
        [JsMapped]
        public string Source { get; set; } = "Telecomms Broadcaster";
        [JsMapped]
        public string Job { get; set; } = "Machine";
        [JsMapped][JsonConverter(typeof(ByondBoolConverter))]
        public bool Pass { get; set; } = true;
        [JsMapped]
        public string Verb { get; set; } = "says";
        [JsMapped]
        public string Language { get; set; } = "Ceti Basic";
        public string? Reference { get; set; }

        [JsCallable("clone")]
        public TComSignal Clone()
        {
            var n = new TComSignal();
            n.Content = Content;
            n.Freq = Freq;
            n.Source = Source;
            n.Job = Job;
            n.Verb = Verb;
            n.Language = Language;
            return n;
        }
    }
}
