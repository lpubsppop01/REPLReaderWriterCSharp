using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.REPLReaderWriter
{
    public class REPL : ICloneable
    {
        #region Constructors

        public REPL()
        {
            Command = "";
            Arguments = "";
            NewLine = Environment.NewLine;
            PromptWithNewLine = Environment.NewLine;
            ScriptToSetPrompt = new string[0];
        }

        protected REPL(REPL src)
        {
            Command = src.Command;
            Arguments = src.Arguments;
            NewLine = src.NewLine;
            PromptWithNewLine = src.PromptWithNewLine;
            ScriptToSetPrompt = src.ScriptToSetPrompt != null
                ? src.ScriptToSetPrompt.Clone() as string[]
                : null;
        }

        #endregion

        #region Properties

        public string Command { get; set; }
        public string Arguments { get; set; }
        public string NewLine { get; set; }
        public string PromptWithNewLine { get; set; }
        public string[] ScriptToSetPrompt { get; set; }
        public int? TimeoutMilliseconds;

        public int? ProcessID
        {
            get { return runtimeValues != null ? runtimeValues.Process.Id : (int?)null; }
        }

        #endregion

        #region ICloneable Members

        public REPL Clone()
        {
            return new REPL(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Data class

        class RuntimeValueSet
        {
            public Process Process;
            public StringBuilder OutputBuffer;
            public StringBuilder ErrorBuffer;
            public object OutputBufferLock;
            public object ErrorBufferLock;
            public string NewLine;
            public string PromptWithNewLine;
            public int? TimeoutMilliseconds;
        }

        #endregion

        #region Implementation

        RuntimeValueSet runtimeValues;

        public int Start()
        {
            var process = new Process();
            process.StartInfo.FileName = Command;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = new UTF8Encoding(false);
            process.StartInfo.StandardErrorEncoding = new UTF8Encoding(false);

            runtimeValues = new RuntimeValueSet
            {
                Process = process,
                OutputBuffer = new StringBuilder(),
                OutputBufferLock = new object(),
                ErrorBuffer = new StringBuilder(),
                ErrorBufferLock = new object(),
                TimeoutMilliseconds = TimeoutMilliseconds,
                NewLine = NewLine,
                PromptWithNewLine = PromptWithNewLine,
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;
                lock (runtimeValues.OutputBufferLock)
                {
                    runtimeValues.OutputBuffer.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;
                lock (runtimeValues.ErrorBufferLock)
                {
                    runtimeValues.ErrorBuffer.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (ScriptToSetPrompt != null)
            {
                foreach (var line in ScriptToSetPrompt)
                {
                    WriteLine(line);
                }
            }

            return process.Id;
        }

        public void SetTimeout(int? timeoutMilliseconds)
        {
            TimeoutMilliseconds = timeoutMilliseconds;
            if (runtimeValues == null) return;
            runtimeValues.TimeoutMilliseconds = timeoutMilliseconds;
        }

        public void WriteLine(string inputText)
        {
            if (runtimeValues == null) throw new InvalidOperationException();
            runtimeValues.Process.StandardInput.Write(inputText + NewLine);
            runtimeValues.Process.StandardInput.Flush();
        }

        public string ReadLine()
        {
            if (runtimeValues == null) throw new InvalidOperationException();

            // Wait for prompt
            Stopwatch sw = null;
            if (runtimeValues.TimeoutMilliseconds.HasValue)
            {
                sw = new Stopwatch();
                sw.Start();
            }
            do
            {
                lock (runtimeValues.ErrorBufferLock)
                {
                    string errorText = runtimeValues.ErrorBuffer.ToString();
                    runtimeValues.ErrorBuffer.Clear();
                    if (errorText.EndsWith(PromptWithNewLine)) break;
                    if (runtimeValues.TimeoutMilliseconds.HasValue)
                    {
                        if (sw.ElapsedMilliseconds > runtimeValues.TimeoutMilliseconds.Value)
                        {
                            throw new TimeoutException();
                        }
                    }
                }
            } while (true);
            if (runtimeValues.TimeoutMilliseconds.HasValue)
            {
                sw.Stop();
            }

            // Get standard output text
            string outputText = "";
            do
            {
                lock (runtimeValues.OutputBufferLock)
                {
                    outputText += runtimeValues.OutputBuffer.ToString();
                    runtimeValues.OutputBuffer.Clear();
                }
            } while (outputText.Length == 0);
            return outputText.TrimEnd('\r', '\n');
        }

        public void Stop(int processID)
        {
            if (runtimeValues == null) throw new InvalidOperationException();
            if (!runtimeValues.Process.HasExited)
            {
                runtimeValues.Process.Kill();
            }
            runtimeValues.Process.Close();
            runtimeValues = null;
        }

        #endregion
    }
}
