using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;

namespace CleanDeck
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage CleanDeck  n.cbn out.cbn");
                return;
            }
            using (TapeReader r = new TapeReader(args[0], true))
            using (TapeWriter w = new TapeWriter(args[1], true))
            {
                int retval;
                int cnt = 0;
                while ((retval = r.ReadRecord(out bool binary, out byte[] rrecord)) >= 0)
                {
                    cnt++;
                    if (retval == 0)
                    {
                        Console.Error.WriteLine("invalid EOF");
                        return;
                    }
                    if (!binary)
                    {
                        Console.Error.WriteLine("not binary");
                        return;
                    }
                    if (rrecord.Length != 160)
                    {
                        Console.Error.WriteLine("Wrong record length");
                        return;
                    }
                    CBNConverter.FromCBN(rrecord,out Card crd);
                    int i;
                    for (i = 0; i < 24; i++)
                        if (crd.C[i].LW != 0)
                            break;
                    if (i == 24)
                        continue; /* blank card */
                    if (crd.W9L.LW == 0xFFFFFFFFFL)
                        continue; /* 9L has all ones */
                    w.WriteRecord(true, rrecord);
                }
            }

        }
    }
}
