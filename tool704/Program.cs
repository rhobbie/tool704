using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace tool704
{
    struct W704 /* 36 Bit Word with access to Adress, Tag, Decrement, Prefix,  Magnitude, Sign, Logical Word, Arithmetical Word */
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
    class Card  /* Card with 24 words in RCD format */
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
    class ACLsum /* calculates ACL checksum */
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
    static class BinaryCardConverter
    {
        public enum CardType { Full, Abs, Transfer, Rel, RelTransfer, Origin, };
        public static CardType GetCardType(Card C) /* Detects Type of Binary Card according to Share Reference Manual Section 3.10 */
        {
            if (C.W9R.LW != 0 && ((C.W9L.P & 1) == 0)) /* with checksum ?*/
            {
                /* calculate checksum*/
                ACLsum s = new ACLsum(C.W9L);
                for (int i = 2; i < 24; i++)
                    s.ACL(C.C[i]);
                if (s.Get().LW != C.W9R.LW) /* CHecksum differs */
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
                    return CardType.Rel; /* Rel card */
                }
                else
                    return CardType.Full;  /* not a valid card */
            }
            else if (C.W9L.D >> 5 == 1) /* Check for Origin Table */
            {
                return CardType.Origin; /* Origin Table */
            }
            else
                return CardType.Full;  /* not a valid card */
        }
    }

    static class CBNConverter /* Converter betwen binary cards in RCD format and CBN card format */
    {
        public static byte[] ToCBN(Card crd) /* Transponiere Cardimage vom internen Format zum Tape Format */
        {

            byte[] trecord = new byte[160];
            ulong[] mrecord = new ulong[24];

            for (int i = 0; i < 24; i++)
                mrecord[i] = crd.C[i].LW;
            for (int y = 0; y < 12; y++)  /* für alle zeilen von oben nach unten*/
                for (int x = 0; x < 72; x++)  /* für alle spalten von links nach rechts*/
                {
                    /* index and bitpos in Crd */
                    int mpos = x / 36 + (11 - y) * 2;
                    int mbit = 35 - x % 36;
                    /* index and bitpos in trecord */
                    int tpos = x * 2 + y / 6;
                    int tbit = 5 - y % 6;

                    if ((mrecord[mpos] & (1UL << mbit)) != 0) /*bit in mrecord set? */
                        trecord[tpos] |= (byte)(1 << tbit); /* set bit in trecord  */
                }
            return trecord;
        }
        public static void FromCBN(byte[] trecord, out Card crd) /* Transponiere Cardimage vom Tape Format auf internes Format */
        {
            if (trecord.Length != 160)
                throw new Exception("wrong record length");
            ulong[] mrecord = new ulong[24];

            for (int y = 0; y < 12; y++)  /* für alle zeilen von oben nach unten*/
                for (int x = 0; x < 72; x++)  /* für alle spalten von links nach rechts*/
                {
                    /* index und bitpos in mrecord */
                    int mpos = x / 36 + (11 - y) * 2;
                    int mbit = 35 - x % 36;
                    /* index und bitpos in trecord */
                    int tpos = x * 2 + y / 6;
                    int tbit = 5 - y % 6;
                    if ((trecord[tpos] & (1 << tbit)) != 0) /* bit in trecord gesetzt */
                        mrecord[mpos] |= 1UL << mbit;  /* in mrecord setzen */

                }
            crd = new Card();
            for (int i = 0; i < 24; i++)
                crd.C[i].LW = mrecord[i];
        }
    }
    static class HollerithConverter /* Converter between Hollerith codes in CBN card format and BCD data or strings*/
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
        public static int CBNToBCD(byte[] trecord, int start, int length, out byte[] bcd) /* Wandle Hollerith Column Binary Format zu BCD */
        {                                                                            /* rückgabe anzahl ungütiger Hollerith codes */
            bcd = new byte[length];
            int err = 0;
            for (int i = 0, x = start * 2; i < length; i++, x += 2)
            {
                int w = (trecord[x] << 6) | trecord[x + 1];
                if (hollerith2bcd.TryGetValue(w, out byte value))
                    bcd[i] = value;
                else
                {
                    bcd[i] = 0x30;  /* ungültig */
                    err++;
                }
            }
            return err;
        }
        public static void BCDToCBN(byte[] bcd, int start, byte[] trecord) /* Wandle BCD zu Hollerith Column Binary  */
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
        public static int CBNToString(byte[] trecord, int start, int length, out string s) /* Wandle Hollerith Column Binary Format zu string */
        {
            int err;/* rückgabe anzahl ungütiger Hollerith codes */
            err = CBNToBCD(trecord, start, length, out byte[] bcd);
            s = BcdConverter.BcdToString(bcd);
            return err;
        }
        public static void StringToCBN(string s, int start, byte[] trecord) /* Wandle string zu Hollerith Column Binary  */
        {
            BCDToCBN(BcdConverter.StringToBcd(s), start, trecord);
        }
    }
    static class BcdConverter/* Converter betwen BCD data and char/string data */
    {
        /* Umwandlung bcd mem format nach char, quelle: fortran-ii listing, listtape.c */
        static int[] bcd2asc = new int[64]; /*  6 bit bcd to char, wert 10dez ungültig, ergibt -1 */
        static Dictionary<char, int> asc2bcd;
        static readonly string[] ibm704bcd = new string[] /* Umwandlungstabelle, Erste zwei Zeichen Oktalwert, drittes Zeichen Charwert */
        {
            "000",
            "011",
            "022",
            "033",
            "044",
            "055",
            "066",
            "077",
            "108",
            "119",
            "12_",  /* Soll: b durchgestrichen ␢  slashed b substitute blank SM670000 Substitute Blank [That is the small b shape with slash.]  U+2422 BLANK SYMBOL */
            "13=",  /* Report # */                
            "14'",  /* Report @ */
            "15:",
            "16>",
            "17{",  /* auch &"  Soll Wurzel: √ square root tape mark*/
            "20+",  /* Report & */
            "21A",
            "22B",
            "23C",
            "24D",
            "25E",
            "26F",
            "27G",
            "30H",
            "31I",
            "32?",
            "33.",
            "34)", /* Report Kleines quadrat ⌑ SM490000	Lozenge U+2311 SQUARE LOZENGE */
            "35[",
            "36<",
            "37}", /*auch %| soll: Senkrechter strich dreimal durchgestrichen  SS970000 Group Mark [Vertical bar across three short horizontal bars]  U+241D SYMBOL FOR GROUP SEPARATOR ␝ */
            "40-",
            "41J",
            "42K",
            "43L",
            "44M",
            "45N",
            "46O",
            "47P",
            "50Q",
            "51R",
            "52!",
            "53$",
            "54*",
            "55]",
            "56;",
            "57^", /* auch _ soll: dreieck ∆ Δ Greek capital delta mode change*/
            "60 ",
            "61/",
            "62S",
            "63T",
            "64U",
            "65V",
            "66W",
            "67X",
            "70Y",
            "71Z",
            "72|", /* auch #  soll: senkrechter strich zweimal durchgestrichen ‡ ‡  not equals record mark SS950000 Record Mark [Vertical bar across two short horizontal bars]  U+241E SYMBOL FOR RECORD SEPARATOR  U+29E7 THERMODYNAMIC [Vertical bar across two short horizontal bars] */
            "73,",
            "74(", /* Report: % */
            "75~", /* auch ^` soll: inverses ^  ˅ γ U+02C7 ˇ caron  inverted caret or equals  word separator  U+22CE CURLY LOGICAL OR */
            "76\\",
            "77\"" /* auch { soll: wagerechter strich 3 mal durchgestrichen ⧻ triple vertical bar slashed segment mark SS960000 Segment Mark [Horizontal bar across 3 short verticals]   U+241F SYMBOL FOR UNIT SEPARATOR U+29FB TRIPLE PLUS [Horizontal bar across three short verticals] */
        };
        static BcdConverter() /* statischer Konstruktor */
        {
            int i;
            asc2bcd = new Dictionary<char, int>();
            for (i = 0; i < ibm704bcd.Length; i++)
            {
                int b = Convert.ToInt32(ibm704bcd[i].Substring(0, 2), 8);
                if (bcd2asc[b] != 0 || b != i)
                    throw new Exception("BcdConverter: error in bcd table");
                char c = ibm704bcd[i][2];
                bcd2asc[b] = c;
                char cl = Char.ToLower(c);
                asc2bcd.Add(c, b);
                if (cl != c)
                    asc2bcd.Add(cl, b);
            }
        }
        public static char BcdToChar(byte bcd) /* wandelt BCD nach char mit Prüfung */
        {
            if (0 != (bcd & 0xC0u)) /* bits 7 oder 6 müssen Null sein */
                throw new Exception("BcdConverter:invalid BCD char");
            if (bcd2asc[bcd] == -1)
            {
                Console.Write("{0} ", Convert.ToString(bcd, 8).PadLeft(2, '0'));
                return '?';
            }
            return (char)bcd2asc[bcd];
        }
        public static byte CharToBcd(char chr)/* wandelt char nach BCD mit Prüfung */
        {
            if (asc2bcd.TryGetValue(chr, out int v))
            {
                return (byte)v;
            }
            else
            {
                Console.WriteLine("invalid Char {0}", chr);
                return (byte)asc2bcd['?'];
            }
        }
        public static byte[] StringToBcd(string s)/* wandelt string nach BCD Array */
        {
            byte[] B = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                B[i] = CharToBcd(s[i]);
            return B;
        }
        public static string BcdToString(byte[] bcd) /* wandelt BCD array string char mit Prüfung */
        {
            StringBuilder s = new StringBuilder(bcd.Length);
            foreach (byte b in bcd)
                s.Append(BcdToChar(b));
            return s.ToString();
        }
    }
    static class TapeConverter /* Converter between raw tape records and bcd/binary records */
    {
        /* Daten auf Tape ist folge von 6 bit werten: Bits 0-5: Zeichen, Bit 6: Parity */
        /* Die Parity (grade/ungerade) der Bits 0-6 aller Bytes eines Records ist geich*/
        /* Bei ungerader Parity: Binary Record */
        /* Binary Records enthalten beliebige 6 Bit Daten */
        /* Bei geradeder Parity: BCD Record*/
        /* Ein BCD Record darf keine Null enthalten */
        /* Bei BCD Records erfolgt eine Umwandlung vom Tapeformat zum Memoryformat: */
        /* Tape 10dez -> Memory 0 (BCD '0') und  Wenn Bit 4 gesetzt wird Bit 5 invertiert, d.h. von Tape zu Mem werden BDC Zeichen  'A'-'I' mit 'S'-'Z' getauscht,  */

        static int[] tape2mem = null;  /* bcd umwandlung tape <-> mem */
        static int[] mem2tape = null;  /* bcd umwandlung tape <-> mem */
        static bool[] oddparity = null; /* 7-Bit parity table: true = ungerade parität */
        static TapeConverter() /* statischer Konstruktor, füllt oddparity und tape2mem,mem2tape */
        {
            oddparity = new bool[128];
            for (int i = 0; i < 128; i++) /* alle möglichen 7-bit werte */
            {
                bool o = false; /* mit gerader parität anfangen */
                for (int b = 0; b < 7; b++) /* alle bits durchgehen*/
                {
                    if (0 != (i & (1 << b)))  /* bit gesetzt? */
                        o = !o;   /* parity togglen */
                }
                oddparity[i] = o;  /* parity speichern */
            }
            tape2mem = new int[64];
            mem2tape = new int[64];
            for (int m = 0; m < 64; m++)  /* alle möglichen 6-bit werte */
            {
                int t;
                if (m == 10)
                    t = -1;  /* bcd-wert 10 in mem nicht erlaubt */
                else if (m == 0) /* konvertiere 0 */
                    t = 10;
                else if (0 != (m & 16)) /* konvertiere A-I <-> S-Z */
                    t = m ^ 32;
                else
                    t = m;
                mem2tape[m] = t;  /* wert speichern */
            }
            for (int t = 0; t < 64; t++)  /* alle möglichen 6-bit werte */
            {
                int m;
                if (t == 0)
                    m = -1;  /* bcd-wert 0 auf tape nicht erlaubt */
                else if (t == 10) /* konvertiere 0 */
                    m = 0;
                else if (0 != (t & 16)) /* konvertiere A-I <-> S-Z */
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
    class TapeReader : IDisposable /* Reads Tapes in P7B and tap format */
    {
        /* P7B Format */
        /* Ein Tape ist eine Folge von Records */
        /* Record ist folge von Zeichen. Bits 0-6: Zeichen mit Parity, Bit 7: Recordmarker */
        /* Das erste Byte des Records hat Bit 7 gesetzt, alle folgenden Bytes des Records haben Bit 7 nicht gesetzt*/
        /* bcd record mit länge 1 und wert 15 ist EOF Marker */

        /* Tap Format: */
        /* Ein Tape ist eine Folge von Records */
        /* Record besteht aus 4 Byte Länge>0 <Maxbuffer, daten, 4 Byte Länge */
        /* daten ist folge von Zeichen Bits 0-6: Zeichen mit Parity, Bit 7: 0 */
        /* oder 4Byte==0.-> EOF Marker oder 4Byte=0xFFFFFFFF EOM (End of Media) */

        Stream f = null;  /* eingabedatei */
        bool stored; /* true: es wurde schon das erste byte des nächsten records oder eof gelesen.*/
        int last;  /* erstes Byte des nächsten records oder -1=eom */
        bool p7bflag;
        public TapeReader(string filename, bool p7b) /* Konstruktor, öffnet file */
        {
            f = File.OpenRead(filename);
            p7bflag = p7b;
            stored = false; /* noch kein zeichen gelesen */
        }
        public int ReadRecord(out bool binary, out byte[] mrecord) /* liest ein record als array von 6-bit werten aus tape-file, rückgabe -1:ende der eingabedatei, 0: EOF, 1: record gelesen; binary true: binärformat false: bcdformat */
        {
            /* ausgabewerte rücksetzen */
            mrecord = null;
            binary = false;
            if (p7bflag) /* read p7b record */
            {
                int b; /* aktuelles zeichen */

                if (stored) /* schon ein zeichen gemerkt */
                {
                    b = last; /* übernehmen */
                    stored = false;
                }
                else
                    b = f.ReadByte(); /* zeichen lesen */

                if (b < 0) /* end of media ? */
                    return -1; /* EOM */
                if ((b & 128) == 0)  /* das erste zeichen eines records muss msb gesetzt haben */
                    throw new InvalidDataException("TapeReader:Bit 8 not set at record start");
                List<byte> trecord = new List<byte>(160) { (byte)(b & 127) }; /* record start marker entfernen, zeichen speichern */
                do
                {
                    b = f.ReadByte();
                    if (b < 0 || (b & 128) != 0) /* nächster record oder EOF */
                    {
                        stored = true; /* merker setzen */
                        last = b; /* gelesenen wert speichern für nächsten aufruf*/
                        break;
                    }
                    trecord.Add((byte)b);
                }
                while (true);
                TapeConverter.FromTape(trecord.ToArray(), out binary, out mrecord);
                if (!binary && mrecord.Length == 1 && mrecord[0] == 15)
                {
                    return 0; /* EOF */
                }
                return 1; /* kein end of file */
            }
            else /* read tap record */
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
                        throw new Exception("record to big");
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
    class TapeWriter : IDisposable
    {
        Stream f = null;  /* ausgabedatei */
        bool p7bflag;
        public TapeWriter(string filename, bool p7b)
        {
            f = File.OpenWrite(filename);
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
    class DeckReader
    {
        TapeReader r; /* Speicher für Tapereader */
        Card crd; /* Speicher für Karte */
        bool transferread; /* transferkarte schon gelesen */
        BinaryCardConverter.CardType t; /* typ */
        int n; /* Anzahl Worte schon gelesen */
        int cur_adr; /* Aktuelle Adresse auf Karte */
        List<Card> deck; /* speichert gelesene karten */
        public char Cardtype()
        {
            char dt;
            dt = ' ';
            switch(t)
            {
                case BinaryCardConverter.CardType.Full:
                    dt = 'F';
                    break;
                case BinaryCardConverter.CardType.Abs:
                    dt = 'A';
                    break;
                case BinaryCardConverter.CardType.Rel:
                    dt = 'R';
                    break;
                case BinaryCardConverter.CardType.Transfer:
                case BinaryCardConverter.CardType.RelTransfer:
                    dt = 'T';
                    break;
            }
            return dt;
        }
        public DeckReader(TapeReader re, List<Card> d)
        {
            /* liest karten im CBN format vom tape, und speichert nach d, dt: kartentyp, F=FUL,A=ABS,R=REL,T=TRANSFER */

            transferread = false;
            r = re;
            deck = d;
            if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* karte lesen */
                throw new Exception("last card read");
            CBNConverter.FromCBN(mrecord, out crd); /* umwandeln */

            t = BinaryCardConverter.GetCardType(crd); /* Typ der karte bestimmen */
            n = 0; /* noch nichts gelesen */
            switch (t) /* typ auswerten */
            {
                case BinaryCardConverter.CardType.Full:            
                    cur_adr = 0; /* Full decks beginnen bei 0*/
                    break;
                case BinaryCardConverter.CardType.Abs:
                    cur_adr = (int)crd.W9L.A; /* startadresse lesen */
                    break;
                case BinaryCardConverter.CardType.Rel:
                    cur_adr = (int)crd.W9L.A; /* startadresse lesen */
                    break;
                case BinaryCardConverter.CardType.Transfer:
                case BinaryCardConverter.CardType.RelTransfer:
                    break;
                default:
                    throw new InvalidDataException("invalid card");
            }
            deck.Add(crd); /* karte zum deck hinzufügen */
        }
        public bool Read(out int adr, out long value) /* wort und adresse aus karte lesen, rückgabe true normale karte, rückgabe false transferkarte, dann value=0, adr=transferadresse */
        {
            adr = 0;
            value = 0;
            if (transferread)
                throw new InvalidOperationException("transfercard alread read");
            switch (t)
            {
                case BinaryCardConverter.CardType.Full:
                    if (n == 24) /* alles schon gelesen? */
                    {
                        /* nächste karte lesen */
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0)
                            throw new Exception("last card read");

                        /* umwandlen */
                        CBNConverter.FromCBN(mrecord, out crd);
                        deck.Add(crd); /* karte zum deck hinzufügen */
                        BinaryCardConverter.CardType tt = BinaryCardConverter.GetCardType(crd);
                        if (tt != BinaryCardConverter.CardType.Full) /* muss auch full sein */
                            Console.WriteLine("invalid card type {0})",tt);
                        n = 0; /* noch nichts gelesen */

                        
                    }
                    adr = cur_adr; /* adresse übernehmen */
                    value = (long)crd.C[n].LW; /* wert übernehmen */
                    /* weiterzählen */
                    n++;
                    cur_adr++;
                    return true; /* karte gelesen */
                case BinaryCardConverter.CardType.Abs:
                    if (n == crd.W9L.D) /* alle karten schon gelesen */
                    {
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* Nächste Karte lesen */
                            throw new Exception("last card read");
                        CBNConverter.FromCBN(mrecord, out crd); /* umwandeln */
                        deck.Add(crd); /* karte zum deck hinzufügen */
                        BinaryCardConverter.CardType nt = BinaryCardConverter.GetCardType(crd); /* kartentyp bestimmen */
                        if (nt == BinaryCardConverter.CardType.Transfer) /* transferkarte ? */
                        {
                            adr = (int)crd.W9L.A;/* adresse übernehmen */
                            value = 0; /* wert 0 be transferkarte */
                            transferread = true; /* merker setzen */
                            return false; /* transferkarte gelesen */
                        }
                        if (nt != t) /* typ anders ?*/
                            throw new Exception("invalid card type");
                        n = 0; /* zähler rücksetzen */
                        cur_adr = (int)crd.W9L.A; /* akt adresse übernehmen */

                       
                    }
                    adr = cur_adr; /* adresse übernehmen */
                    value = (long)crd.C[n + 2].LW; /* wert aus karte übernehmen */
                    n++;/* weiterzählen */
                    cur_adr++;
                    return true;  /* karte gelesen */
                case BinaryCardConverter.CardType.Rel:
                    if (n == crd.W9L.D) /* alle karten schon gelesen */
                    {
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* Nächste Karte lesen */
                            throw new Exception("last card read");
                        CBNConverter.FromCBN(mrecord, out crd); /* umwandeln */
                        deck.Add(crd); /* karte zum deck hinzufügen */
                        BinaryCardConverter.CardType nt = BinaryCardConverter.GetCardType(crd);/* kartentyp bestimmen */
                        if (nt == BinaryCardConverter.CardType.RelTransfer) /* transferkarte ? */
                        {
                            adr = (int)crd.W9L.A;/* adresse übernehmen */
                            value = 0;/* wert 0 be transferkarte */
                            transferread = true; /* merker setzen */
                            return false; /* transferkarte gelesen */
                        }
                        if (nt != t)  /* typ anders ?*/
                            throw new Exception("invalid card type");
                        n = 0; /* zähler rücksetzen */
                        cur_adr = (int)crd.W9L.A; /* akt adresse übernehmen */
                    }
                    adr = cur_adr; /* adresse übernehmen */
                    value = (long)crd.C[n + 4].LW; /* wert aus karte übernehmen */
                    n++; /* weiterzählen */
                    cur_adr++;
                    return true; /* karte gelesen */
                case BinaryCardConverter.CardType.Transfer:
                case BinaryCardConverter.CardType.RelTransfer:
                    {
                        adr = (int)crd.W9L.A;/* adresse übernehmen */
                        value = 0;/* wert 0 be transferkarte */
                        transferread = true; /* merker setzen */
                        return false; /* transferkarte gelesen */
                    }
                default:
                    throw new InvalidDataException("invalid card");
            }
        }
        public bool CardEmpty()
        {
            switch (t)
            {
                case BinaryCardConverter.CardType.Full:
                    if (n < 24) /* still data on card */
                    {
                        for (int i = n; i < 24; i++)
                            if (crd.C[i].LW != 0) /* values not zero */
                                return false;
                    }
                    break;
                case BinaryCardConverter.CardType.Abs:
                    if (n < crd.W9L.D) /* still data on card */
                        return false;
                    break;
            }
            return true;
        }
    }
    class Lister
    {
        int thispage = 0;
        int thisline = 0;
        int startpage;
        int startline;
        int FirstAdr=1000000;
        int LastAdr =-1000000;
        int curadr;
        bool bssflag = false;
        
        TapeReader tr;
        DeckReader dr;
        List<Card> deck;
        bool hadnoorg;
        string out_p;
        public void Setpage(int page)
        {
            thispage = page;
            thisline = 1;
        }
        public void Nextline()
        {
            thisline++;
        }
        public void ORG(int adr)
        {
            if (dr!=null)
                END(null);
            hadnoorg = false;            
            curadr = adr;
            bssflag = false;
            startline = thisline;
            startpage = thispage;
        }
        public void BES(int adr)
        {
            curadr = adr;
            bssflag = false;
        }
        public void BSS(int adr)
        {
            if (bssflag)
                curadr = adr;
            if (adr != curadr)
                throw new InvalidDataException(string.Format("{0} {1} wrongadr", thispage, thisline));
            bssflag = true;
        }
        public void END(int? TransferAdr)
        {
            char tch = ' ';

            if (TransferAdr != null && dr == null)
            {
                deck = new List<Card>();
                dr = new DeckReader(tr, deck);
                if(hadnoorg)
                {
                    startline = thisline;
                    startpage = thispage;
                }
            }
            if(dr!=null)
            {
                if (dr.Cardtype() == 'R')
                {
                    TransferAdr = 0;
                }
                if ((TransferAdr != null&&(!hadnoorg||TransferAdr!=0)))
                {
                    if (dr.Read(out int adr, out long value))
                    {
                        Console.WriteLine("Abweichung {0} {1}", thispage, thisline);
                        Console.WriteLine("Card {0} {1}", Convert.ToString(adr, 8).PadLeft(5, '0'), Convert.ToString(value, 8).PadLeft(12, '0'));
                        Console.WriteLine("List Transfer {0}", Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0'));
                        Environment.Exit(-1);
                    }
                    else if (adr != TransferAdr)
                    {
                        Console.WriteLine("Abweichung {0} {1}", thispage, thisline);
                        Console.WriteLine("Card Transfer {0}", Convert.ToString(adr, 8).PadLeft(5, '0'));
                        Console.WriteLine("List Transfer {0}", Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0'));
                        Environment.Exit(-1);
                    }
                    if (dr.Cardtype() != 'T')
                        tch = 'T';
                }
                if (!dr.CardEmpty())
                {
                    Console.WriteLine("Abweichung {0} {1}", thispage, thisline);
                    Console.WriteLine("more data on card");
                    Environment.Exit(-1);
                }
                string x="";
                if(startpage!=0)
                {
                    x += string.Format("Page{0}", startpage);
                }                
                x += string.Format("Line{0}", startline);
                x += "_";
                x += dr.Cardtype();
                if (tch != ' ') 
                    x += tch;
                Console.Write("{0}:{1} {2} {3} {4}\n",x ,deck.Count,Convert.ToString(FirstAdr,8).PadLeft(5,'0'), Convert.ToString(LastAdr, 8).PadLeft(5, '0'),TransferAdr!=null? Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0'):"");
                using (TapeWriter tw = new TapeWriter(out_p+ x+".cbn", true))
                    foreach (Card crd in deck)
                        tw.WriteRecord(true, CBNConverter.ToCBN(crd));
                deck.Clear();
                dr = null;
                
            }
            curadr = 0;
            bssflag = false;
            hadnoorg = true;

            FirstAdr = 1000000;
            LastAdr = -1000000;
        }
        public void Putval(int adr, long value)
        {
            if (dr == null)
            {
                deck = new List<Card>();
                dr = new DeckReader(tr, deck);
                if (hadnoorg)
                {
                    startline = thisline;
                    startpage = thispage;
                }
            }
            if (bssflag)
                curadr = adr;
            if (adr != curadr)
            {
                throw new InvalidDataException(string.Format("{0} {1} wrongadr", thispage, thisline));
            }
            if (dr.Read(out int dadr, out long dvalue))
            {
                if ((adr != dadr || value != dvalue)&&(dr.Cardtype()!='F'))
                {
                    Console.WriteLine("Abweichung {0} {1}", thispage, thisline);
                    Console.WriteLine("Card {0} {1}", Convert.ToString(dadr, 8).PadLeft(5, '0'), Convert.ToString(dvalue, 8).PadLeft(12, '0'));
                    Console.WriteLine("List {0} {1}", Convert.ToString(adr, 8).PadLeft(5, '0'), Convert.ToString(value, 8).PadLeft(12, '0'));
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine("Abweichung {0} {1}", thispage, thisline);
                Console.WriteLine("Card Transfer {0}", Convert.ToString(dadr, 8).PadLeft(5, '0'));
                Console.WriteLine("List {0} {1}", Convert.ToString(adr, 8).PadLeft(5, '0'), Convert.ToString(value, 8).PadLeft(12, '0'));
                Environment.Exit(-1);
            }

            if (adr < FirstAdr)
                FirstAdr = adr;
            if (adr > LastAdr)
                LastAdr = adr;

            curadr++;
        }
        public Lister(TapeReader tp,string o)
        {
            tr = tp;
            dr = null;
            deck = new List<Card>();
            hadnoorg = true;
            out_p = o;
            FirstAdr = 1000000;
            LastAdr = -1000000;
            

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string[] skipine = new string[] { "0SHARE ASSEMBLER STATISTICS", "0TAPE  TOTAL", " INP", " LIB", " COL", "0NUMBER OF ON-LINE INPUT RECORDS", "0NUMBER OF OFF-LINE PRINT RECORDS", "0NUMBER OF SYMBOLS" };
            int skipline = 0;
            bool isadr;
            string out_p = @"D:\Section4_8K";
            //string out_p= @"D:\F2Diag\F2Diag";
            //string out_p = @"D:\NYBL1\NYBL1";
            //tring out_p = @"D:\MR\MR";
            using (StreamReader r = new StreamReader(@"C:\Users\Renke\Documents\Section4_8K.txt"))
            //using (StreamReader r = new StreamReader(@"D:\OUT.TXT"))
            //using (StreamReader r = new StreamReader(@"C:\cygwin64\home\Renke\F2Diag.lst"))
            //using (StreamReader r = new StreamReader(@"C:\cygwin64\home\Renke\NYBL1.lst"))
            //using (StreamReader r = new StreamReader(@"C:\cygwin64\home\Renke\MR.lst"))

            using (TapeReader d = new TapeReader(@"C:\cygwin64\home\Renke\Section4_8K.cbn", true))
            //using (TapeReader d = new TapeReader(@"C:\cygwin64\home\Renke\fort2_roberto_20180225.cbn", true))
            //using (TapeReader d = new TapeReader(@"C:\cygwin64\home\Renke\F2Diag.cbn", true))
            //using (TapeReader d = new TapeReader(@"C:\cygwin64\home\Renke\NYBL1.cbn", true))
            //using (TapeReader d = new TapeReader(@"C:\cygwin64\home\Renke\MR.cbn", true))
            {
                Lister L = new Lister(d, out_p);
                while (!r.EndOfStream)
                {
                    string line = r.ReadLine().PadRight(114);
                    if (line.Length > 120 && line.Substring(120, 4) == "Page")
                        L.Setpage(int.Parse(line.Substring(125)));
                    else
                        L.Nextline();
                    if (skipline > 0)
                    {
                        if (!line.StartsWith(skipine[8 - skipline]))
                            throw new Exception("invalid line");
                        skipline--;
                        continue;
                    }
                    if (line.StartsWith(skipine[0]))
                    {
                        skipline = 7;
                        continue;
                    }
                    if (line.Substring(6).StartsWith("                  "))
                    {
                        isadr = true;
                        for (int i = 24; i <= 28; i++)
                        {
                            if (line[i] < '0' || line[i] > '7')
                            {
                                isadr = false;
                                break;
                            }
                        }
                        if (isadr)
                        {
                            int adr = Convert.ToInt32(line.Substring(24, 5), 8);
                            switch (line.Substring(37, 3))
                            {
                                case "ORG":
                                    L.ORG(adr);
                                    break;
                                case "BES":
                                    L.BES(adr);
                                    break;
                                case "BSS":
                                    L.BSS(adr);
                                    break;
                                case "END":
                                    if (line[41] != ' ')
                                        L.END(adr);
                                    else
                                        L.END(null);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        isadr = true;
                        for (int i = 7; i <= 11; i++)
                        {
                            if (line[i] < '0' || line[i] > '7')
                            {
                                isadr = false;
                                break;
                            }
                        }
                        if (isadr)
                        {
                            bool uflag = line[1] == 'U';
                            int adr = Convert.ToInt32(line.Substring(7, 5), 8);
                            bool neg = line[13] == '-';
                            if (line[15] == ' ' && line[21] == ' ' && line[23] == ' ')
                                line = line[14] + line.Substring(16, 5) + line[22] + line.Substring(24, 5);
                            else
                                line = line.Substring(14, 12);
                            if (uflag)
                                line = line.Replace(' ', '0');
                            long val = Convert.ToInt64(line, 8);
                            if (neg)
                                val |= 0x800000000L;
                            L.Putval(adr, val);

                        }
                    }
                }
                L.END(null);
            }
        }

    }

}

