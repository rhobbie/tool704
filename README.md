# tool704
Tools for the IBM 704 simulator.

CleanDeck
  - Removes blank cards and card with all ones in the first word from the binary output of an UASAP and Fortran compiler run.
  
CopyCards
  - Copies card files and joins several card files into one.
  
MasterTape
  - Creates a dummy master tape for creating the Fortran II systemtapes.
  
PatchCard
  - Can change one or multiple words of a card. Needs a cardfile with a single card as input.
  
Punch
  - Converts an ASCII text file into a Hollerith coded cardfile  

RemoveTransfercard
  - Similar to CopyCards but removes an UASAP transfercard form the end of the cardfile.

SapSplit
  - Needs an UASAP listfile and a binary carddeck from the same UASAP run as input. It splits the cardfile according to the ORG and END statements in the listing.
  
ShowCards
  - Lists the content the binary cards of an UASAP/Fortran run and shows them as FUL/ABS/REL cards or Transfercard.
  
ShowFCards
  - Similar to ShowCards but shows all card as FUL cards.
  
ShowHCards
  - Shows the content of a Hollerith coded cardfile.
  
ShowTape
  - Shows the content of the BCD records of a tapefile. This tool can be used to display the BCD records of the SHARE tapes (needs then an 80 as second parameter).
  
SplitDeck
  - Splits a cardfile into two parts.
 
Tp2p7b
  - Converts a SimH tape in *.tp format into p7b format.

Tools704
  - Common classes for tape and card handling

TapeExtract
  - This tool needs one of the SHARE tapes as inputs.  (Currently working only for some of the tapes).  For each binary (BI) and symbolic (SY) record on the tape it creates one card file.
  The binary records are copied into a binary cardfile, the symbolic records are converted into a Hollerith coded cardfile.  A textfile is created which lists all records. 
  
WriteTape
  - Converts an ASCII text file into a BCD tapefile.
