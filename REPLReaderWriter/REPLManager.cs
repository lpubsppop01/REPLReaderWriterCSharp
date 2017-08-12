using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lpubsppop01.REPLReaderWriter
{
    public class REPLManager
    {
        #region Export

        static List<REPL> repls = new List<REPL>();

        public static int Start(string command, string templateName)
        {
            var repl = REPLTemplates.GetByName(templateName).Clone();
            repl.Command = command;
            repls.Add(repl);
            return repl.Start();
        }

        public static void SetTimeout(int processID, int timeoutMilliseconds)
        {
            var repl = repls.FirstOrDefault(r => r.ProcessID == processID);
            if (repl == null) throw new InvalidOperationException();
            repl.SetTimeout(timeoutMilliseconds);
        }

        public static void UnsetTimeout(int processID)
        {
            var repl = repls.FirstOrDefault(r => r.ProcessID == processID);
            if (repl == null) throw new InvalidOperationException();
            repl.SetTimeout(null);
        }

        public static void WriteLine(int processID, string inputText)
        {
            var repl = repls.FirstOrDefault(r => r.ProcessID == processID);
            if (repl == null) throw new InvalidOperationException();
            repl.WriteLine(inputText);
        }

        public static string ReadLine(int processID)
        {
            var repl = repls.FirstOrDefault(r => r.ProcessID == processID);
            if (repl == null) throw new InvalidOperationException();
            return repl.ReadLine();
        }

        public static void Stop(int processID)
        {
            var repl = repls.FirstOrDefault(r => r.ProcessID == processID);
            if (repl == null) throw new InvalidOperationException();
            repl.Stop(processID);
        }

        #endregion
    }
}
