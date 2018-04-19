using System;
using System.Collections.Generic;
using System.IO;
using Tools704;
namespace Split
{
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
                            Console.WriteLine("invalid card type {0})", tt);
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
