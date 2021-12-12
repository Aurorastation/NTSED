using CCore.Net.Managed;
using NTSED.Attributes;
using System.Text;
using System.Web;

namespace NTSED.ProgramTypes.ScriptInner
{
    [JsObject]
    public class Terminal
    {
        [JsMapped("cursorX")]
        public int CursorX { get; set; } = 0;
        [JsMapped("cursorY")]
        public int CursorY { get; set; } = 0;
        [JsMapped("width")]
        public int Width { get; private set; } = 64;
        [JsMapped("height")]
        public int Height { get; private set; } = 20;

        TerminalChar[,] buffer;

        public Color Background = new Color(0, 0, 0);
        public Color Foreground = new Color(255, 255, 255);

        private readonly BaseProgram context;

        public Terminal(int width, int height, BaseProgram context)
        {
            Width = width;
            Height = height;
            this.context = context;
            buffer = new TerminalChar[Width,Height];
        }

        public Terminal(BaseProgram context) : this(64, 20, context) { }

        [JsCallable("setForeground")]
        public void SetForeground(float r, float g, float b) => Foreground = new Color(r, g, b);
        [JsCallable("getForeground")]
        public JsArray getForeground()
        {
            var array = new JsArray(3);
            array[0] = (JsNumber)Foreground.R;
            array[1] = (JsNumber)Foreground.G;
            array[2] = (JsNumber)Foreground.B;
            return array;
        }
        [JsCallable("setBackground")]
        public void SetBackground(float r, float g, float b) => Background = new Color(r, g, b);
        [JsCallable("getBackground")]
        public JsArray GetBackground()
        {
            var array = new JsArray(3);
            array[0] = (JsNumber)Background.R;
            array[1] = (JsNumber)Background.G;
            array[2] = (JsNumber)Background.B;
            return array;
        }
        [JsCallable("setCursor")]
        public void setCursor(int x, int y)
        {
            CursorX = x;
            CursorY = y;
        }
        [JsCallable("getCursor")]
        public JsArray GetCursor()
        {
            var array = new JsArray(2);
            array[0] = (JsNumber)CursorX;
            array[1] = (JsNumber)CursorY;
            return array;
        }

        [JsCallable("clear")]
        public void Clear()
        {
            CursorX = 0;
            CursorY = 0;
            buffer = new TerminalChar[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    buffer[x, y].Foreground = Foreground;
                    buffer[x, y].Background = Background;
                }

        }
        [JsCallable("write")]
        public void Write(JsValue val)
        {
            if(val is JsString jsString)
                RealWrite(jsString);
            else
                RealWrite(val.ConvertToString());
        }

        [JsCallable("write")]
        public void Write(JsValue val, bool promt, JsFunction callback)
        {
            var callbackObject = context.RegisterCallback(callback);
            if (val is JsString jsString)
                RealWrite(jsString, callbackObject, promt);
            else
                RealWrite(val.ConvertToString(), callbackObject, promt);
        }

        [JsCallable("write")]
        public void Write(JsValue val, JsFunction callback) => Write(val, false, callback);

        [JsCallable("print")]
        public void Print(JsValue val)
        {
            Write(val);
            CursorX = 0;
            MoveDown();
        }

        private void RealWrite(string str, Tuple<string, JsFunction>? topic = null, bool prompt = false)
        {
            foreach (char c in str)
            {
                if (c == '\r')
                {
                    CursorX = 0;
                }
                else if (c == '\n')
                {
                    CursorX = 0;
                    MoveDown();
                }
                else if (c == '\t')
                {
                    MoveRight();
                    while (CursorX % 4 > 0)
                        MoveRight();
                }
                else
                {
                    lock (buffer)
                    {
                        buffer[CursorX,CursorY] = new TerminalChar(c, Background, Foreground, topic, prompt);
                    }
                    MoveRight();
                }
            }
        }
        [JsCallable("getSize")]
        public JsArray GetSize()
        {
            var array = new JsArray(2);
            array[0] = (JsNumber)Width;
            array[1] = (JsNumber)Height;
            return array;
        }

        [JsCallable("setTopic")]
        public void SetTopic(int x, int y, int w, int h, bool promt, JsFunction callback)
        {
            var callbackObject = context.RegisterCallback(callback);
            lock (buffer)
                for (int X = 0; X < w; X++)
                    for (int Y = 0; Y < h; Y++)
                        SetTopic(x + X, y + Y, callbackObject, promt);
        }
        [JsCallable("setTopic")]
        public void SetTopic(int x, int y, int w, int h, JsFunction callback) => SetTopic(x, y, w, h, false, callback);

        [JsCallable("clearTopic")]
        public void ClearTopic(int x, int y, int w, int h)
        {
            lock (buffer)
                for (int X = 0; X < w; X++)
                    for (int Y = 0; Y < h; Y++)
                        SetTopic(x + X, y + Y, null, false);
        }

        private void SetTopic(int x, int y, Tuple<string, JsFunction>? callback, bool prompt)
        {
            if (y >= 0 && x >= 0 && x < Width && y < Height)
            {
                buffer[y,x].Callback = callback;
                buffer[y,x].Prompt = prompt;

            }
        }

        public string Stringify()
        {
            StringBuilder o = new StringBuilder();
            lock (buffer)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color lastfg = Background;
                    Color lastbg = Foreground;
                    string? lastTopic = null;
                    for (int x = 0; x < Width; x++)
                    {
                        var termChar = buffer[x, y];
                        if (x == 0)
                        {
                            // Jut clean line init
                            if (termChar.Topic != null)
                            {
                                o.Append(FormatLinkOpening(termChar.Topic));
                                lastTopic = termChar.Topic;
                            }
                            o.Append(FormatColorOpening(termChar.Foreground, termChar.Background));
                            lastfg = termChar.Foreground;
                            lastbg = termChar.Background;
                        }
                        else
                        {
                            if (lastTopic != termChar.Topic)
                            {
                                // Topic is diffrent, let's change that
                                o.Append(FormatColorClosing(lastfg, lastbg)); // before doing anything close colors
                                if (lastTopic != null) // If topic was opened
                                {
                                    o.Append(FormatLinkClosing(lastTopic)); // Close topic
                                }
                                if (termChar.Topic != null)
                                {
                                    o.Append(FormatLinkOpening(termChar.Topic)); // Open new topic
                                }
                                o.Append(FormatColorOpening(termChar.Foreground, termChar.Background)); // Open new colors
                                lastTopic = termChar.Topic;
                                lastfg = termChar.Foreground;
                                lastbg = termChar.Background;
                            }
                            if (lastfg != termChar.Foreground || lastbg != termChar.Background)
                            {
                                // Colors diffrent, close and reopen
                                o.Append(FormatColorClosing(lastfg, lastbg)); // close colors
                                o.Append(FormatColorOpening(termChar.Foreground, termChar.Background)); // Open new colors
                                lastfg = termChar.Foreground;
                                lastbg = termChar.Background;
                            }
                        }

                        // Ourput char
                        o.Append(Encode(termChar.Text));
                    }

                    o.Append(FormatColorClosing(lastfg, lastbg)); // close colors
                    if (lastTopic != null) // If topic was opened
                    {
                        o.Append(FormatLinkClosing(lastTopic)); // Close topic
                    }
                    o.Append("<br/>"); // Add line break
                }
            }
            return o.ToString();
        }

        internal static string FormatLinkOpening(string topic) => $"<to to=\"{topic}\">";
        internal static string FormatLinkClosing(string topic) => $"</to>";


        internal static string FormatColorOpening(Color fg, Color bg) => $"<co fg=\"{fg.ToHex()}\" bg=\"{bg.ToHex()}\">";
        internal static string FormatColorClosing(Color fg, Color bg) => $"</co>";

        public static string Encode(char c)
        {
            if (c == ' ')
                return "&nbsp;";
            return HttpUtility.HtmlEncode(c.ToString());
        }

        public void MoveRight()
        {
            CursorX++;
            if (CursorX >= Width)
            {
                CursorX = 0;
                MoveDown();
            }
        }

        public void MoveDown()
        {
            CursorY++;
            if (CursorY >= Height) // Are we past buffer Heigth?
            {
                CursorY--; // Back of
                Array.Copy(buffer, Width, buffer, 0, buffer.Length - Width);
                for (int x = 0; x < Width; x++)
                {
                    buffer[Height - 1, x].Background = Background;
                    buffer[Height - 1, x].Foreground = Foreground;
                }
            }
        }


        internal void PrintException(Exception ex)
        {
            var lastfgcolor = Foreground;
            var lastbgcolor = Background;
            Foreground = new Color(255, 0, 0);
            Background = new Color(0, 0, 0);
            RealWrite(ex.Message);
            RealWrite("\r\n");
            Foreground = lastfgcolor;
            Background = lastbgcolor;
        }


        struct TerminalChar
        {
            public char Text = ' ';
            public Color Background = new Color(0, 0, 0);
            public Color Foreground = new Color(255, 255, 255);
            public Tuple<string, JsFunction>? Callback = null;
            public bool Prompt = false;

            public string? Topic
            {
                get
                {
                    if (Callback != null)
                    {
                        if (Prompt) return '?' + Callback.Item1;
                        return Callback.Item1;
                    }
                    return null;
                }
            }


            public TerminalChar()
            {
            }

            public TerminalChar(char text, Color background, Color foreground, Tuple<string, JsFunction>? callback = null, bool prompt = false)
            {
                Text = text;
                Background = background;
                Foreground = foreground;
                Callback = callback;
                Prompt = prompt;
            }


        }

        public struct Color
        {
            public byte R = 0;
            public byte G = 0;
            public byte B = 0;

            public Color()
            {
            }

            public Color(float r, float g, float b) : this((int)(r * 255), (int)(g * 255), (int)(b * 255))
            {
            }

            public Color(int r, int g, int b)
            {
                R = (byte)Math.Clamp(r, 0, 255);
                G = (byte)Math.Clamp(g, 0, 255);
                B = (byte)Math.Clamp(b, 0, 255);
            }

            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }


            public string ToHex()
            {
                return $"{R:X2}{G:X2}{B:X2}";
            }

            public bool Equals(Color c)
            {
                return R == c.R && G == c.G && B == c.B;
            }

            public override bool Equals(object? c)
            {
                if (c is Color)
                    return Equals((Color)c);
                return false;
            }

            public override int GetHashCode()
            {
                int hash = R.GetHashCode();
                unchecked
                {
                    hash += G.GetHashCode();
                    hash += B.GetHashCode();
                }
                return hash;
            }

            public static bool operator ==(Color lhs, Color rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(Color lhs, Color rhs)
            {
                return !(lhs == rhs);
            }
        }
    }
}
