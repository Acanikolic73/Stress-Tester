using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace StressTester
{
    public partial class Form1 : Form
    {

        Random rnd = new Random(); // global for no repeating
        Language test, randomGenerated;

        public Form1()
        {
            InitializeComponent();
        }

        private void AppendOutput(string output)
        {
            rtbOutput.AppendText(output + Environment.NewLine);
            rtbOutput.SelectionStart = rtbOutput.Text.Length;
            rtbOutput.ScrollToCaret();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            label1.ForeColor = Color.FromArgb(51, 51, 51);
            label2.ForeColor = Color.FromArgb(51, 51, 51);
            label3.ForeColor = Color.FromArgb(51, 51, 51);
            label4.ForeColor = Color.FromArgb(51, 51, 51);
            panel1.BackColor = Color.FromArgb(74, 144, 226);
            button1.BackColor = Color.FromArgb(74, 144, 226);
            comboLanguage.SelectedIndex = 0;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private string Get(string code)
        {
            string result = "";
            result += "#include <bits/stdc++.h>\r\n";
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
            for(int i = 0; i < code.Length; i++)
            {
                if(i + 3 < code.Length)
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
                                if(cnt > 1 && cnt % 2 == 1)
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

        string fileName, Code;
        private string GenerateTestCase()
        {
            //MessageBox.Show(ReverseCode(Code));
            File.WriteAllText(fileName, ReverseCode(Code));
            string randomExe = randomGenerated.Compile(fileName);
            string input = randomGenerated.Run(randomExe, "");
            File.Delete(randomExe);
            return input;
        }

        private async Task StressTest(string bruteFile, string optimizedFile)
        {
            rtbOutput.ForeColor = Color.Gray;
            for(int i = 1; i <= 100; i++)
            {
                //testing 100 inputs
                AppendOutput("Running on test " + i + " ...\n");
                string input = GenerateTestCase();
                string outputBrute = await Task.Run(() => test.Run(bruteFile, input));
                string outputOpt = await Task.Run(() => test.Run(optimizedFile, input));
                outputBrute = outputBrute.Trim();
                outputOpt = outputOpt.Trim();
                if(outputBrute != outputOpt)
                {
                    rtbOutput.Clear();
                    rtbOutput.ForeColor = Color.Red;
                    AppendOutput("Mismatch found!\n");
                    AppendOutput("Input:\n" + input + "\n");
                    AppendOutput("Brute Output:\n" + outputBrute + "\n");
                    AppendOutput("Optimized Output:\n" + outputOpt + "\n");
                    return;
                }
                
            }
            rtbOutput.Clear();
            rtbOutput.ForeColor = Color.Green;
            AppendOutput("All tests passed!");
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            rtbOutput.ForeColor = Color.Gray;
            rtbOutput.Clear();
            string bruteCode = rtbBrute.Text;     // Brute force code textbox
            string optCode = rtbOptimized.Text;   // Optimized code textbox
            string language = comboLanguage.SelectedItem.ToString();
            Code = bruteCode;
            
            if (string.IsNullOrWhiteSpace(bruteCode) || string.IsNullOrWhiteSpace(optCode) || language == null)
            {
                MessageBox.Show("Please paste both codes and select a language!");
                return;
            }

            try
            {
                //make a temporary files brute and optimized
                string bruteFile;
                string optimizedFile;

                //if some process crash, program will work property
                string brute = Guid.NewGuid().ToString();
                string opt = Guid.NewGuid().ToString();
                string rndm = Guid.NewGuid().ToString();

                if(comboLanguage.Text == "C++")
                {
                    brute += ".cpp";
                    opt += ".cpp";
                    rndm += ".cpp";
                    bruteFile = Path.Combine(Path.GetTempPath(), brute);
                    optimizedFile = Path.Combine(Path.GetTempPath(), opt);
                    fileName = Path.Combine(Path.GetTempPath(), rndm);
                }
                else if(comboLanguage.Text == "Python")
                {
                    brute += ".py";
                    opt += ".py";
                    rndm += ".py";
                    bruteFile = Path.Combine(Path.GetTempPath(), brute);
                    optimizedFile = Path.Combine(Path.GetTempPath(), opt);
                    fileName = Path.Combine(Path.GetTempPath(), rndm);
                }
                else
                {
                    //this never will be happen
                    bruteFile = Path.Combine(Path.GetTempPath(), "brute.cpp");
                    optimizedFile = Path.Combine(Path.GetTempPath(), "opt.cpp");
                }

                // write text in file
                File.WriteAllText(bruteFile, bruteCode);
                File.WriteAllText(optimizedFile, optCode);

                //compile and convert to exe file 
                test = new CppRunner();
                randomGenerated = new CppRunner();
                if(comboLanguage.Text == "C++")
                {
                    test = new CppRunner();
                    randomGenerated = new CppRunner();
                }
                else if (comboLanguage.Text == "Python")
                {
                    test = new PythonRunner();
                    randomGenerated = new PythonRunner();
                }
                 string bruteExe = test.Compile(bruteFile);
                 string optExe = test.Compile(optimizedFile);

                 await StressTest(bruteExe, optExe);

                 // delete temporary files
                 File.Delete(bruteExe);
                 File.Delete(optExe);
            }
            catch (Exception ex)
            {
                rtbOutput.AppendText("Error: " + ex.Message + "\n");
            }
        }
    }
}
