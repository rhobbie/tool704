using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
namespace ShowCards
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
                    {
                        Console.WriteLine("EOF");
                    }
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
                        BinaryCardConverter.CardType t = BinaryCardConverter.GetCardType(crd);
                        switch (t)
                        {
                            case BinaryCardConverter.CardType.Full:
                                //if (HollerithConverter.CBNToString(mrecord, 0, 72, out string txt) == 0) /* check for Hollerith card */
                                  //  Console.WriteLine("{0} Card {1} HOL {2}", txt, cardno,label);
                                //else
                                //{
                                    for (int i = 0; i < 24; i++)
                                    {
                                        Console.Write("             {0}", crd.C[i].ToString());
                                        if (i == 0)
                                            Console.WriteLine("  Card {0} FUL {1}", cardno, label);
                                        else
                                            Console.WriteLine();
                                    }
                                //}
                                break;
                            case BinaryCardConverter.CardType.Abs:
                                for (int i = 0; i < crd.W9L.D; i++)
                                {
                                    Console.Write("       {0} {1}", Convert.ToString(crd.W9L.A + i, 8).PadLeft(5, '0'), crd.C[i + 2].ToString());
                                    if (i == 0)
                                        Console.WriteLine(" Card {0} ABS {1}", cardno, label);
                                    else
                                        Console.WriteLine();
                                }
                                break;
                            case BinaryCardConverter.CardType.Rel:
                                BinaryCardConverter.RelType[] rel = BinaryCardConverter.GetRelData(crd);
                                for (int i = 0; i < crd.W9L.D; i++)
                                {
                                    char rd = ' ', ra = ' ';
                                    switch (rel[i * 2])
                                    {
                                        case BinaryCardConverter.RelType.absolute:
                                            rd = ' ';
                                            break;
                                        case BinaryCardConverter.RelType.relocatable_direct:
                                            rd = 'R';
                                            break;
                                        case BinaryCardConverter.RelType.relocatable_complemented:
                                            rd = 'C';
                                            break;
                                    }
                                    switch (rel[i * 2 + 1])
                                    {
                                        case BinaryCardConverter.RelType.absolute:
                                            ra = ' ';
                                            break;
                                        case BinaryCardConverter.RelType.relocatable_direct:
                                            ra = 'R';
                                            break;
                                        case BinaryCardConverter.RelType.relocatable_complemented:
                                            ra = 'C';
                                            break;
                                    }
                                    Console.Write("   {0} {1} {2} {3}", rd, ra, Convert.ToString(crd.W9L.A + i, 8).PadLeft(5, '0'), crd.C[i + 4].ToString());
                                    if (i == 0)
                                        Console.WriteLine(" Card {0} REL {1}", cardno, label);
                                    else
                                        Console.WriteLine();
                                }
                                break;
                            case BinaryCardConverter.CardType.Transfer:
                                Console.WriteLine("               TRANSFER {0} Card {1} ABS {2}", Convert.ToString(crd.W9L.A, 8).PadLeft(5, '0'), cardno, label);
                                break;
                            case BinaryCardConverter.CardType.RelTransfer:
                                Console.WriteLine("               TRANSFER {0} Card {1} REL {2}", Convert.ToString(crd.W9L.A, 8).PadLeft(5, '0'), cardno, label);
                                break;
                            default:
                                Console.Error.WriteLine("Invalid Card Type");
                                return;
                        }
                    }
                }
            }
        }
    }
}
