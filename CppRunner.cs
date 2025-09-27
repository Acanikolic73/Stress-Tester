using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private string Get(string code)
        {
            string result = "";
            result += "#include <ctime>\r\n";
            result += "#include <random>\r\n";
            result += "std::mt19937 rng(time(0));\r\n";
            result += "int random() {\r\n";
            result += "return rng() % 10;\r\n";
            result += "}\n";
            result += code;
            return result;
        }

        private string ReverseCode(string code)
        {
            string result = "";
            code = Get(code);
            for (int i = 0; i < code.Length; i++)
            {
                if (i + 3 < code.Length)
                {
                    string substring = code.Substring(i, 4);
                    bool Is = false;
                    for (int j = i + 4; j < code.Length; j++)
                    {
                        if (code[j] == ' ') continue;
                        if (code[j] == '<') Is = true;
                        else break;
                    }
                    if (substring == "cout" && Is)
                    {
                        while (i < code.Length)
                        {
                            if (code[i] == ';')
                            {
                                i++;
                                break;
                            }
                            i++;
                        }
                        continue;
                    }
                    else
                    {
                        goto here;
                    }
                }
            here:
                if (i + 2 < code.Length)
                {
                    string substring = code.Substring(i, 3);
                    bool Is = false;
                    for (int j = i + 3; j < code.Length; j++)
                    {
                        if (code[j] == ' ') continue;
                        if (code[j] == '>') Is = true;
                        else break;
                    }
                    if (substring == "cin" && Is)
                    {
                        i += 3;
                        int cnt = 0;
                        string var = "";
                        while (i < code.Length)
                        {
                            if (code[i] == '>')
                            {
                                cnt++;
                                if (cnt > 1 && cnt % 2 == 1)
                                {
                                    result += "cout << (" + var + "=random()) << ' ';\n";
                                    var = "";
                                }
                            }
                            else if (code[i] == ';')
                            {
                                result += "cout << (" + var + "=random()) << ' ';\n";
                                i++;
                                break;
                            }
                            else
                            {
                                var += code[i];
                            }
                            i++;
                        }
                    }
                    else
                    {
                        result += code[i];
                    }
                }
                else
                {
                    result += code[i];
                }
            }
            return result;
        }

        private string Exe = "", fileName = "";

        public override string GenerateTestCase(string Code)
        {
            if(fileName == "")
            {
                fileName = Guid.NewGuid().ToString() + ".cpp";
                fileName = Path.Combine(Path.GetTempPath(), fileName);
                File.WriteAllText(fileName, ReverseCode(Code));
            }
            if (Exe == "")
            {
                Exe = Compile(fileName);
            }
            string input = Run(Exe, "");
            return input;
        }

    }
}
