using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    class DataConsoleBuffer
    {
        const string LogFileName = "log.txt";

        public StringBuilder Log = new StringBuilder();

        public DataConsoleBuffer()
        {
            if (File.Exists(LogFileName))
                Log.Append(File.ReadAllText(LogFileName));
        }

        public void Add(string s)
        {
            Log.Append(s);
        }

        ~DataConsoleBuffer()
        {
            File.WriteAllText(LogFileName, Log.ToString(), Encoding.UTF8);
        }
    }
}
