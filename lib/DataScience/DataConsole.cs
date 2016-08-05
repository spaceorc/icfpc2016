using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public static class DataConsole
    {
        static DataConsoleBuffer buffer;
        static DataConsole()
        {
            buffer = new DataConsoleBuffer();
            Console.WriteLine(buffer.Log.ToString());
            WriteLine("\n\n\n");
            WriteLine("====================================================================================================================================================================================================================");
            WriteLine("\n\n");
        }

        public static void Write(object data)
        {
            if (data != null)
                buffer.Add(data.ToString());
            else
                buffer.Add("NULL");
            Console.Write(data);
        }

        public static void WriteLine(object line)
        {
            if (line != null)
                buffer.Add(line.ToString() + "\r\n");
            else
                buffer.Add("NULL\r\n");
            Console.WriteLine(line);
        }

        public static void WriteLine()
        {
            buffer.Add("\r\n");
            Console.WriteLine();
        }
    }
}
