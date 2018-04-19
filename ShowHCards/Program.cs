using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools704;
using System.IO;
namespace ShowHCards
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: ShowHCards input.cbn");
                return;
            }
            using (TapeReader r = new TapeReader(args[0], true))
            {
                int ret;
                while((ret=r.ReadRecord(out bool binary,out byte[] mrecord))>=0)
                    if(ret==0)
                        Console.WriteLine("\\Eof\\");
                    else
                    {
                        if (mrecord.Length != 160 || !binary)
                            Console.Error.WriteLine("\\invalid {0} record with length={1}\\", binary?"binary":"BCD",mrecord.Length );
                        else
                        {
                            HollerithConverter.CBNToString(mrecord, 0, 80, out string s);
                            Console.WriteLine(s.TrimEnd());
                        }
                    }
            }
        }
    }
}
