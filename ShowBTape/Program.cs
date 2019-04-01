using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
namespace ShowBTape
{
    class Program
    {
        static int numbcd = 0;
        static void Printskipped()
        {
            if (numbcd > 0)
            {
                Console.WriteLine("\\{0} BCD Record{1}skipped\\", numbcd, numbcd > 1 ? "s " : " ");
                numbcd = 0;
            }
        }
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: ShowBTape Tape.tap ");
                return;
            }
            using (TapeReader r = new TapeReader(args[0], true))
            {
                int rtype;
                int file = 1;
                int record = 1;
                while ((rtype = r.ReadRecord(out bool binary, out byte[] mrecord)) >= 0)
                {
                    if (rtype == 0)
                    {
                        Printskipped();
                        Console.WriteLine("\\Eof\\");
                        file++;
                        record = 1;
                    }
                    else if (!binary)
                        numbcd++;
                    else
                    {
                        Printskipped();
                        for (int i = 0; i < mrecord.Length; i += 6)
                        {
                            Console.Write("      ");

                            ulong x = 0;
                            for (int j = i; j < i + 6; j++)
                            {
                                x <<= 6;
                                if (j < mrecord.Length)
                                    x |= mrecord[j];
                            }
                            W704 WRD = new W704 { LW = x };
                            Console.Write("       {0} {1}", Convert.ToString(i / 6, 8).PadLeft(5, '0'), WRD.ToString());
                            if (i == 0)
                                Console.Write(" File {0} Record {1}", file, record);
                            Console.WriteLine();
                        }

                    }
                    record++;
                }
                Printskipped();
            }
        }
    }
}

