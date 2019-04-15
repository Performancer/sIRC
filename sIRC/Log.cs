using System;
using System.IO;
using System.Text;

namespace sIRC
{
    class Log : StreamWriter
    {
        public delegate void MessageHandler();
        public MessageHandler OnWrite;

        public Log() : base(Console.OpenStandardOutput(), Console.OutputEncoding)
        {
            AutoFlush = true;
            Console.SetOut(this);
        }

        public override void WriteLine(string value)
        {
            ClearCurrentLine();
            base.WriteLine(value);

            OnWrite.Invoke();
        }

        public static void WriteLine(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value, true);
            Console.ResetColor();
        }

        public static void ClearLastCharacter()
        {
            int left = Console.CursorLeft - 1;

            if (left < 0)
                left = 0;

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.Write(' ');
            Console.SetCursorPosition(left, Console.CursorTop);
        }

        public static void ClearCurrentLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        public static void WriteToFile(string file, string text, bool timestamp)
        {
            string prefix = "";

            if (timestamp)
            {
                string date = DateTime.Now.ToString("dd.MM.yyyy");
                string time = DateTime.Now.ToString("HH:mm:ss");

                prefix = string.Format("{0} {1} ", date, time);
            }

            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            using (StreamWriter writer = File.AppendText(@"logs\" + file))
                writer.WriteLine(string.Format("{0}{1}", prefix, text));
        }
    }
}
