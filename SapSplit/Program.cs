using System;
using System.Collections.Generic;
using System.IO;
using Tools704;
namespace Split
{
    class DeckReader
    {
        TapeReader r; /* stores tapereader */
        Card crd; /* stores card */
        bool transferread; /* transfercard already read */
        BinaryCardConverter.CardType t; /* card type */
        int n; /* number of words already read */
        int cur_adr; /* currend address on card */
        List<Card> deck; /* stores already read cards */
        public char Cardtype()
        {
            char dt;
            dt = ' ';
            switch (t)
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
        public DeckReader(TapeReader re, List<Card> d) /* constructor */
        {
            /* reads cards in CBN format from tape, and stores them to d */

            transferread = false;
            r = re;
            deck = d;
            if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* read card */
                throw new Exception("last card read");
            CBNConverter.FromCBN(mrecord, out crd); /* convert */

            t = BinaryCardConverter.GetCardType(crd); /* detect card type */
            n = 0; /* nothing read yet */
            switch (t) /* evaluate type */
            {
                case BinaryCardConverter.CardType.Full:
                    cur_adr = 0; /* a full deck start at 0*/
                    break;
                case BinaryCardConverter.CardType.Abs:
                    cur_adr = (int)crd.W9L.A; /* read start address */
                    break;
                case BinaryCardConverter.CardType.Rel:
                    cur_adr = (int)crd.W9L.A; /* read start address */
                    break;
                case BinaryCardConverter.CardType.Transfer:
                case BinaryCardConverter.CardType.RelTransfer:
                    break;
                default:
                    throw new InvalidDataException("invalid card");
            }
            deck.Add(crd); /* add card to deck */
        }
        public bool Read(out int adr, out long value) /* read  address and word from cards, return false transfercard read, then value=0, adr=transferaddress */
        {
            adr = 0;
            value = 0;
            if (transferread)
                throw new InvalidOperationException("transfercard alread read");
            switch (t)
            {
                case BinaryCardConverter.CardType.Full:
                    if (n == 24) /* all words already read? */
                    {
                        /* get next card */
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0)
                            throw new Exception("last card read");

                        /* convert */
                        CBNConverter.FromCBN(mrecord, out crd);
                        deck.Add(crd); /* add to deck */
                        BinaryCardConverter.CardType tt = BinaryCardConverter.GetCardType(crd);
                        if (tt != BinaryCardConverter.CardType.Full) /* check card type */
                            Console.WriteLine("invalid card type {0})", tt);
                        n = 0; /* nothing read yet */


                    }
                    adr = cur_adr; /* get address */
                    value = (long)crd.C[n].LW; /* get word */
                    /* count  */
                    n++;
                    cur_adr++;
                    return true; /* word from card read */
                case BinaryCardConverter.CardType.Abs:
                    if (n == crd.W9L.D) /* all words already read? */
                    {
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* get next card */
                            throw new Exception("last card read");
                        CBNConverter.FromCBN(mrecord, out crd); /* convert */
                        deck.Add(crd); /* add to deck */
                        BinaryCardConverter.CardType nt = BinaryCardConverter.GetCardType(crd); /* check card type */
                        if (nt == BinaryCardConverter.CardType.Transfer) /* transfercard ? */
                        {
                            adr = (int)crd.W9L.A;/* get transfer address */
                            value = 0; /*  0 for transfercard */
                            transferread = true; /* set flag */
                            return false; /* transfercard read */
                        }
                        if (nt != t) /* type changed ?*/
                            throw new Exception("invalid card type");
                        n = 0; /* reset word counter */
                        cur_adr = (int)crd.W9L.A; /* set address */


                    }
                    adr = cur_adr; /* get address */
                    value = (long)crd.C[n + 2].LW; /* get word */
                    n++;/* count */
                    cur_adr++;
                    return true;  /* word from card read */
                case BinaryCardConverter.CardType.Rel:
                    if (n == crd.W9L.D) /* all words already read? */
                    {
                        if (r.ReadRecord(out bool binary, out byte[] mrecord) <= 0) /* get next card */
                            throw new Exception("last card read");
                        CBNConverter.FromCBN(mrecord, out crd); /* convert */
                        deck.Add(crd); /* add to deck */
                        BinaryCardConverter.CardType nt = BinaryCardConverter.GetCardType(crd);/* check card type */
                        if (nt == BinaryCardConverter.CardType.RelTransfer) /* transfercard ? */
                        {
                            adr = (int)crd.W9L.A;/* get transfer address */
                            value = 0;/*  0 for transfercard */
                            transferread = true; /* set flag */
                            return false; /* transfercard read */
                        }
                        if (nt != t)  /* type changed ?*/
                            throw new Exception("invalid card type");
                        n = 0; /* reset word counter */
                        cur_adr = (int)crd.W9L.A; /* set address */
                    }
                    adr = cur_adr; /* get address */
                    value = (long)crd.C[n + 4].LW; /* get word */
                    n++; /* count */
                    cur_adr++;
                    return true; /* word from card read */
                case BinaryCardConverter.CardType.Transfer:
                case BinaryCardConverter.CardType.RelTransfer:
                    {
                        adr = (int)crd.W9L.A;/* get transfer address */
                        value = 0;/*  0 for transfercard */
                        transferread = true; /* set flag */
                        return false; /* transfercard read */
                    }
                default:
                    throw new InvalidDataException("invalid card");
            }
        }
        public bool CardEmpty() /* check if no more words on card*/
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
                case BinaryCardConverter.CardType.Rel:
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
        int FirstAdr = 1000000;
        int LastAdr = -1000000;
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
            if (dr != null)
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
                if (hadnoorg)
                {
                    startline = thisline;
                    startpage = thispage;
                }
            }
            if (dr != null)
            {
                if (dr.Cardtype() == 'R')
                {
                    TransferAdr = 0;
                }
                if ((TransferAdr != null && (!hadnoorg || TransferAdr != 0)))
                {
                    if (dr.Read(out int adr, out long value))
                    {
                        Console.WriteLine("Difference {0} {1}", thispage, thisline);
                        Console.WriteLine("Card {0} {1}", Convert.ToString(adr, 8).PadLeft(5, '0'), Convert.ToString(value, 8).PadLeft(12, '0'));
                        Console.WriteLine("List Transfer {0}", Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0'));
                        Environment.Exit(-1);
                    }
                    else if (adr != TransferAdr)
                    {
                        Console.WriteLine("Difference {0} {1}", thispage, thisline);
                        Console.WriteLine("Card Transfer {0}", Convert.ToString(adr, 8).PadLeft(5, '0'));
                        Console.WriteLine("List Transfer {0}", Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0'));
                        Environment.Exit(-1);
                    }
                    if (dr.Cardtype() != 'T')
                        tch = 'T';
                }
                if (!dr.CardEmpty())
                {
                    Console.WriteLine("Difference {0} {1}", thispage, thisline);
                    Console.WriteLine("more data on card");
                    Environment.Exit(-1);
                }
                string x = "";
                if (startpage != 0)
                {
                    x += string.Format("Page{0}", startpage);
                }
                x += string.Format("Line{0}", startline);
                x += "_";
                x += dr.Cardtype();
                if (tch != ' ')
                    x += tch;
                Console.Write("{0}:{1} {2} {3} {4}\n", x, deck.Count, Convert.ToString(FirstAdr, 8).PadLeft(5, '0'), Convert.ToString(LastAdr, 8).PadLeft(5, '0'), TransferAdr != null ? Convert.ToString((int)TransferAdr, 8).PadLeft(5, '0') : "");
                using (TapeWriter tw = new TapeWriter(out_p + x + ".cbn", true))
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
                if ((adr != dadr || value != dvalue) && (dr.Cardtype() != 'F'))
                {
                    Console.WriteLine("Difference {0} {1}", thispage, thisline);
                    Console.WriteLine("Card {0} {1}", Convert.ToString(dadr, 8).PadLeft(5, '0'), Convert.ToString(dvalue, 8).PadLeft(12, '0'));
                    Console.WriteLine("List {0} {1}", Convert.ToString(adr, 8).PadLeft(5, '0'), Convert.ToString(value, 8).PadLeft(12, '0'));
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine("Difference {0} {1}", thispage, thisline);
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
        public Lister(TapeReader tp, string o)
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
    static class Program
    {
        static void Main(string[] args)
        {
            string[] skipine = new string[] { "0SHARE ASSEMBLER STATISTICS", "0TAPE  TOTAL", " INP", " LIB", " COL", "0NUMBER OF ON-LINE INPUT RECORDS", "0NUMBER OF OFF-LINE PRINT RECORDS", "0NUMBER OF SYMBOLS" };
            int skipline = 0;
            bool isadr;

            if(args.Length!=3)
            {
                Console.Error.WriteLine("Usage: SapSplit input.lst input.cbn output_dir");
                return;
            }            
            using (StreamReader r = new StreamReader(args[0]))            
            using (TapeReader d = new TapeReader(args[1], true))
            {
                Lister L = new Lister(d, args[2]);
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
