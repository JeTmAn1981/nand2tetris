using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace HackVMTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser(args[0]);
            parser.CodeWriter.writeInit();

            string currentPath = Directory.GetCurrentDirectory();

            string[] fileEntries = Directory.GetFiles(currentPath);
           
            foreach (string fileName in fileEntries)
               if (fileName.Substring(fileName.Length - 3, 3) == ".vm")
               {
                   Console.WriteLine("processing " + fileName);
                   parser.parse(fileName);
               }

            parser.CodeWriter.close();
        }
    }

    class Parser
    {
        private StreamReader sr;
        private string ASMFile;
        private string CurrentCommand;
        public CodeWriter CodeWriter;
        private const string C_ARITHMETIC = "Arithmetic";
        private const string C_POP = "Pop";
        private const string C_PUSH = "Push";
        private const string C_LABEL = "Label";
        private const string C_GOTO = "Goto";
        private const string C_IF = "If";
        private const string C_FUNCTION = "Function";
        private const string C_RETURN = "Return";
        private const string C_CALL = "Call";
        
        public Parser(String OutputFileName)
        {
            ASMFile = OutputFileName;
            CodeWriter = new CodeWriter(OutputFileName);
        }

        private void RunTests()
        {
            //CurrentCommand = "goto TestLabel";
            //Console.WriteLine(commandType());
            //CodeWriter.writeGoto(arg1());

            //CurrentCommand = "label TestLabel";
            //Console.WriteLine(commandType());
            //CodeWriter.writeLabel(arg1());

            //CurrentCommand = "if-goto TestLabel";
            //Console.WriteLine(commandType());
            //CodeWriter.writeIf(arg1());

            //CodeWriter.writeCall("TestFunction", "2");
            CodeWriter.writeReturn();
            CodeWriter.close();
        }

        public void parse(String VMFileName)
        {
            CodeWriter.CurrentVMFile = VMFileName;
            sr = new StreamReader(VMFileName);

            //RunTests();
            //return;
            
            while (hasMoreCommands())
            {
                advance();

                if (commandType() == C_ARITHMETIC)
                    CodeWriter.writeArithmetic(arg1());
                else if (commandType() == C_PUSH)
                    CodeWriter.writePushPop("push", arg1(), arg2());
                else if (commandType() == C_POP)
                    CodeWriter.writePushPop("pop", arg1(), arg2());
                else if (commandType() == C_LABEL)
                    CodeWriter.writeLabel(arg1());
                else if (commandType() == C_GOTO)
                    CodeWriter.writeGoto(arg1());
                else if (commandType() == C_IF)
                    CodeWriter.writeIf(arg1());
                else if (commandType() == C_CALL)
                    CodeWriter.writeCall(arg1(), arg2());
                else if (commandType() == C_FUNCTION)
                    CodeWriter.writeFunction(arg1(), arg2());
                else if (commandType() == C_RETURN)
                    CodeWriter.writeReturn();

                //Console.WriteLine(CurrentCommand);
                //Console.WriteLine(commandType());
                //Console.WriteLine(arg1());
                //Console.WriteLine(arg2());
                //Console.WriteLine("");
            }

            //CodeWriter.close();
        }

        public string arg1()
        {
            string CurrentCommandType = commandType();

            if (CurrentCommandType == C_ARITHMETIC || CurrentCommandType == C_RETURN )
                return CurrentCommand;
            else if (CurrentCommandType == C_PUSH || CurrentCommandType == C_POP || CurrentCommandType == C_GOTO || CurrentCommandType == C_LABEL || CurrentCommandType == C_IF || CurrentCommandType == C_CALL || CurrentCommandType == C_FUNCTION)
            {
                string[] CommandComponents = CurrentCommand.Split(' ');
                return CommandComponents[1];
            }

            return "No arg 1";
        }

        public string arg2()
        {
            string CurrentCommandType = commandType();

            if (CurrentCommandType == C_PUSH || CurrentCommandType == C_POP || CurrentCommandType == C_FUNCTION || CurrentCommandType == C_CALL)
            {
                string[] CommandComponents = CurrentCommand.Split(' ');
                return CommandComponents[2];
            }

            return "No arg 2";
        }

        public void advance()
        {
            CurrentCommand = sr.ReadLine();
            RemoveExtraneousInformation();
            
            if (hasMoreCommands() && CurrentCommand == "")
                advance();
        }

        private void RemoveExtraneousInformation()
        {
            int commentindex = CurrentCommand.IndexOf('/');

            if (commentindex != -1)
                CurrentCommand = CurrentCommand.Substring(0, commentindex);

            CurrentCommand = CurrentCommand.Trim();
            CurrentCommand = CurrentCommand.ToLower();
        }

        public Boolean hasMoreCommands()
        {
            return sr.EndOfStream.Equals(false);
        }

        public string commandType()
        {
            try
            {
                string left3 = "", left2 = "";

                if (CurrentCommand.Length >= 3)
                    left3 = CurrentCommand.Substring(0, 3);

                if (CurrentCommand.Length >= 2)
                    left2 = CurrentCommand.Substring(0, 2);

                if (left3 == "add" || left3 == "sub" || left3 == "neg" || left3 == "and" || left3 == "not" || left2 == "eq" || left2 == "gt" || left2 == "lt" || left2 == "or")
                    return C_ARITHMETIC;
                else if (left3 == "pop")
                    return C_POP;
                else if (CurrentCommand.Substring(0, 4) == "push")
                    return C_PUSH;
                else if (CurrentCommand.Substring(0, 4) == "goto")
                    return C_GOTO;
                else if (CurrentCommand.Substring(0, 4) == "call")
                    return C_CALL;
                else if (CurrentCommand.Substring(0, 5) == "label")
                    return C_LABEL;
                else if (CurrentCommand.Substring(0, 6) == "return")
                    return C_RETURN;
                else if (CurrentCommand.Substring(0, 7) == "if-goto")
                    return C_IF;
                else if (CurrentCommand.Substring(0, 8) == "function")
                    return C_FUNCTION;

            }
            catch
            {
                Console.WriteLine("error processing command type - " + CurrentCommand);
            }
            
            return "";
        }
    }
 
    class CodeWriter
    {
        private String outputFile;
        public string CurrentVMFile;
        private StreamWriter sw;
        private int LoopCount;
        private int ReturnAddressCount;
        private string CurrentFunctionName;
        private bool registerBaseOnly;
        public CodeWriter(String FileName)
        {
            outputFile = FileName;
            sw = new StreamWriter(outputFile.Replace(".vm",".asm"));
            LoopCount = 0;
            ReturnAddressCount = 0;
            CurrentFunctionName = "Main$";
            registerBaseOnly = false;
        }

        public void writeCall(String functionName, String numArgs)
        {
            //write code to push return address - how to reference this?
            //writePushPop("push", "ReturnAddress" + ReturnAddressCount.ToString());
            sw.WriteLine("@ReturnAddress" + ReturnAddressCount.ToString());
            sw.WriteLine("D=A");
            sw.WriteLine("@SP");
            sw.WriteLine("A=M");
            sw.WriteLine("M=D");
            WriteStackPointerChange("+");
            
            //Save current positions of local, argument, this and that memory segments to the stack
            registerBaseOnly = true;
            writePushPop("push","local");
            writePushPop("push", "argument");
            writePushPop("push", "this");
            writePushPop("push", "that");
            registerBaseOnly = false;

            //Set the argument stack to start at the position of the first argument.  This position is calculated
            //by looking at the stack's position and then subtracting 5 for the five values we just pushed (return address,
            //local, etc.) and then subtracting the number of arguments for the function being called.
            int argumentOffset = Convert.ToInt16(numArgs) + 5;

            sw.WriteLine("@SP");
            sw.WriteLine("D=M");
            sw.WriteLine("@" + argumentOffset);
            sw.WriteLine("D=D-A");
            sw.WriteLine("@ARG");
            sw.WriteLine("M=D");

            //Adjust the current local address to point to the empty spot right above the top of the current main stack.
            //This local address can then be used to index into that empty segment outside the main stack for 
            //storing local variables.  Pushing a variable onto the stack while in the function will put it in 
            //this local memory segment rather than in the main stack.
            sw.WriteLine("@SP");
            sw.WriteLine("D=M");
            sw.WriteLine("@LCL");
            sw.WriteLine("M=D");

            sw.WriteLine("@" + functionName);
            sw.WriteLine("0;JMP");
            writeLabel("ReturnAddress" + ReturnAddressCount.ToString(),false);
            ReturnAddressCount++;
        }

        public void writeReturn()
        {
            //Save return address
            sw.WriteLine("@LCL");
            sw.WriteLine("D=M");
            sw.WriteLine("@5");
            sw.WriteLine("A=D-A");
            sw.WriteLine("D=M");
            sw.WriteLine("@RETADDR");
            sw.WriteLine("M=D");
            
            writePushPop("pop", "argument", "0");

            //sw.WriteLine("@LCL");
            //sw.WriteLine("D=M");
            //sw.WriteLine("@FRAME");
            //sw.WriteLine("M=D");

            //Restore stack pointer
            sw.WriteLine("@ARG");
            sw.WriteLine("D=M+1");
            sw.WriteLine("@SP");
            sw.WriteLine("M=D");

            RestoreRegister("THAT", "1");
            RestoreRegister("THIS", "2");
            RestoreRegister("ARG", "3");

            sw.WriteLine("@4");
            sw.WriteLine("D=A");
            sw.WriteLine("@LCL");
            sw.WriteLine("A=M-D");
            sw.WriteLine("D=M");
            sw.WriteLine("@LCL");
            sw.WriteLine("M=D");
            
            sw.WriteLine("@RETADDR");
            sw.WriteLine("A=M");
            sw.WriteLine("0;JMP");
        }

        private void RestoreRegister(String registerName, String offset)
        {
            sw.WriteLine("@LCL");
            sw.WriteLine("D=M");
            sw.WriteLine("@" + offset);
            sw.WriteLine("A=D-A");
            sw.WriteLine("D=M");
            sw.WriteLine("@" + registerName);
            sw.WriteLine("M=D");
        }

        public void writeFunction(String functionName, String numLocals)
        {
            writeLabel(functionName, false);

            for (int i = 0; i < Convert.ToInt32(numLocals); i++)
                writePushPop("push", "constant","0");
        }

        public void writeInit()
        {
            sw.WriteLine("@256");
            sw.WriteLine("D=A");
            sw.WriteLine("@SP");
            sw.WriteLine("M=D");

            writeCall("sys.init", "0");
            
        }

        public string writeLabel(String label, bool includeFunctionName = true)
        {
            String completelabel = "(" + (includeFunctionName ? CurrentFunctionName : "") + label + ")";

            sw.WriteLine(completelabel);
            Console.WriteLine(completelabel);

            return completelabel;
        }

        public void writeIf(String label)
        {
            sw.WriteLine("@SP");
            sw.WriteLine("A=M-1");
            sw.WriteLine("D=M");
            sw.WriteLine("@" + CurrentFunctionName + label);
            sw.WriteLine("D;JLT");

            Console.WriteLine("@SP");
            Console.WriteLine("A=M-1");
            Console.WriteLine("D=M");
            Console.WriteLine("@" + CurrentFunctionName + label);
            Console.WriteLine("0;JLT");
        }

        public void writeGoto(String label)
        {
            sw.WriteLine("@" + CurrentFunctionName  + label);
            sw.WriteLine("0;JMP");
            Console.WriteLine("@" + CurrentFunctionName  + label);
            Console.WriteLine("0;JMP");
        }

        public void writeArithmetic(String Command)
        {
            if (Command == "add")
                WriteDoubleArgumentArithmetic("+");
            else if (Command == "sub")
                 WriteDoubleArgumentArithmetic("-");
            else if (Command == "neg")
                WriteSingleArgumentArithmetic("-");
            else if (Command == "not")
                WriteSingleArgumentArithmetic("!");
            else if (Command == "and")
                WriteDoubleArgumentArithmetic("&");
            else if (Command == "or")
                WriteDoubleArgumentArithmetic("|");
            else if (Command == "eq" || Command == "gt" || Command == "lt")
                WriteBooleanEval(Command.ToUpper());
         }

        public void writePushPop(String Command, String Arg1, String Arg2 = "")
        {
            int secondargument;

            WriteMemoryReference(Arg1, Arg2, Command);

            if (Command == "push")
            {
                if (Arg1 == "temp")
                {
                    secondargument = Convert.ToInt16(Arg2) + 5;

                    sw.WriteLine("@" + secondargument);
                    sw.WriteLine("D=M");
                }
                else if (Arg1 == "static")
                {
                    Console.WriteLine("@" + Path.GetFileName(CurrentVMFile).Replace(".vm", "").Replace(".asm","") + "." + Arg2);
                    sw.WriteLine("@" + Path.GetFileName(CurrentVMFile).Replace(".vm", "").Replace(".asm", "") + "." + Arg2);
                    sw.WriteLine("D=M");
                }

                sw.WriteLine("@SP");
                sw.WriteLine("A=M");
                sw.WriteLine("M=D");

                WriteStackPointerChange("+");
            }
            else if (Command == "pop")
            {
                if (Arg1 == "temp")
                {
                    WriteSetDToTopStackValue();

                    secondargument = Convert.ToInt16(Arg2) + 5;
                    sw.WriteLine("@" + secondargument);
                    sw.WriteLine("M=D");
                }
                else if (Arg1 == "static")
                {
                    WriteSetDToTopStackValue();

                    sw.WriteLine("@" + Path.GetFileName(CurrentVMFile).Replace(".vm", "").Replace(".asm", "") + "." + Arg2);
                    sw.WriteLine("M=D");
                }
                else
                {
                    sw.WriteLine("@R13");
                    sw.WriteLine("M=D");

                    WriteSetDToTopStackValue();

                    sw.WriteLine("@R13");
                    sw.WriteLine("A=M");
                    sw.WriteLine("M=D");
                }

                WriteStackPointerChange("-");
            }
        }

        public void close()
        {
            sw.Close();
        }

        private void WriteSingleArgumentArithmetic(String Operator)
        {
            sw.WriteLine("@SP");
            sw.WriteLine("A=M-1");
            sw.WriteLine("M=" + Operator + "M");
        }

        private void WriteDoubleArgumentArithmetic(String Operator)
        {
                sw.WriteLine("@SP");
                sw.WriteLine("A=M-1");
                sw.WriteLine("D=M");
                WriteStackPointerChange("-");
                sw.WriteLine("@SP");
                sw.WriteLine("A=M-1");
                sw.WriteLine("M=M" + Operator + "D");
                //Leave stack pointer decremented by 1 since we are reducing two arguments to one
        }

        private void WriteStackPointerChange(string changeoperator)
        {
            sw.WriteLine("@SP");
            sw.WriteLine("M=M" + changeoperator + "1");
        }

        private void WriteBooleanEval(String evaluationtype)
        {
            WriteDoubleArgumentArithmetic("-");
        
            sw.WriteLine("@SP");
            sw.WriteLine("A=M-1");
            sw.WriteLine("D=M");

            sw.WriteLine("@" + CurrentFunctionName  + "EVALTRUE" + LoopCount);
            sw.WriteLine("D;J" + evaluationtype);

            sw.WriteLine("@" + CurrentFunctionName + "EVALFALSE" + LoopCount);
            sw.WriteLine("0;JMP");

            sw.WriteLine("(" + CurrentFunctionName + "EVALTRUE" + LoopCount + ")");
            WriteSetEvalValue("-1");
            sw.WriteLine("@" + CurrentFunctionName + "RESUME" + LoopCount);
            sw.WriteLine("0;JMP");

            sw.WriteLine("(" + CurrentFunctionName + "EVALFALSE" + LoopCount + ")");
            WriteSetEvalValue("0");
            sw.WriteLine("@" + CurrentFunctionName + "RESUME" + LoopCount);
            sw.WriteLine("0;JMP");

            sw.WriteLine("(" + CurrentFunctionName + "RESUME" + LoopCount + ")");

            LoopCount++;
       }

       private void WriteSetEvalValue(String value)
        {
            sw.WriteLine("@SP");
            sw.WriteLine("A=M-1");
            sw.WriteLine("M=" + value);
        }

        private void WritePrepareArithmeticArguments()
        {
            //Save first argument to R14
            WriteStackPointerChange("-");
            WriteStackPointerChange("-");
            sw.WriteLine("@SP");
            sw.WriteLine("D=M");
            sw.WriteLine("@R14");
            sw.WriteLine("M=D");

            //Save second argument to D
            WriteStackPointerChange("+");
            sw.WriteLine("@SP");
            sw.WriteLine("D=M");
            sw.WriteLine("@R14");

            WriteStackPointerChange("+");
        }

        
        private void WriteSetDToTopStackValue()
        {
            sw.WriteLine("@SP");
            sw.WriteLine("A=M-1");
            sw.WriteLine("D=M");
        }

        private void WriteMemoryReference(String Arg1, String Arg2, String indextype)
        {
            if (Arg1 == "local")
                sw.WriteLine("@LCL");
            else if (Arg1 == "argument")
                sw.WriteLine("@ARG");
            else if (Arg1 == "this")
                sw.WriteLine("@THIS");
            else if (Arg1 == "that")
                sw.WriteLine("@THAT");
            else if (Arg1 == "constant")
            {
                sw.WriteLine("@" + Arg2);
                sw.WriteLine("D=A");
            }
            else
            {
                sw.WriteLine("@" + Arg1);
                sw.WriteLine("D=M");
            }

            if (Arg1 == "local" || Arg1 == "argument" || Arg1 == "this" || Arg1 == "that")
                WriteGetMemoryOffset(Arg2, indextype);
                
        }

        private void WriteGetMemoryOffset(String index, String offsettype = "Push")
        {
            sw.WriteLine("D=M");

            if (!registerBaseOnly)
            {
                sw.WriteLine("@" + index);

                if (offsettype.ToLower() == "push")
                {
                    sw.WriteLine("A=D+A");
                    sw.WriteLine("D=M");
                }
                else
                    sw.WriteLine("D=D+A");

            }
        }

    }
}
