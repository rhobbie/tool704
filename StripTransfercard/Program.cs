using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tool704;
namespace StripTransfercard
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage StripTransfercard in.cbn[+in2.cbn ...] out.cbn");
                return;
            }
            bool cardtypeset = false;
            bool tfound = false;
            int retval = 0;
            BinaryCardConverter.CardType t0 = BinaryCardConverter.CardType.Full;
            string[] split = args[0].Split(new char[] { '+' });
            using (TapeWriter w = new TapeWriter(args[1], true))
                foreach (string rfile in split)
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
                                Console.Error.WriteLine("not binary");
                                return;
                            }
                            if (rrecord.Length != 160)
                            {
                                Console.Error.WriteLine("Wrong record length");
                                return;
                            }
                            CBNConverter.FromCBN(rrecord, out Card crd);
                            BinaryCardConverter.CardType t = BinaryCardConverter.GetCardType(crd);
                            if (tfound)
                            {
                                Console.Error.WriteLine("Transfercard not at end");
                                return;
                            }
                            switch (t)
                            {
                                case BinaryCardConverter.CardType.Full:
                                case BinaryCardConverter.CardType.Abs:
                                case BinaryCardConverter.CardType.Rel:
                                    if (cardtypeset && t0 != t)
                                    {
                                        Console.Error.WriteLine("Card type change");
                                        return;
                                    }
                                    w.WriteRecord(binary, rrecord);
                                    break;
                                case BinaryCardConverter.CardType.Transfer:
                                    if (cardtypeset && t0 != BinaryCardConverter.CardType.Abs)
                                    {
                                        Console.Error.WriteLine("Card type change");
                                        return;
                                    }
                                    Console.WriteLine("transfercard removed");
                                    tfound = true;
                                    break;
                                case BinaryCardConverter.CardType.RelTransfer:
                                    if (cardtypeset && t0 != BinaryCardConverter.CardType.Rel)
                                    {
                                        Console.Error.WriteLine("Card type change");
                                        return;
                                    }
                                    Console.WriteLine("transfercard removed");
                                    tfound = true;
                                    break;
                                default:
                                    Console.Error.WriteLine("wrong Card type");
                                    return;
                            }
                            if (!cardtypeset)
                            {
                                t0 = t;
                                cardtypeset = true;
                            }
                        }
                    
        }
    }
}
