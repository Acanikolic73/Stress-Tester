using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace StressTester
{
    public class PythonRunner : Language
    {
        public override string Compile(string file)
        {
            /*string name = Guid.NewGuid().ToString() + ".bat";
            string tempExe = Path.Combine(Path.GetTempPath(), name);
            File.WriteAllText(tempExe, "@echo off\n");
            File.WriteAllText(tempExe, "python " + file);*/
            string name = Guid.NewGuid().ToString() + ".bat";
            string tempExe = Path.Combine(Path.GetTempPath(), name);

            // napiši ceo sadržaj odjednom
            string bat = "@echo off\r\n" +
                         "py \"" + file + "\"\r\n";    // ili "python" umesto "py"
            File.WriteAllText(tempExe, bat, Encoding.UTF8);

            return tempExe;
        }
        public override string Run(string exeFile, string input)
        {
            return RunProgram.Run(exeFile, input);
        }

        private string Get(string code)
        {
            string result = "import random\r\n";
            result += code;
            return result;
        }

        private string ReverseCode(string code)
        {
            code = Get(code);
            string result = "";
            int n = code.Length;
            for(int i = 0; i < n; i++)
            {
                if(i + 6 < n && code.Substring(i, 7) == "input()")
                {
                    result += "print(random.randint(1, 10))";
                    i += 6;
                } else
                {
                    result += code[i];
                }
            }
            return result;
        }

        private string Exe = "", fileName = "";

        public override string GenerateTestCase(string code)
        {
            string Code = ReverseCode(code);
            if(fileName == "")
            {
                fileName = Guid.NewGuid().ToString() + ".py";
                fileName = Path.Combine(Path.GetTempPath(), fileName);
                File.WriteAllText(fileName, Code);
            }
            if(Exe == "")
            {
                Exe = Compile(fileName);
            }
            string input = Run(Exe, "");
            return input;
        }
    }
}
