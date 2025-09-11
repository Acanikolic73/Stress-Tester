using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTester
{
    public abstract class Language
    {
        public abstract string Compile(string file);
        public abstract string Run(string exeFile, string input);
    }
}
