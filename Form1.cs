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

        Language test;

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
            rtbOutput.ForeColor = Color.Gray;
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
                string bruteFile;
                string optimizedFile;

                if(comboLanguage.Text == "C++")
                {
                    bruteFile = Path.Combine(Path.GetTempPath(), "brute.cpp");
                    optimizedFile = Path.Combine(Path.GetTempPath(), "opt.cpp");
                }
                else if(comboLanguage.Text == "Python")
                {
                    bruteFile = Path.Combine(Path.GetTempPath(), "brute.py");
                    optimizedFile = Path.Combine(Path.GetTempPath(), "opt.py");
                }
                else
                {
                    bruteFile = Path.Combine(Path.GetTempPath(), "brute.cpp");
                    optimizedFile = Path.Combine(Path.GetTempPath(), "opt.cpp");
                }

                // write text in file
                File.WriteAllText(bruteFile, bruteCode);
                File.WriteAllText(optimizedFile, optCode);

                //compile and convert to exe file 
                test = new CppRunner();
                if(comboLanguage.Text == "C++")
                {
                    test = new CppRunner();
                }
                else if (comboLanguage.Text == "Python")
                {
                    test = new PythonRunner();
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
