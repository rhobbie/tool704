using System;
using Tools704;

namespace CopyCards
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: CopyCards in.cbn[+in2.cbn ...] out.cbn");
                return;
            }            
            int retval = 0;
            string[] split = args[0].Split(new char[] { '+' });
            using (TapeWriter w = new TapeWriter(args[1], true))
                foreach (string rfile in split)
                {
                    Console.WriteLine(rfile);
                    using (TapeReader r = new TapeReader(rfile, true))
                        while ((retval = r.ReadRecord(out bool binary, out byte[] rrecord)) >= 0)
                        {
                            if (retval == 0)
                            {
                                Console.Error.WriteLine("invalid EOF");
                                return;
                            }
                            if (!binary)
                            {
                                Console.Error.WriteLine("not binary record");
                                return;
                            }
                            if (rrecord.Length != 160)
                            {
                                Console.Error.WriteLine("wrong record length");
                                return;
                            }
                            w.WriteRecord(binary, rrecord);
                        }
                }
            Console.WriteLine("   {0} written", args[1]);
        }
    }
}
