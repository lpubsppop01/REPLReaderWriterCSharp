using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lpubsppop01.REPLReaderWriter
{
    public static class REPLTemplates
    {
        public static readonly REPL Python = new REPL
        {
            Command = "python",
            Arguments = "-i",
            NewLine = "\n",
            PromptWithNewLine = "lpubsppop01.REPLReaderWriter" + Environment.NewLine,
            ScriptToSetPrompt = new string[]
            {
                "import sys",
                "sys.ps1 = 'lpubsppop01.REPLReaderWriter\\n'"
            },
        };

        public static REPL GetByName(string name)
        {
            switch(name)
            {
                case "Python":
                    return Python;
            }
            return null;
        }
    }
}
