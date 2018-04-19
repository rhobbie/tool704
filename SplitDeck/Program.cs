using System;
using System.IO;

namespace SplitDeck
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] record = new byte[160];
            if (args.Length != 4)
            {
                Console.Error.WriteLine("usage: SplitDeck in.cbn number_of_cards_which_go_into_out1 out1.cbn out2.cbn");
                Console.Error.WriteLine("   or  SplitDeck in.cbn -number_of_cards_which_go_into_out2 out1.cbn out2.cbn");
                return;
            }
            if (!int.TryParse(args[1], out int num))
            {
                Console.Error.WriteLine("Second argument must be a number");
                return;
            }
            using (FileStream r = new FileStream(args[0], FileMode.Open))
            {
                long length = r.Length;
                if (length % 160 != 0)
                {
                    Console.Error.WriteLine("Length of input file not multiple of 160.");
                    return;
                }
                int numrec = (int)(length / 160);
                if(num<0)
                    num = numrec - num;                
                if (num <= 0 || num >= numrec)
                {
                    Console.Error.WriteLine("wrong split position");
                    return;
                }
                using (FileStream w1 = new FileStream(args[2], FileMode.Create))
                    for (int i = 0; i < num; i++)
                    {
                        r.Read(record, 0, 160);
                        w1.Write(record, 0, 160);
                    };
                using (FileStream w2 = new FileStream(args[3], FileMode.Create))
                    for (int i = num; i < numrec; i++)
                    {
                        r.Read(record, 0, 160);
                        w2.Write(record, 0, 160);
                    }
            }
        }
    }
}
