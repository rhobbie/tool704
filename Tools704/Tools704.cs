using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace Tools704
{
    public struct W704 /* 36 Bit Word with access to Adress, Tag, Decrement, Prefix,  Magnitude, Sign, Logical Word, Arithmetical Word */
    {

        const int alen = 15; /* Adress length */
        const int tlen = 3; /* Tag length */
        const int dlen = 15; /* Decrement length */
        const int plen = 3; /* Prefix length */
        const int wlen = alen + tlen + dlen + plen; /* Word length */
        const int slen = 1; /* Sign length */
        const int mlen = wlen - slen; /* Magnitude length */

        const int apos = 0; /* bit pos for Adress*/
        const int tpos = alen; /* bit pos for Tag*/
        const int dpos = tpos + tlen; /* bit pos for Decrement*/
        const int ppos = dpos + dlen; /* bit pos for Prefix*/
        const int mpos = 0;           /* bit pos for Magniude */
        const int spos = wlen - slen; /* bit pos for Sign */

        const ulong a0mask = (1UL << alen) - 1UL; /* mask for Adress at bit 0*/
        const ulong t0mask = (1UL << tlen) - 1UL; /* mask for Tag at bit 0*/
        const ulong d0mask = (1UL << dlen) - 1UL; /* mask for Decrement at bit 0 */
        const ulong p0mask = (1UL << plen) - 1UL; /* mask for Prefix at bit 0 */
        const ulong wmask = (1UL << wlen) - 1UL;  /* mask for Word */
        const ulong m0mask = (1UL << mlen) - 1UL; /* mask for Magnitude at bit 0*/
        const ulong s0mask = (1UL << slen) - 1UL; /* mask for Sign at bit 0 */

        const ulong awmask = a0mask << apos; /* mask for Adress in word */
        const ulong twmask = t0mask << tpos; /* mask for Tag in word */
        const ulong dwmask = d0mask << dpos; /* mask for Decrement in word */
        const ulong pwmask = p0mask << ppos; /* mask for Prefix in word */
        const ulong mwmask = m0mask << mpos; /* mask for Magnitude in word  */
        const ulong swmask = s0mask << spos; /* mask for Sign in word */

        const ulong iawmask = (wmask ^ awmask); /* inv mask for Adress in word */
        const ulong itwmask = (wmask ^ twmask); /* inv mask for Tag in word  */
        const ulong idwmask = (wmask ^ dwmask); /* inv mask for Decrement in word  */
        const ulong ipwmask = (wmask ^ pwmask); /* inv mask for Prefix in word  */
        const ulong imwmask = (wmask ^ mwmask); /* inv mask for Magnitude in word  */
        const ulong iswmask = (wmask ^ swmask); /* inv mask for Sign in word  */

        ulong w; /* 36 bit Word stored in 64 bit ulong*/

        public uint A  /* Adress 15 bit */
        {
            get
            {
                return (uint)((w >> apos) & a0mask);
            }
            set
            {
                w = ((w & iawmask) | ((value & a0mask) << apos));
            }
        }
        public uint T  /* Tag 3 bit */
        {
            get
            {
                return (uint)((w >> tpos) & t0mask);
            }
            set
            {
                w = ((w & itwmask) | ((value & t0mask) << tpos));
            }
        }
        public uint D  /* Decrement 15 bit */
        {
            get
            {
                return (uint)((w >> dpos) & d0mask);
            }
            set
            {
                w = ((w & idwmask) | ((value & d0mask) << dpos));
            }
        }
        public uint P  /* Prefix 3 bit */
        {
            get
            {
                return (uint)((w >> ppos) & p0mask);
            }
            set
            {
                w = ((w & ipwmask) | ((value & p0mask) << ppos));
            }
        }
        public ulong M  /* Magnitude 35 bit */
        {
            get
            {
                return ((w >> mpos) & m0mask);
            }
            set
            {
                w = ((w & imwmask) | ((value & m0mask) << mpos));
            }
        }
        public uint S  /* Sign 1 bit*/
        {
            get
            {
                return (uint)((w >> spos) & s0mask);
            }
            set
            {
                w = ((w & iswmask) | ((value & s0mask) << spos));
            }
        }
        public ulong LW  /* Logical Word 36 bit unsigned */
        {
            get
            {
                return w & wmask;
            }
            set
            {
                w = value & wmask;
            }
        }
        public long AW  /* Arithmetical Word 35 bit with sign */
        {
            get
            {
                if ((w & swmask) != 0)
                    return (long)(w & m0mask);
                else
                    return -(long)(w & m0mask);
            }
            set
            {
                if (value >= 0)
                {
                    w = (ulong)value & m0mask;
                }
                else
                {
                    w = swmask | ((ulong)-value & m0mask);
                }
            }
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder(16);
            if (S != 0)
                s.Append('-');
            else
                s.Append(' ');

            string C = Convert.ToString((long)M, 8).PadLeft(12, '0');
            s.Append(C[0]);
            s.Append(' ');
            s.Append(C.Substring(1, 5));
            s.Append(' ');
            s.Append(C[6]);
            s.Append(' ');
            s.Append(C.Substring(7, 5));
            return s.ToString();
        }
    }
    public class Card  /* Card with 24 words in RCD format */
    {
        public W704[] C = new W704[24];
        public W704 W9L
        {
            get { return C[0]; }
            set { C[0] = value; }
        }
        public W704 W9R
        {
            get { return C[1]; }
            set { C[1] = value; }
        }
        public W704 W8L
        {
            get { return C[2]; }
            set { C[2] = value; }
        }
        public W704 W8R
        {
            get { return C[3]; }
            set { C[3] = value; }
        }
        public void Write()
        {
            for (int i = 0; i < 24; i++)
            {
                Console.WriteLine(C[i]);
            }
        }
    }
    public class ACLsum /* calculates ACL checksum */
    {
        ulong sum;

        public ACLsum(W704 i)
        {
            sum = i.LW;
        }
        public void ACL(W704 w)
        {
            const ulong carry = (1UL << 36);

            sum += w.LW;
            if ((sum & carry) != 0)
                sum -= carry - 1;
        }
        public W704 Get()
        {
            return new W704() { LW = sum };
        }
    }
    public static class BinaryCardConverter /* Handles UASAP binary cards */
    {
        public enum CardType { Full, Abs, Transfer, Rel, RelTransfer, Origin, };
        public enum RelType { absolute, relocatable_direct, relocatable_complemented };
        public static CardType GetCardType(Card C) /* Detects type of binary card according to Share Reference Manual Section 3.10 */
        {
            if (C.W9R.LW != 0 && ((C.W9L.P & 1) == 0)) /* with checksum ?*/
            {
                if (!VerifyChecksum(C)) /* Checksum differs */
                    return CardType.Full; /* not a valid card */
            }
            if (C.W9L.T != 0) /* Tag not zero? */
                return CardType.Full;  /* not a valid card */
            if (C.W9L.D >> 5 == 0) /* Check for abs or rel card */
            {
                if (C.W9L.P >> 1 == 0) /* check for abs card */
                {
                    /* Absolute Card */
                    if (C.W9L.D > 22) /* number of entries to large? */
                        return CardType.Full;  /* not a valid card */
                    if (C.W9L.D == 0) /* number of entries Zero? */
                        return CardType.Transfer; /* Abs Transfer card */
                    return CardType.Abs; /* Abs card */
                }
                else if (C.W9L.P >> 1 == 1)  /* check for rel card */
                {
                    /* Relocatable Card */
                    if (C.W9L.D > 20) /* number of entries to large? */
                        return CardType.Full;  /* not a valid card */
                    if (C.W9L.D == 0) /* number of entries Zero? */
                        return CardType.RelTransfer; /* Rel Transfer card */
                    if(GetRelData(C)==null) /* invalid Relocation data ?*/
                        return CardType.Full;  /* not a valid card */
                    return CardType.Rel; /* Rel card */
                }
                else
                    return CardType.Full;  /* not a valid card */
            }
            else if (C.W9L.D >> 5 == 1) /* Check for Origin Table */
                return CardType.Origin; /* Origin Table */
            else
                return CardType.Full;  /* not a valid card */
        }
        public static RelType[] GetRelData(Card C) /* Decodes Reloctation Data for REL cards. Returns null if invalid Reloctation Data */
        {
            string rel = Convert.ToString((long)C.W8L.LW, 2).PadLeft(36, '0') + Convert.ToString((long)C.W8R.LW, 2).PadLeft(36, '0'); /* convert W8L and W8R to 72 Bit Binary string */
            int pos = 0;
            RelType[] r = new RelType[C.W9L.D*2]; 
            for(int i=0;i<C.W9L.D*2;i++) /* for each decrement and address in Card */
            {
                if (pos >= rel.Length)  /* not enough data -> invalid */
                    return null; 
                if (rel[pos] == '0') /* Bit 0: Absolute */
                    r[i] = RelType.absolute;
                else
                {
                    pos++;
                    if (pos >= rel.Length)
                        return null;
                    if (rel[pos] == '0') /*Bits 10: Relocatable Direct */
                        r[i] = RelType.relocatable_direct;
                    else  /*Bits 11: Relocatable Complemented */
                        r[i] = RelType.relocatable_complemented;                    
                }
                pos++;
            }
            for(int i=pos;i<rel.Length;i++)
                if(rel[i]!='0')
                    return null; /* additional data at the end -> invalid */
            return r;
        }
        public static bool VerifyChecksum(Card C) /* Verifies Checksum. return value true if checksum OK */
        {
            ACLsum s = new ACLsum(C.W9L);
            for (int i = 2; i < 24; i++)
                s.ACL(C.C[i]);
            return s.Get().LW == C.W9R.LW;
        }
        public static void UpdateChecksum(Card C) /* Creates and updates Checksum */
        {
            ACLsum s = new ACLsum(C.W9L);
            for (int i = 2; i < 24; i++)
                s.ACL(C.C[i]);
            C.W9R = s.Get();
        }
    }
    public static class CBNConverter /* Converter betwen binary cards in RCD format and CBN card format */
    {
        public static byte[] ToCBN(Card crd) /* convert card from RCD Format to CBN format */
        {
           
            byte[] trecord = new byte[160];
            ulong[] mrecord = new ulong[24];

            for (int i = 0; i < 24; i++)
                mrecord[i] = crd.C[i].LW;
            for (int y = 0; y < 12; y++)  /* for all rows */
                for (int x = 0; x < 72; x++)  /* for all columns */
                {
                    /* index and bitpos in RCD format*/
                    int mpos = x / 36 + (11 - y) * 2;
                    int mbit = 35 - x % 36;
                    /* index and bitpos in CBN format */
                    int tpos = x * 2 + y / 6;
                    int tbit = 5 - y % 6;

                    if ((mrecord[mpos] & (1UL << mbit)) != 0) /*bit in mrecord set? */
                        trecord[tpos] |= (byte)(1 << tbit); /* set bit in trecord  */
                }
            return trecord;
        }
        public static void FromCBN(byte[] trecord, out Card crd) /* convert  card from CBN Format to RCD format */
        {
            if (trecord.Length != 160)
                throw new Exception("wrong record length");
            ulong[] mrecord = new ulong[24];

            for (int y = 0; y < 12; y++)  /* for all rows */
                for (int x = 0; x < 72; x++)  /* for all columns */
                {
                    /* index and bitpos in RCD format*/
                    int mpos = x / 36 + (11 - y) * 2;
                    int mbit = 35 - x % 36;
                    /* index and bitpos in CBN format */
                    int tpos = x * 2 + y / 6;
                    int tbit = 5 - y % 6;
                    if ((trecord[tpos] & (1 << tbit)) != 0) /* bit in trecord set ? */
                        mrecord[mpos] |= 1UL << mbit;  /* set bit in mrecord */
                }
            crd = new Card();
            for (int i = 0; i < 24; i++)
                crd.C[i].LW = mrecord[i];
        }
    }
    public static class HollerithConverter /* Converter between Hollerith codes in CBN card format and BCD data or strings*/
    {
        static readonly string[] hcode = new string[64] {
             "0",    "1",    "2",    "3",    "4",    "5",    "6",    "7",    "8",    "9",    "8-2",    "8-3",     "8-4",    "8-5",    "8-6",    "8-7",
            "12", "12-1", "12-2", "12-3", "12-4", "12-5", "12-6", "12-7", "12-8", "12-9", "12-8-2", "12-8-3",  "12-8-4", "12-8-5", "12-8-6", "12-8-7",
            "11", "11-1", "11-2", "11-3", "11-4", "11-5", "11-6", "11-7", "11-8", "11-9", "11-8-2", "11-8-3",  "11-8-4", "11-8-5", "11-8-6", "11-8-7",
              "",  "0-1",  "0-2",  "0-3",  "0-4",  "0-5",  "0-6",  "0-7",  "0-8",  "0-9",  "0-8-2",  "0-8-3",   "0-8-4",  "0-8-5",  "0-8-6",  "0-8-7" };
        static int[] bcd2hollerith;
        static Dictionary<int, byte> hollerith2bcd;
        static HollerithConverter()
        {
            bcd2hollerith = new int[64];
            hollerith2bcd = new Dictionary<int, byte>();
            for (byte i = 0; i < 64; i++)
            {
                int w = 0;

                string[] s = hcode[i].Split(new char[] { '-' });
                foreach (string c in s)
                    if (c != "")
                    {
                        int d = Convert.ToInt32(c);
                        if (d < 10)
                            d = 10 - d;
                        w |= 1 << (d - 1);
                    }
                bcd2hollerith[i] = w;
                hollerith2bcd.Add(w, i);
            }
        }
        public static int CBNToBCD(byte[] trecord, int start, int length, out byte[] bcd) /* Convert Hollerith Column Binary format to BCD */
        {                                                                            /* return: number of invalid Hollerith codes */
            bcd = new byte[length];
            int err = 0;
            for (int i = 0, x = start * 2; i < length; i++, x += 2)
            {
                int w = (trecord[x] << 6) | trecord[x + 1];
                if (hollerith2bcd.TryGetValue(w, out byte value))
                    bcd[i] = value;
                else
                {
                    bcd[i] = 0x30;  /* invalid */
                    err++;
                }
            }
            return err;
        }
        public static void BCDToCBN(byte[] bcd, int start, byte[] trecord) /* Convert BCD zu Hollerith Column Binary  */
        {
            int x = start * 2;
            foreach (byte b in bcd)
            {
                if (0 != (b & 0xC0))
                    throw new FormatException("invalid BCD char");
                int w = bcd2hollerith[b];
                trecord[x] = (byte)((w >> 6) & 0x3f);
                trecord[x + 1] = (byte)(w & 0x3f);
                x += 2;
            }
        }
        public static int CBNToString(byte[] trecord, int start, int length, out string s) /* Convert Hollerith Column Binary format to string */
        {                                                                                  /* return: number of invalid Hollerith codes */
            int err;
            err = CBNToBCD(trecord, start, length, out byte[] bcd);
            s = BcdConverter.BcdToString(bcd);
            return err;
        }
        public static void StringToCBN(string s, int start, byte[] trecord) /* Convert string zu Hollerith Column Binary  */
        {
            BCDToCBN(BcdConverter.StringToBcd(s), start, trecord);
        }
    }
    public static class BcdConverter/* Converter betwen BCD data and char/string data */
    {

        static int[] bcd2asc = new int[64]; /*  6 bit bcd to char, value 10dec=invalid, results to -1 */
        static Dictionary<char, int> asc2bcd;
        static readonly string[] ibm704bcd = new string[] /* Conversion table, first two chars: octal value, third: char value */
        {
            "000","011","022","033","044","055","066","077","108","119","12_","13=","14'","15:","16>","17{",
            "20+","21A","22B","23C","24D","25E","26F","27G","30H","31I","32?","33.","34)","35[","36<","37}",
            "40-","41J","42K","43L","44M","45N","46O","47P","50Q","51R","52!","53$","54*","55]","56;","57^",
            "60 ","61/","62S","63T","64U","65V","66W","67X","70Y","71Z","72|","73,","74(","75~","76\\","77\""
        };
        static BcdConverter() /* static Construktor */
        {
            int i;
            asc2bcd = new Dictionary<char, int>();
            for (i = 0; i < ibm704bcd.Length; i++)
            {
                int b = Convert.ToInt32(ibm704bcd[i].Substring(0, 2), 8);
                if (bcd2asc[b] != 0 || b != i)
                    Console.Error.WriteLine("BcdConverter: error in bcd table");
                char c = ibm704bcd[i][2];
                bcd2asc[b] = c;
                char cl = Char.ToLower(c);
                asc2bcd.Add(c, b);
                if (cl != c)
                    asc2bcd.Add(cl, b);
            }
        }
        public static char BcdToChar(byte bcd) /* converts BCD to char */
        {
            if (0 != (bcd & 0xC0u)) /* Bits 7 oder 6 have to be zero */
                throw new Exception("BcdConverter:invalid BCD char");
            if (bcd2asc[bcd] == -1)
            {
                Console.Error.Write("{0} ", Convert.ToString(bcd, 8).PadLeft(2, '0'));
                return '?';
            }
            return (char)bcd2asc[bcd];
        }
        public static byte CharToBcd(char chr)/* converts char to BCD */
        {
            if (asc2bcd.TryGetValue(chr, out int v))
            {
                return (byte)v;
            }
            else
            {
                Console.Error.WriteLine("invalid Char {0}", chr);
                return (byte)asc2bcd['?'];
            }
        }
        public static byte[] StringToBcd(string s)/* converts string to BCD array */
        {
            byte[] B = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                B[i] = CharToBcd(s[i]);
            return B;
        }
        public static string BcdToString(byte[] bcd) /* converts BCD array to string */
        {
            StringBuilder s = new StringBuilder(bcd.Length);
            foreach (byte b in bcd)
                s.Append(BcdToChar(b));
            return s.ToString();
        }
    }
    public static class TapeConverter /* Converter between raw tape records and bcd/binary records */
    {
        /* Data on Tape ist sequence of 6 bit values: bits 0-5: character, Bit 6: parity */
        /* The parity (even/odd) of all bytes from a records is the same*/
        /* Odd parity: binary record */
        /* binary records can contain any 6 bit data */
        /* Even Parity: BCD eecord*/
        /* BCD Record cannot contain a Zero */
        /* for BCD Record a conversion from tapeformat to memoryformat is performed */
        /* Tape 10dez -> Memory 0 (BCD '0') and if bit 4 is set, bit 5 is inverted: fomr tape to mem  BDC   'A'-'I' is swapped with 'S'-'Z' */

        static int[] tape2mem = null;  /* bcd conversion tape -> mem */
        static int[] mem2tape = null;  /* bcd conversion mem  -> tape */
        static bool[] oddparity = null; /* 7-Bit parity table: true = odd parity */
        static TapeConverter() /* statischer constructor, fills oddparity and tape2mem,mem2tape */
        {
            oddparity = new bool[128];
            for (int i = 0; i < 128; i++) /* for all 7-bit values */
            {
                bool o = false; /* start with even parity  */
                for (int b = 0; b < 7; b++) /* for all bits*/
                {
                    if (0 != (i & (1 << b)))  /* bit set? */
                        o = !o;   /* toggle parity */
                }
                oddparity[i] = o;  /* store parity */
            }
            tape2mem = new int[64];
            mem2tape = new int[64];
            for (int m = 0; m < 64; m++)  /* for all 6-bit values */
            {
                int t;
                if (m == 10)
                    t = -1;  /* bcd-value 10 in mem not allowed */
                else if (m == 0) /* convert 0 */
                    t = 10;
                else if (0 != (m & 16)) /* convert A-I <-> S-Z */
                    t = m ^ 32;
                else
                    t = m;
                mem2tape[m] = t;  /* store value */
            }
            for (int t = 0; t < 64; t++)  /* for all 6-bit values */
            {
                int m;
                if (t == 0)
                    m = -1;  /* bcd-value 0 on tape not allowed */
                else if (t == 10) /* convert 10 */
                    m = 0;
                else if (0 != (t & 16)) /* convert A-I <-> S-Z */
                    m = t ^ 32;
                else
                    m = t;
                tape2mem[t] = m;  /* wert speichern */
            }
        }
        public static void ToTape(bool binary, byte[] mrecord, out byte[] trecord) /* converts record from Binary/BCD into raw tape format */
        {
            trecord = new byte[mrecord.Length];
            for (int i = 0; i < mrecord.Length; i++)
            {
                byte b = mrecord[i];
                if ((b & 128) != 0)
                    throw new InvalidDataException("TapeConverter:invalid bit 7 set in mem");
                if ((b & 64) != 0)
                    throw new InvalidDataException("TapeConverter:invalid bit 6 set in mem");
                if (!binary)
                {
                    if (mem2tape[b] == -1)
                        throw new Exception("TapeConverter:invalid BCD character in mem");
                    b = (byte)mem2tape[b];
                }
                if (binary != oddparity[b])
                    b |= 0x40;
                trecord[i] = b;
            }
        }
        public static void FromTape(byte[] trecord, out bool binary, out byte[] mrecord) /* converts record from raw tape format into Binary/BCD record */
        {
            binary = oddparity[trecord[0]]; /* ungerade parität -> binärfile */
            mrecord = new byte[trecord.Length];
            for (int j = 0; j < trecord.Length; j++)
            {
                if ((trecord[j] & 128) != 0)
                    throw new InvalidDataException("TapeConverter:bit 7 is set on tape");
                if (binary != oddparity[trecord[j]]) /* weitere zeichen auf parität prüfen */
                    throw new InvalidDataException("TapeConverter:parity error on tape");
                if (binary)
                    mrecord[j] = (byte)(trecord[j] & 63); /* binärdaten direkt übernehmen */
                else
                {
                    int c = tape2mem[trecord[j] & 63];   /* bcddaten konverteren */
                    if (c == -1) /*  (0 auf tape) -> ungültig */
                        throw new InvalidDataException("TapeConverter:invalid BCD char 0 on tape");
                    mrecord[j] = (byte)c; /* speichern*/
                }
            }
        }
    }    
    public class TapeReader /* Reads Tapes in P7B and tap format */ : IDisposable 
    {
        /* P7B format */
        /* A tape is a sequence of records */
        /* A record is a sequence of characters. Bits 0-6: character including parity, Bit 7: Recordmarker */
        /* The first byte of a records has bit 7 set, all following have bit 7 not set */
        /* even parity= bcd record, odd parity=binary record */
        /* bcd record with length 1 and value 15dez is EOF marker */

        /* Tap format: */
        /* A tape is a sequence of records */
        /* a record is  4 Byte Length, data, 4 Byte Byte Length */
        /* data is a sequence of characters. Bits 0-6: character including parity bit 7: 0 */
        /* or a record is 4 byte==0.-> EOF marker or 4 byte=0xFFFFFFFF EOM (End of Media) */

        FileStream f = null;  /* inputfile */
        bool stored; /* true:fist byte of record already read */
        int last;  /* value of first byte or -1 for eom*/
        bool p7bflag;
        public TapeReader(string filename, bool p7b) /* Constructor,  opens file */
        {
            f = File.OpenRead(filename);
            p7bflag = p7b;
            stored = false; /* no char read */
        }
        public int ReadRecord(out bool binary, out byte[] mrecord) /* reads a record as array of 6-bit value from tape-file, retun -1:end of input file 0: EOF marker read, 1: record read; binary=true: binary record false: bcd record */
        {
            /* reset output values */
            mrecord = null;
            binary = false;
            if (p7bflag) /* read p7b record */
            {
                int b; /* current character */

                if (stored) /* fist byte of record already read? */
                {
                    b = last; /* use it */
                    stored = false;
                }
                else
                    b = f.ReadByte(); /* read byte */

                if (b < 0) /* end of media ? */
                    return -1; /* EOM */
                if ((b & 128) == 0)  /* The first byte of a records must have bit 7 set */
                    throw new InvalidDataException("TapeReader:Bit 7 not set at record start");
                List<byte> trecord = new List<byte>(160) { (byte)(b & 127) }; /* remove record start marker, store character */
                do
                {
                    b = f.ReadByte();
                    if (b < 0 || (b & 128) != 0) /* next record or EOF */
                    {
                        stored = true; /* set flag */
                        last = b; /* store value for next call*/
                        break;
                    }
                    trecord.Add((byte)b);
                }
                while (true);
                TapeConverter.FromTape(trecord.ToArray(), out binary, out mrecord);
                if (!binary && mrecord.Length == 1 && mrecord[0] == 15)
                    return 0; /* EOF */
                return 1; /* no EOF */
            }
            else /* read tp record */
            {
                const int maxbuffer = 32000;
                byte[] len = new byte[4];

                if (f.Read(len, 0, 4) != 4) /* read length, check if readable */
                    return -1;
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(len);
                uint l = BitConverter.ToUInt32(len, 0);
                if (l == 0)
                    return 0; /* length of 0 means EOF */
                else if (l == 0xFFFFFFFF)
                    return -1;/* length of -1 means EOM */
                else
                {
                    if (l > maxbuffer)
                        return -1; 
                    byte[] trecord = new byte[l];
                    if (l != f.Read(trecord, 0, (int)l))
                        throw new Exception("read error");
                    byte[] elen = new byte[4];
                    if (4 != f.Read(elen, 0, 4))
                        throw new Exception("read error");
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(elen);
                    if (l != BitConverter.ToUInt32(elen, 0))
                        throw new Exception("wrong elen");
                    TapeConverter.FromTape(trecord, out binary, out mrecord);
                    return 1; /* record was read */
                }
            }
        }
        public void Dispose() /* IDisposable-Handling */
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing) /* IDisposable-Handling */
        {
            if (disposing)
            {
                // free managed resources  
                if (f != null)
                {
                    f.Dispose();
                    f = null;
                }
            }
        }
    }
    public class TapeWriter /* Writes Tapes in P7B and tap format */ : IDisposable
    {
        FileStream f = null;  /* output file */
        bool p7bflag;
        public TapeWriter(string filename, bool p7b)
        {
            f = File.Create(filename);
            p7bflag = p7b;
        }
        public void WriteRecord(bool binary, byte[] mrecord)
        {
            TapeConverter.ToTape(binary, mrecord, out byte[] trecord);
            if (p7bflag)
            {
                trecord[0] |= 0x80;
                f.Write(trecord, 0, trecord.Length);
            }
            else
            {
                byte[] len = BitConverter.GetBytes(trecord.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(len);
                f.Write(len, 0, 4);
                f.Write(trecord, 0, (int)trecord.Length);
                f.Write(len, 0, 4);
            }
        }
        public void WriteEOF()
        {
            if (p7bflag)
                WriteRecord(false, new byte[] { 15 });
            else
            {
                int n = 0;
                byte[] len = BitConverter.GetBytes(n);
                f.Write(len, 0, 4);
            }
        }
        public void Dispose() /* IDisposable-Handling */
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing) /* IDisposable-Handling */
        {
            if (disposing)
            {
                // free managed resources  
                if (f != null)
                {
                    f.Dispose();
                    f = null;
                }
            }
        }
    }
}
