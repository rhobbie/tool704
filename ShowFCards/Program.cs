using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
namespace ShowFCards
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: ShowCards deck.cbn");
                return;
            }
            int cardno = 0;
            using (TapeReader r = new TapeReader(args[0], true))
            {
                int rtype;
                string label;
                while ((rtype = r.ReadRecord(out bool binary, out byte[] mrecord)) >= 0)
                {
                    cardno++;
                    if (rtype == 0)
                        Console.WriteLine("EOF");
                    else
                    {
                        if (!binary)
                        {
                            Console.Error.WriteLine("not binary record");
                            return;
                        }
                        if (HollerithConverter.CBNToString(mrecord, 72, 8, out label) > 0)
                            label = "";
                        CBNConverter.FromCBN(mrecord, out Card crd);
                        for (int i = 0; i < 24; i++)
                        {
                            Console.Write("             {0}", crd.C[i].ToString());
                            if (i == 0)
                                Console.WriteLine("  Card {0} FUL {1}", cardno, label);
                            else
                                Console.WriteLine();
                        }
                    }
                }
            }
        }
    }

}
