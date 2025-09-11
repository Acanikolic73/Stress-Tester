using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTester
{
    public class PythonRunner : Language
    {
        public override string Compile(string file)
        {
            string tempExe = Path.Combine(Path.GetTempPath(), "File.bat");
            File.WriteAllText(tempExe, "python " + file);
            return tempExe;

        }
        public override string Run(string exeFile, string input)
        {
            return RunProgram.Run(exeFile, input);
        }
    }
}
