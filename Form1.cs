using System;
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

        private string CompileCpp(string cppFile)
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
                process.WaitForExit();
                if(!string.IsNullOrEmpty(errors))
                {
                    throw new Exception("Compalation errors:\n" + errors);
                }
            }
            return exeFile;
        }

        private string RunProgram(string exePath, string input)
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
                process.WaitForExit();
                return output.Trim();
            }
        }

        private string GenerateTestCase()
        {
            Random rnd = new Random();
            int n = rnd.Next(1, 10);
            string testCase = n.ToString() + "\n";
            for (int i = 1; i <= n; i++)
            {
                testCase += rnd.Next(1, 100) + " ";
            }
            return testCase.Trim();
        }

        private async Task StressTest(string bruteFile, string optimizedFile)
        {
            rtbOutput.ForeColor = Color.Gray;
            for(int i = 1; i <= 100; i++)
            {
                //testing 100 inputs
                AppendOutput("Running on test " + i + " ...\n");
                string input = GenerateTestCase();
                string outputBrute = await Task.Run(() => RunProgram(bruteFile, input));
                string outputOpt = await Task.Run(() => RunProgram(optimizedFile, input));
                if(outputBrute != outputOpt)
                {
                    rtbOutput.Clear();
                    rtbOutput.ForeColor = Color.Red;
                    AppendOutput("Mismatch found!\n");
                    AppendOutput("Input:\n" + input + "\n");
                    AppendOutput("Brute Output: " + outputBrute + "\n");
                    AppendOutput("Optimized Output: " + outputOpt + "\n");
                    return;
                }
                
            }
            rtbOutput.Clear();
            rtbOutput.ForeColor = Color.Green;
            AppendOutput("All tests passed!");
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            string bruteCode = rtbBrute.Text;     // Brute force code textbox
            string optCode = rtbOptimized.Text;   // Optimized code textbox
            string language = comboLanguage.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(bruteCode) || string.IsNullOrWhiteSpace(optCode) || language == null)
            {
                MessageBox.Show("Please paste both codes and select a language!");
                return;
            }

            try
            {
                //make a temporary files brute and optimized
                string bruteFile = Path.Combine(Path.GetTempPath(), "brute.cpp");
                string optimizedFile = Path.Combine(Path.GetTempPath(), "opt.cpp");

                // write text in file
                File.WriteAllText(bruteFile, bruteCode);
                File.WriteAllText(optimizedFile, optCode);

                //compile and convert to exe file 
                string bruteExe = CompileCpp(bruteFile);
                string optExe = CompileCpp(optimizedFile);

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
