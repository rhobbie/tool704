using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
using System.IO;
namespace WriteTape
{
    class Program
    {
        static string ExpandTabs(string input, int tabLength)
        {
            if (input == null || input == "")
                return input;
            string[] parts = input.Split('\t');
            if (parts.Length == 1)
                return input;
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (string part in parts)
            {
                count++;
                sb.Append(part);
                if (count < parts.Length)
                    sb.Append(new string(' ', tabLength - (part.Length % tabLength)));                
            }
            return sb.ToString();
        }
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: WriteTape input.txt output.tap");
                return;
            }
            using (StreamReader r = new StreamReader(args[0]))
            using (TapeWriter w = new TapeWriter(args[1], true))
            {
                while (!r.EndOfStream)
                {
                    string line = ExpandTabs(r.ReadLine().ToUpper(), 8).PadRight(80).Substring(0, 80).PadRight(84);
                    w.WriteRecord(false, BcdConverter.StringToBcd(line));
                }
                w.WriteEOF();
            }
        }
    }
}
