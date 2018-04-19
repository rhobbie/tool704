using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;

namespace ShowTape
{
    class Program
    {
        static int numbin = 0;
        static void Printskipped()
        {
            if (numbin > 0)
            {
                Console.WriteLine("\\{0} Binary Record{1}skipped\\", numbin, numbin > 1 ? "s " : " ");
                numbin = 0;
            }
        }
        static void Main(string[] args)
        {
            if (args.Length > 2|| args.Length==0)
            {
                Console.Error.WriteLine("Usage: ShowTape Tape.tap  [linelength]");
                return;
            }
            int lenght = -1;
            if(args.Length==2)
            {
                if(!int.TryParse(args[1],out lenght))
                {
                    Console.Error.WriteLine("wrong linelength");
                    return;
                }
            }
            using (TapeReader r = new TapeReader(args[0], true))
            {
                int rtype;
                while ((rtype = r.ReadRecord(out bool binary, out byte[] mrecord)) >= 0)
                {
                    if (rtype == 0)
                    {
                        Printskipped();
                        Console.WriteLine("\\Eof\\");
                    }
                    else if (binary)
                        numbin++;
                    else
                    {
                        Printskipped();
                        string line = BcdConverter.BcdToString(mrecord);
                        if(lenght<=0)
                             Console.WriteLine(line.TrimEnd());
                        else
                            for(int i=0;i<line.Length;i+=lenght)
                            {
                                if(i+lenght<line.Length)
                                    Console.WriteLine(line.Substring(i,lenght).TrimEnd());
                                else
                                    Console.WriteLine(line.Substring(i).TrimEnd());
                            }
                    }
                }
                Printskipped();
            }
        }
    }
}
