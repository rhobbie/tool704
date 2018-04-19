using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
namespace Tp2p7b
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: Tp2p7b input.tap output.tap");
                return;
            }
            using (TapeReader r = new TapeReader(args[0], false))
            using (TapeWriter w = new TapeWriter(args[1], true))
            {
                int ret;
                while ((ret = r.ReadRecord(out bool binary, out byte[] mrecord)) >= 0)
                {
                    if (ret == 0)
                        Console.WriteLine("Eof");
                    else
                        Console.WriteLine("{0} record, length {1}", binary ? "binary" : "BCD", mrecord.Length);
                    if (ret == 0)
                        w.WriteEOF();
                    else
                        w.WriteRecord(binary, mrecord);
                }
            }
            return;

        }
    }
}
