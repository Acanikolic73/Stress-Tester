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
        Language language, randomGenerated;

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

        string fileName, Code;
        private async Task StressTest(string bruteFile, string optimizedFile)
        {
            rtbOutput.ForeColor = Color.Gray;
            for(int i = 1; i <= 100; i++)
            {
                //testing 100 inputs
                AppendOutput("Running on test " + i + " ...\n");
                string input = randomGenerated.GenerateTestCase(rtbBrute.Text);
                string outputBrute = await Task.Run(() => language.Run(bruteFile, input));
                string outputOpt = await Task.Run(() => language.Run(optimizedFile, input));
                outputBrute = outputBrute.Trim();
                outputOpt = outputOpt.Trim();
                if(outputBrute+"11" != outputOpt)
                {
                    rtbOutput.Clear();
                    rtbOutput.ForeColor = Color.Red;
                    AppendOutput("Mismatch found!\n");
                    AppendOutput("Input:\n" + input + "\n");
                    AppendOutput("Brute Output:\n" + outputBrute + "\n");
                    AppendOutput("Optimized Output:\n" + outputOpt + "\n");
                    button1.Enabled = true;
                    return;
                }
                
            }
            rtbOutput.Clear();
            rtbOutput.ForeColor = Color.Green;
            AppendOutput("All tests passed!");
            button1.Enabled = true;
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            rtbOutput.ForeColor = Color.Gray;
            rtbOutput.Clear();
            string bruteCode = rtbBrute.Text;     // Brute force code textbox
            string optCode = rtbOptimized.Text;   // Optimized code textbox
            string language2 = comboLanguage.SelectedItem.ToString();
            Code = bruteCode;
            
            if (string.IsNullOrWhiteSpace(bruteCode) || string.IsNullOrWhiteSpace(optCode) || language2 == null)
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

                if(comboLanguage.Text == "C++")
                {
                    brute += ".cpp";
                    opt += ".cpp";
                    bruteFile = Path.Combine(Path.GetTempPath(), brute);
                    optimizedFile = Path.Combine(Path.GetTempPath(), opt);
                }
                else if(comboLanguage.Text == "Python")
                {
                    brute += ".py";
                    opt += ".py";
                    bruteFile = Path.Combine(Path.GetTempPath(), brute);
                    optimizedFile = Path.Combine(Path.GetTempPath(), opt);
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
                language = new CppRunner();
                randomGenerated = new CppRunner();
                if(comboLanguage.Text == "C++")
                {
                    language = new CppRunner();
                    randomGenerated = new CppRunner();
                }
                else if (comboLanguage.Text == "Python")
                {
                    language = new PythonRunner();
                    randomGenerated = new PythonRunner();
                }
                 string bruteExe = language.Compile(bruteFile);
                 string optExe = language.Compile(optimizedFile);

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
