using System;
using Tools704;
namespace PatchCard
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] loc1 = new string[] { "9L", "9R", "8L", "8R", "7L", "7R", "6L", "6R", "5L", "5R", "4L", "4R", "3L", "3R", "2L", "2R", "1L", "1R", "0L", "0R", "11L", "11R", "12L", "12R" };
            string[] loc2 = new string[] { "","A","T","D","P","M","S" };
            int[] startbit=new int[]     {  0,  0, 15, 18, 33,  0, 35 };
            int[] bitlen = new[]         { 36, 15,  3, 15,  3, 35,  1 };
            if (args.Length != 3 && args.Length != 2)
            {
                Console.Error.WriteLine("Usage PatchCard location=value,... in.cbn [out.cbn]");
                Console.Error.Write("location is x or xy, where x=9L,9R ... 0L,0R, ... 12L,12R");
                Console.Error.WriteLine(" and y=A,T,D,P,M,S,1,2,...,36");
                Console.Error.WriteLine("value is octal number");
                return;
            }


            
            byte[] wrecord;
            using (TapeReader r = new TapeReader(args[1], true))
            {
                if (r.ReadRecord(out bool binary, out byte[] rrecord) != 1 || !binary)
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
                bool cs = BinaryCardConverter.VerifyChecksum(C);
                string[] lis1 = args[0].Split(new char[] { ',' });
                foreach (string a0 in lis1)
                {
                    string[] lis = a0.Split(new char[] { '=' });
                    if (lis.Length != 2)
                    {
                        Console.Error.WriteLine("= missing");
                        return;
                    }
                    string loc = lis[0].ToUpper();
                    int pos = -1;
                    for (int i = 0; i < loc1.Length; i++)
                    {
                        if (loc.Length >= loc1[i].Length && loc.StartsWith(loc1[i]))
                        {
                            pos = i;
                            break;
                        }
                    }
                    if (pos == -1)
                    {
                        Console.Error.WriteLine("wrong location");
                        return;
                    }
                    loc = loc.Substring(loc1[pos].Length);
                    int type = -1;
                    int bpos = 0;
                    int blen = 0;
                    if (loc.Length > 0)
                    {
                        for (int i = 0; i < loc2.Length; i++)
                        {
                            if (loc == loc2[i])
                            {
                                type = i;
                                bpos = startbit[i];
                                blen = bitlen[i];
                                break;
                            }
                        }
                        if (type == -1)
                        {
                            if (!int.TryParse(loc, out int result) || result < 1 || result > 36)
                            {
                                Console.Error.WriteLine("wrong location");
                                return;
                            }
                            bpos = 36 - result;
                            blen = 1;
                        }
                    }
                    long value = Convert.ToInt64(lis[1], 8);
                    ulong imask = ((1ul << 36) - 1ul) - (((1ul << blen) - 1ul) << bpos);
                    if (value < 0 || value >= (1 << blen))
                    {
                        Console.Error.WriteLine("wrong value");
                        return;
                    }
                    ulong uvalue = (ulong)value << bpos;
                    
                    ulong oldvalue = C.C[pos].LW;
                    C.C[pos].LW = ((C.C[pos].LW & imask) | uvalue);
                    ulong newvalue = C.C[pos].LW;
                    Console.WriteLine("{0} updated from {1} to {2}", loc1[pos], Convert.ToString((long)oldvalue, 8).PadLeft(12, '0'), Convert.ToString((long)newvalue, 8).PadLeft(12, '0'));
                }
                if(cs)
                { 
                    BinaryCardConverter.UpdateChecksum(C);
                    Console.WriteLine("Checksum updated");
                }
                wrecord = CBNConverter.ToCBN(C);
                Array.Copy(rrecord, 72 * 2, wrecord, 72 * 2, 8 * 2);
            }
            using (TapeWriter w = new TapeWriter(args[args.Length - 1], true))
                w.WriteRecord(true, wrecord);
            
        }
    }
}
