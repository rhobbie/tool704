using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        static void ungetRecord(bool binary, byte[] mrecord)
        {
            if (stored)
                throw new InvalidOperationException("record already stored");
            bin = binary;
            rec = mrecord;
            stored = true;
        }
        static void Unblock(byte[] irecord, int rsize, out byte[][] orecord)
        {
            if (irecord.Length % rsize != 0)
            {
                Console.WriteLine("invalid record size {0}, {1} ,{2}", irecord.Length, rsize, irecord.Length % rsize);
                orecord = null;
            }
            else
            {
                orecord = new byte[irecord.Length / rsize][];

                for (int i = 0, j = 0; i < irecord.Length; i += rsize, j++)
                {
                    orecord[j] = new byte[rsize];
                    Array.Copy(irecord, i, orecord[j], 0, rsize);
                }
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
            string dir= args[1];
            int count = 0;
            using (StreamWriter tx = new StreamWriter(dir + "index.txt"))
            using (TapeReader r = new TapeReader(tape, true))
            {
                rd = r;
                while (GetRecord(out bool binary, out byte[] mrecord) == 1)
                {
                    if (binary || mrecord.Length != 80)
                        throw new Exception("wrong Control card");
                    string descr = BcdConverter.BcdToString(mrecord);
                    tx.WriteLine(descr);
                    string type = descr.Substring(33, 2);
                    string filename = dir + descr.Substring(3, 2).Trim() + "_" + descr.Substring(6, 4).Trim() + "_" + descr.Substring(20, 4).Trim() + "." + type.Trim();
                    using (TapeWriter wr = new TapeWriter(filename, true))
                        if (type == "SY")
                        {
                            bool lastrecord = false;
                            do
                            {
                                int ret = GetRecord(out binary, out mrecord);
                                if (ret != 1)
                                    lastrecord = true;
                                else
                                {
                                    if (binary)
                                        throw new Exception("inexpected bin record");
                                    if (mrecord.Length == 80)
                                    {
                                        ungetRecord(binary, mrecord);
                                        lastrecord = true;
                                    }
                                    else
                                    {
                                        Unblock(mrecord, 80, out byte[][] orecord);
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
                            while (!lastrecord);
                        }
                        else if (type == "BI" || (type == "SI"))
                        {
                            bool lastrecord = false;                           

                            do
                            {

                                int ret = GetRecord(out binary, out mrecord);
                                if (ret != 1)
                                    lastrecord = true;
                                else
                                {
                                    if (!binary)
                                    {
                                        if (mrecord.Length == 80)
                                        {
                                            ungetRecord(binary, mrecord);
                                            lastrecord = true;
                                        }
                                        else
                                            throw new Exception("inexpected sym record");
                                    }
                                    else
                                    {
                                        Unblock(mrecord, 160, out byte[][] orecord);
                                        if (orecord != null)
                                            for (int i = 0; i < orecord.Length; i++)
                                            {
                                                wr.WriteRecord(true, orecord[i]);
                                                count++;
                                            }
                                    }
                                }
                            }
                            while (!lastrecord);                            
                        }
                        else
                            throw new Exception("wrong Control card");
                }
                if (GetRecord(out bool binary2, out byte[] mrecord2) == 1)
                    throw new Exception("Data after eof");
                rd = null;
            }
            Console.WriteLine("{0} Cards written", count);

        }
    }
}
