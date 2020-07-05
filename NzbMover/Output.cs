using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbMover
{
    public class Output
    {
        public bool Debug { get; set; }
        private int MaxOutputType { get; set; }

        public Output()
        {
            MaxOutputType = Enum.GetNames(typeof(OutputType)).Where(w => !w.ToLower().Contains("none")).Max(m => m.Length);
        }

        /// <summary>
        /// writes without final linebreak
        /// </summary>
        /// <param name="oType"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void WriteStart(OutputType oType, string message, params object[] args)
        {
            if (oType == OutputType.Debug && !Debug)
                return;

            string msg = null;

            if (oType.ToString().ToLower().Contains("none"))
                msg = message;
            else
                msg = string.Format("[{0}{1}] {2}", oType.ToString(), new string(' ', MaxOutputType - oType.ToString().Length), message);

            WriteNext(oType, msg, args);
        }

        /// <summary>
        /// writes the next part without ending the line
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void WriteNext(OutputType oType, string message, params object[] args)
        {
            switch (oType)
            {
                case OutputType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case OutputType.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case OutputType.Error:
                case OutputType.ErrorNone:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            string msg = string.Format("{0}", message);
            Console.Write(msg, args ?? new object[0]);
        }

        /// <summary>
        /// writes and ends the line
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void WriteEnd(OutputType oType, string message, params object[] args)
        {
            string msg = string.Format("{0}{1}", message, Environment.NewLine);
            WriteNext(oType, msg, args);
        }

        /// <summary>
        /// writes a new line
        /// </summary>
        /// <param name="oType"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void WriteLine(OutputType oType, string message, params object[] args)
        {
            string msg = string.Format("{0}{1}", message, Environment.NewLine);
            WriteStart(oType, msg, args);
        }

        /// <summary>
        /// writes an empty line
        /// </summary>
        public void WriteEmptyLine(OutputType oType = OutputType.None)
        {
            WriteLine(OutputType.None, null);
        }

        /// <summary>
        /// write a seperator line
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="length"></param>
        public void WriteSeperator(char sep = '-', int length = 75)
        {
            WriteLine(OutputType.None, GetSeperator(sep, length));
        }

        /// <summary>
        /// get a seperator line string
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetSeperator(char sep = '-', int length = 75)
        {
            return new string(sep, length);
        }

        /// <summary>
        /// writes exception details
        /// </summary>
        /// <param name="ex"></param>
        public void WriteException(Exception ex, bool debug)
        {
            WriteException(null, ex, debug);
        }

        /// <summary>
        /// writes exception details
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="details"></param>
        public void WriteException(string message, Exception ex, bool debug)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSeperator());

            if (!string.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine(message);
                sb.AppendLine(GetSeperator());
            }

            sb.AppendLine(string.Format("Message: {0}", ex.Message));

            if(debug)
            {
                sb.AppendLine(GetSeperator());
                sb.AppendLine("Details:");
                sb.AppendLine(ex.ToString());
            } 
            else
            {
                sb.AppendLine("(Enable debug for details.)");
            }

            sb.AppendLine(GetSeperator());
            WriteLine(OutputType.ErrorNone, sb.ToString());
        }

        public void WaitForKeyPress(string message = "Press any key to continue.")
        {
            WriteLine(OutputType.None, message);
            Console.ReadKey();
        }

        public void WaitForKeyPressExit()
        {
            WaitForKeyPress("Press any key to exit the program.");
        }

        public enum OutputType
        {
            Info,
            Warn,
            Error,
            ErrorNone,
            Debug,
            DebugNone,
            None,
            Success
        }
    }
}
