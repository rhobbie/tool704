using System;
using System.IO;
using Tools704;
namespace TapeExtract
{
    class Program
    {
        static bool bin;
        static byte[] rec;
        static bool stored = false;
        static TapeReader rd;
        static int GetRecord(out bool binary, out byte[] mrecord)
        {
            int retvalue;
            if (stored)
            {
                binary = bin;
                mrecord = rec;
                retvalue = 1;
                stored = false;
            }
            else
                retvalue = rd.ReadRecord(out binary, out mrecord);
            return retvalue;
        }
        static void UngetRecord(bool binary, byte[] mrecord)
        {
            if (stored)
                throw new InvalidOperationException("record already stored");
            bin = binary;
            rec = mrecord;
            stored = true;
        }
        static void Deblock(byte[] irecord, int rsize, out byte[][] orecord)
        {
            if (irecord.Length % rsize != 0)
            {
                byte[] tmp = new byte[irecord.Length + rsize-(irecord.Length % rsize)];
                irecord.CopyTo(tmp, 0);
                irecord = tmp;
            }
            orecord = new byte[irecord.Length / rsize][];
            for (int i = 0, j = 0; i < irecord.Length; i += rsize, j++)
            {
                orecord[j] = new byte[rsize];
                Array.Copy(irecord, i, orecord[j], 0, rsize);
            }
        }
        static void Main(string[] args)
        {
            byte[] trecord = new byte[160];
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: TapeExtract tapefile dir");
            }
            string tape = args[0];
            string dir = args[1]+"\\";
            int count = 0;
            bool eof = false;
            using (StreamWriter tx = new StreamWriter(dir + "index.txt"))
            using (TapeReader r = new TapeReader(tape, true))
            {
                rd = r;
                while (!eof&&GetRecord(out bool binary, out byte[] mrecord) == 1)
                {
                    if (binary || (mrecord.Length != 80&& mrecord.Length != 84))
                        throw new Exception("wrong Control card");
                    string descr = BcdConverter.BcdToString(mrecord).Substring(0,80);
                    tx.WriteLine(descr.TrimEnd());
                    descr = descr.Replace('/', '_');
                    descr = descr.Replace('.', '_');
                    descr = descr.Replace('+', '_');
                    string filename;
                    if (descr[5] == ' ')
                        filename = dir + descr.Substring(3, 2).Trim() + "_" + descr.Substring(6, 5).Trim();
                    else
                        filename = dir + descr.Substring(3, 8).Trim();
                    filename+= "_" + descr.Substring(20, 4).Trim() + "." + descr.Substring(33, 2).Trim();
                    
                    using (TapeWriter wr = new TapeWriter(filename, true))
                    {
                        bool lastrecord = false;
                        do
                        {
                            int ret = GetRecord(out binary, out mrecord);
                            if (ret != 1)
                            { 
                                eof = true;
                                lastrecord = true;
                            }
                            else
                            {
                                if (binary)
                                {
                                    Deblock(mrecord, 160, out byte[][] orecord);
                                    if (orecord != null)
                                        for (int i = 0; i < orecord.Length; i++)
                                        {
                                            wr.WriteRecord(true, orecord[i]);
                                            count++;
                                        }
                                }
                                else
                                {
                                    if (mrecord.Length == 80|| mrecord.Length == 84)
                                    {
                                        UngetRecord(binary, mrecord);
                                        lastrecord = true;
                                    }
                                    else
                                    {
                                        Deblock(mrecord, 80, out byte[][] orecord);
                                        if (orecord != null)
                                        {
                                            for (int i = 0; i < orecord.Length; i++)
                                            {
                                                HollerithConverter.BCDToCBN(orecord[i], 0, trecord);
                                                wr.WriteRecord(true, trecord);
                                                count++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        while (!lastrecord);
                    }
                }
                rd = null;
            }
            Console.WriteLine("{0} cards written", count);

        }
    }
}
