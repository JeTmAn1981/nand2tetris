using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace HackAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser(args[0]);

            parser.parse();
        }
    }

   class Parser
    {
       private StreamReader sr;
       private StreamWriter sw;
       private string ASMFile;
       private string CurrentCommand;
       private Hashtable Symbols;
       private int ProgramCounter;
       private int VariableAddress;
       private const string A_COMMAND = "Address";
       private const string C_COMMAND = "Compute";
       private const string L_COMMAND = "Label";
       private const string NONE_COMMAND = "None";

        public Parser(String AssemblyFile)
        {
           ASMFile = AssemblyFile;
           InitializeSymbols();
           ProgramCounter = -1;
           VariableAddress = 16;
        }

       void InitializeSymbols()
        {
            Symbols = new Hashtable();

            Symbols.Add("@SP", 0);
            Symbols.Add("@LCL", 1);
            Symbols.Add("@ARG", 2);
            Symbols.Add("@THIS", 3);
            Symbols.Add("@THAT", 4);

           for (int i = 0; i < 16; i++)
               Symbols.Add
("@R" + i.ToString(), i);

           Symbols.Add("@SCREEN", 16384);
           Symbols.Add("@KBD", 24576);
        }

       public void parse()
        {
            sr = new StreamReader(ASMFile);
           
            while (hasMoreCommands())
                {
                    advance();
                    UpdateSymbolTable(false);
                }

            sr = new StreamReader(ASMFile);
            sw = new StreamWriter(ASMFile.Replace(".asm", ".hack"));

             while (hasMoreCommands())
             {
                 advance();
                 UpdateSymbolTable(true);
                 ReplaceCommandSymbols();
                 WriteCurrentCommand();
                 WriteBinary();
             }

             sw.Close();

             foreach (string key in Symbols.Keys)
             {
                 Console.WriteLine(String.Format("{0}: {1}", key, Symbols[key]));
             }
        }

       void ReplaceCommandSymbols()
       {
           if (commandType() == A_COMMAND && Symbols.Contains(CurrentCommand))
               CurrentCommand = "@" + Symbols[CurrentCommand];
       }

       public void advance()
        {
           CurrentCommand = sr.ReadLine();

           int commentindex = CurrentCommand.IndexOf('/');

           if (commentindex != -1)
               CurrentCommand = CurrentCommand.Substring(0, commentindex);

           CurrentCommand = CurrentCommand.Trim();

           if (commandType() == A_COMMAND || commandType() == C_COMMAND)
               ProgramCounter++;
           else if (commandType() == NONE_COMMAND && hasMoreCommands() || CurrentCommand == "")
                advance();
           

       }

       public void WriteCurrentCommand()
       {
           Console.WriteLine(CurrentCommand);
           Console.WriteLine(GetBinary()); 
       }

       public void WriteBinary()
       {
           if (GetBinary() != "")
           sw.WriteLine(GetBinary());
         
       }

       public string GetBinary()
       {
           String TranslatedBinary = "";
           String currentBinary = "";

           if (commandType() == A_COMMAND)
           {
               currentBinary = Convert.ToString(symbol(), 2);

               while (currentBinary.Length < 16)
                   currentBinary = "0" + currentBinary;

               if (currentBinary[0] == '1')
                   currentBinary = "0" + currentBinary.Substring(1, currentBinary.Length - 1);

               TranslatedBinary = currentBinary;
           }
           else if (commandType() == C_COMMAND)
           {
               String Abit = "0";
               String currentDest = dest();
               String currentComp = comp();
               String currentJump = jump();

               if (CurrentCommand == "D;JGT")
                   Console.WriteLine("comp is " + comp());

              // if (currentDest.IndexOf("M") != -1 || currentComp.IndexOf("M") != -1)
               if (currentComp.IndexOf("M") != -1)
                   Abit = "1";
               else
                   Console.WriteLine(currentDest + " - " + currentComp);


               TranslatedBinary = "111" + Abit;
               TranslatedBinary += Code.comp(currentComp);
               TranslatedBinary += Code.dest(currentDest);
               TranslatedBinary += Code.jump(currentJump);                     
           }

           return TranslatedBinary;
       }

        public Boolean hasMoreCommands()
        {
            return sr.EndOfStream.Equals(false);

            //if (sr.EndOfStream.Equals(false))
            //    return true;

            //sr.Close();
            //sw.Close();

            //return false;
        }

       public string commandType()
       {
           if (CurrentCommand.Length > 0)
           {
               string leftCharacter = CurrentCommand.Substring(0, 1);

               if (leftCharacter == "@")
                   return A_COMMAND;
               else if (leftCharacter == "(")
                   return L_COMMAND;
               else if ((CurrentCommand.Contains("=") || CurrentCommand.Contains(";")) && leftCharacter != @"/")
                   return C_COMMAND;

           }

           return NONE_COMMAND;
       }

       void UpdateSymbolTable(bool SecondPass)
       {
              if (commandType() == L_COMMAND)
               {
                   if (!Symbols.ContainsKey("@" + CurrentCommand.Replace("(","").Replace(")","")))
                       Symbols.Add("@" + CurrentCommand.Replace("(", "").Replace(")", ""), ProgramCounter + 1);
               }
               else if (commandType() == A_COMMAND && SecondPass)
               {
                   if (!Symbols.ContainsKey(CurrentCommand))
                   {
                       int n;
                       bool isNumeric = int.TryParse(CurrentCommand.Replace("@",""), out n);

                       if (!isNumeric)
                       {
                           Symbols.Add(CurrentCommand, VariableAddress);
                           VariableAddress++;
                       }
                       
                   }
               }
       }

       int symbol()
       {
           if (commandType() == A_COMMAND || commandType() == L_COMMAND)
               return Convert.ToInt16(CurrentCommand.Replace("@", "").Replace("(", "").Replace(")", ""));

           return 0;
       }

       string dest()
       {
           if (commandType() == C_COMMAND)
           {
               int destIndex = CurrentCommand.IndexOf('=');

               if (destIndex != -1)
                   return CurrentCommand.Substring(0, destIndex);
               else
                   return "";
           }

           return "";
       }

       string comp()
       {
           if (commandType() == C_COMMAND)
           {
               int destIndex = CurrentCommand.IndexOf('=');

               if (destIndex != -1)
                   return CurrentCommand.Substring(destIndex + 1, CurrentCommand.Length - destIndex - 1);
               else
                   return CurrentCommand.Substring(0, CurrentCommand.IndexOf(";"));
           }

           return "";
       }

       string jump()
       {
           if (commandType() == C_COMMAND)
           {
               int destIndex = CurrentCommand.IndexOf(';');

               if (destIndex == -1)
                   return "";
               else
                   return CurrentCommand.Substring(destIndex + 1, CurrentCommand.Length - destIndex - 1);
           }

           return "";
       }
    }

    class Code
    {
     public static string dest(String dest)
        {
            string Dbit = "0", Mbit = "0", Abit = "0";

            for (int i = 0; i < dest.Length; i++)
            {
                if (dest[i] == 'D')
                    Dbit = "1";

                if (dest[i] == 'M')
                    Mbit = "1";

                if (dest[i] == 'A')
                    Abit = "1";

            }

            return Abit + Dbit + Mbit;
        }

     public static string comp(String comp)
        {
            String compbits = "000000";
            //String compbit0 = "0";
            
            //List<String> Bit0Strings = new List<String>()
            //{"0","-1","1","A","!A"};

            //if (Bit0Strings.Any(s => comp == s))
            //    compbit0 = "1";
            
            if (comp == "0")
                compbits = "101010";
            else if (comp == "1")
                compbits = "111111";
            else if (comp == "-1")
                compbits = "111010";
            else if (comp == "D")
                compbits = "001100";
            else if (comp == "A" || comp == "M")
                compbits = "110000";
            else if (comp == "!D")
                compbits = "001101";
            else if (comp == "!A" || comp == "!M")
                compbits = "110001";
            else if (comp == "-D")
                compbits = "001111";
            else if (comp == "-A" || comp == "-M")
                compbits = "110011";
            else if (comp == "D+1")
                compbits = "011111";
            else if (comp == "A+1" || comp == "M+1")
                compbits = "110111";
            else if (comp == "D-1")
                compbits = "001110";
            else if (comp == "A-1" || comp == "M-1")
                compbits = "110010";
            else if (comp == "D+A" || comp == "D+M")
                compbits = "000010";
            else if (comp == "D-A" || comp == "D-M")
                compbits = "010011";
            else if (comp == "A-D" || comp == "M-D")
                compbits = "000111";
            else if (comp == "D&A" || comp == "D&M")
                compbits = "000000";
            else if (comp == "D|A" || comp == "D|M")
                compbits = "010101";
         
            return compbits;
                    }

     public static string jump(String jump)
        {
            string JumpBits = "000";

               if (jump == "JGT")
                JumpBits = "001";
            else if (jump == "JEQ")
                JumpBits = "010";
            else if (jump == "JGE")
                JumpBits = "011";
                          else if (jump == "JLT")
                JumpBits = "100";
                            else if (jump == "JNE")
                JumpBits = "101";
                          else if (jump == "JLE")
                JumpBits = "110";
                       else if (jump == "JMP")
                JumpBits = "111";
            
            return JumpBits;    
        }
    }
}
