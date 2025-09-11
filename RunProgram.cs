using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTester
{
    public static class RunProgram
    {
        public static string Run(string exePath, string input)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath)
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                return output.Trim();
            }
        }
    }
}
