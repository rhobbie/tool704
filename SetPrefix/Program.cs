using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tool704;

namespace SetPrefix
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length!=3&&args.Length!=2)
            {
                Console.Error.WriteLine("Usage SetPrefix prefix in.cbn [out.cbn]");
                return;
            }
            if(args[0].Length!=1)
            {
                Console.Error.WriteLine("invalid prefix");
                return;
            }
            char pch = args[0][0];
            if(pch<'0'||pch>'7')
            {
                Console.Error.WriteLine("invalid prefix value");
                return;
            }
            uint pfx = (uint)(pch - '0');
            byte[] wrecord;
            using (TapeReader r = new TapeReader(args[1], true))
            {
                if (r.ReadRecord(out bool binary, out byte[] rrecord) != 1||!binary)
                {
                    Console.Error.WriteLine("invalid input file");
                    return;
                }                
                if (r.ReadRecord(out bool b2, out byte[] m2) != -1)
                {
                    Console.Error.WriteLine("not a single card file");
                    return;
                }
                CBNConverter.FromCBN(rrecord, out Card C);
                if (!BinaryCardConverter.VerifyChecksum(C))
                {
                    Console.Error.WriteLine("Wrong checksum in card");
                    return;
                }
                C.C[0].P = pfx;
                BinaryCardConverter.UpdateChecksum(C);
                wrecord = CBNConverter.ToCBN(C);
                Array.Copy(rrecord, 72*2, wrecord, 72*2, 8*2);
            }
            using (TapeWriter w = new TapeWriter(args[args.Length-1], true))
                w.WriteRecord(true, wrecord);
            Console.WriteLine("Prefix of W9L updated to {0}", pfx);
        }
    }
}
