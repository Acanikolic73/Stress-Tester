using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StressTester
{
    public class CppRunner : Language
    {
        public override string Compile(string cppFile)
        {
            string exeFile = Path.ChangeExtension(cppFile, ".exe");
            ProcessStartInfo processStartInfo = new ProcessStartInfo("g++", cppFile + " -o " + exeFile)
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(processStartInfo))
            {
                string errors = process.StandardError.ReadToEnd();
                bool finished = process.WaitForExit(2000);
                if (!finished)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch (Exception ex) 
                    {
                        throw new Exception("Errors killed process " + ex);
                    }
                }
                if (!string.IsNullOrEmpty(errors))
                {
                    throw new Exception("Compilation errors:\n" + errors);
                }
            }
            return exeFile;
        }

        public override string Run(string exePath, string input)
        {
            return RunProgram.Run(exePath, input);
        }

    }
}
