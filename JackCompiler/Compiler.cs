using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace JackCompiler
{
    public class Compiler
    {
        private LinkedList<Token> tokens;
        private LinkedList<Hashtable> variablemaps;
        private String currentClassName, currentFunctionName;
        LinkedListNode<Token> currentToken;
        private Hashtable operatorDetail;
        private int whileCounter = 0, ifCounter = 0;
        public StreamWriter sw;
        
        public Compiler(LinkedList<Token> inputTokens, String fileName)
        {
            tokens = inputTokens;
            initializeoperatorDetail();
            sw = new StreamWriter(fileName.Replace(".jack", ".vm"));
        }

        private void initializeoperatorDetail()
        {
            operatorDetail = new Hashtable();
            operatorDetail.Add("+", new operatorDetail { vmCommand = "add",precedence = 2, associativity = "Left" });
            operatorDetail.Add("-", new operatorDetail { vmCommand = "sub",precedence = 2, associativity = "Left" });
            operatorDetail.Add("*", new operatorDetail { vmCommand = "call Math.multiply 2",precedence = 3, associativity = "Left" });
            operatorDetail.Add("/", new operatorDetail { vmCommand = "call Math.divide 2",precedence = 3, associativity = "Left" });

            operatorDetail.Add("~", new operatorDetail { vmCommand = "not" });
            operatorDetail.Add("&", new operatorDetail { vmCommand = "and" });
            operatorDetail.Add("|", new operatorDetail { vmCommand = "or" });
            operatorDetail.Add("<", new operatorDetail { vmCommand = "lt" });
            operatorDetail.Add(">", new operatorDetail { vmCommand = "gt" });
            operatorDetail.Add("=", new operatorDetail { vmCommand = "eq" });
        }

        public Stack operators = new Stack();
        public Queue output = new Queue();

        public void StartParse(String terminator = ";")
        {
            String outputString = "";

            operators = new Stack();
            output = new Queue();

            ParseExpression(terminator);
        }

        public void ParseExpression(String terminator = ";")
        {
            Boolean match = false;
            Regex intMatcher, negativeIntMatcher,functionMatcher, operatorMatcher, variableMatcher;
            Match matched;
            String variableReference = "", functionName = "";
            int parameterCount = 0;

            intMatcher = new Regex("^(" + Tokenizer.intRegex + ")");
            negativeIntMatcher = new Regex("^(" + Tokenizer.negativeIntRegex + ")");
            functionMatcher = new Regex("^(" + Tokenizer.functionRegex + ")");
            operatorMatcher = new Regex("^(" + Tokenizer.operatorRegex + ")");
            variableMatcher = new Regex("^(" + Tokenizer.identifierRegex + @")");

            while (getCurrentSequence() != terminator)
            {
                if (intMatcher.IsMatch(getCurrentSequence()))
                    output.Enqueue("push constant " + getCurrentSequence());
                if (negativeIntMatcher.IsMatch(getCurrentSequence()))
                {
                    output.Enqueue("push constant " + getCurrentSequence().Replace("-", ""));
                    output.Enqueue("neg");
                }
                else if (getCurrentSequence() == "null" || getCurrentSequence() == "false")
                    output.Enqueue("push constant 0");
                else if (getCurrentSequence() == "true")
                {
                    output.Enqueue("push constant 0");
                    output.Enqueue("not");
                }
                else if (variableMatcher.IsMatch(getCurrentSequence()))
                {
                    //Console.WriteLine("variable: " + getCurrentSequence());

                    if (currentToken.Next.Value.sequence == ".")
                    {
                        variableReference = GetVariableReference(getCurrentSequence());

                        String objectType = "";

                      //  Console.WriteLine("var reference: " + variableReference);

                        int localParameterCount = 0;

                        if (variableReference != getCurrentSequence() + " not found")
                        {
                            objectType = GetVariableReference(getCurrentSequence(), true);
                            functionName = "call " + objectType + ".";
                            output.Enqueue("push " + variableReference);
                            localParameterCount++;
                        }
                        else
                        {
                            functionName = "call " + getCurrentSequence() + ".";
                        }

                        advanceToken(2);
                        functionName += getCurrentSequence();

                        if (!(currentToken.Next.Value.sequence == "(" && currentToken.Next.Next.Value.sequence == ")"))
                            localParameterCount++;

                        functionName += " " + localParameterCount;

                        operators.Push(functionName);

                        //Console.WriteLine("function: " + functionName);
                    }
                    else if (currentToken.Next.Value.sequence == "(")
                    {
                        functionName = "call " + currentClassName + "." + getCurrentSequence();
                        functionName += currentToken.Next.Next.Value.sequence == ")" ? " 1" : " 2";
                        operators.Push(functionName);
                        output.Enqueue("push pointer 0");
                        //Console.WriteLine(functionName);
                    }
                    else if (currentToken.Next.Value.sequence == "[")
                    {

                        variableReference = getCurrentSequence();
                        //Console.WriteLine(variableReference + " is array");
                        advanceToken(2);

                        operators.Push("[");

                        ParseExpression("]");

                     //   Console.WriteLine("after parsing " + variableReference);


                        while (operators.Count > 0 && operators.Peek().ToString() != "[")
                        {
                            //Console.WriteLine(operators.Peek().ToString());
                            output.Enqueue(operators.Pop());
                        }

                        if (operators.Count > 0)
                        {
                            //Console.WriteLine("deleting " + operators.Peek().ToString());
                            operators.Pop();
                        }

                        output.Enqueue("push " + GetVariableReference(variableReference));
                        output.Enqueue("add");
                        output.Enqueue("pop pointer 1");
                        output.Enqueue("push that 0");
                    }
                    else
                        output.Enqueue("push " + GetVariableReference(getCurrentSequence()));
                }
                else if (getCurrentSequence() == ",")
                {
                    //Console.WriteLine("comma");
                    while (operators.Peek().ToString() != "(" && operators.Count > 0)
                        output.Enqueue(operators.Pop());

                    int i = 0, currentParameterCount;

                    Stack tempStack = new Stack();

                    while (operators.Count > 0 && !(operators.Peek().ToString().Length >= 4 && operators.Peek().ToString().Substring(0, 4) == "call"))
                    {
                        //Console.WriteLine(operators.Peek().ToString());
                        tempStack.Push(operators.Pop());
                    }

                    String functionCall = "";

                    if (operators.Count > 0)
                        functionCall = operators.Pop().ToString();

                    currentParameterCount = Convert.ToInt32(functionCall.Substring(functionCall.Length - 1, 1));
                    currentParameterCount++;

                    functionCall = functionCall.Substring(0, functionCall.Length - 1) + currentParameterCount;

                    operators.Push(functionCall);

                    while (tempStack.Count > 0)
                        operators.Push(tempStack.Pop());
                }
                else if (operatorMatcher.IsMatch(getCurrentSequence()))
                {
                    while (operators.Count > 0 && operatorLowerPrecedence(operators.Peek().ToString(), getCurrentSequence()))
                        output.Enqueue(operators.Pop());

                    operatorDetail currentDetail = (operatorDetail)operatorDetail[getCurrentSequence()];

                    if (currentDetail != null)
                    {
                        //if (operators.Count > 0 && operators.Peek().ToString() == "(" && currentDetail.vmCommand == "sub")
                        //    operators.Push("neg");
                        //else
                        operators.Push(currentDetail.vmCommand);
                    }
                    else
                        Console.WriteLine("no detail for " + getCurrentSequence());
                }
                else if (getCurrentSequence() == "(")
                {
                    //Console.Write("opening paren");
                    operators.Push("(");
                }
                else if (getCurrentSequence() == ")")
                {
                    //Console.Write("closing paren");

                    while (operators.Count > 0 && operators.Peek().ToString() != "(")
                        output.Enqueue(operators.Pop());

                    if (operators.Count > 0)
                        operators.Pop();

                    //if (operators.Count > 0 && new Regex("^(" + Tokenizer.functionRegex + ")").IsMatch(operators.Peek().ToString()))
                    if (operators.Count > 0 && operators.Peek().ToString().Length >= 4 && operators.Peek().ToString().Substring(0, 4) == "call")
                    {
                        //Console.WriteLine("found function");
                        output.Enqueue(operators.Pop());
                    }
                    //else if (operators.Count > 0)
                    //{
                    //    Console.WriteLine("not function: " + operators.Peek().ToString());
                    //}
                }
                else if (getCurrentSequence().Length >= 1 && getCurrentSequence().Substring(0, 1) == @"""")
                    CompileString();

                //else
                //    Console.WriteLine("no match - " + getCurrentSequence());
                
                advanceToken();
            }

        }

        private void CompileString()
        {
            String target = getCurrentSequence();
            Char[] stringChars = target.ToCharArray();

            sw.WriteLine("push constant " + (stringChars.Length - 2));
            sw.WriteLine("call String.new 1");

            for (int i = 1; i < stringChars.Length - 1; i++ )
            {
                sw.WriteLine("push constant " + Convert.ToInt32(stringChars[i]));
                sw.WriteLine("call String.appendChar 2");

            }
       //         Console.WriteLine(stringChars[i]);
        
        }
  
public void advanceString(ref String expression)
        {
            expression = expression.Substring(1);
        }
  
        public void codeWrite(String expression)
        {
            //Console.WriteLine("curent expression - " + expression);

            if (expression.Trim() == "")
                return;

            if (new Regex("^" + Tokenizer.intRegex + "$").IsMatch(expression))
                sw.WriteLine("push constant " + expression);
            else if (new Regex("^" + Tokenizer.stringRegex + "$").IsMatch(expression))
                sw.WriteLine("push representation of string: " + expression);
            else if (expression == "true")
            {
                sw.WriteLine("push constant 0");
                sw.WriteLine("not");
            }
            else if (expression == "false")
                sw.WriteLine("push constant 0");
            else if (expression == "~")
                sw.WriteLine("not");
            else if (new Regex("^(" + Tokenizer.operatorRegex + ")?$").IsMatch(expression))
            {
                operatorDetail currentDetail = (operatorDetail)operatorDetail[expression];
                sw.WriteLine(currentDetail.vmCommand);
            }
            else if (new Regex("^" + Tokenizer.identifierRegex + @"(\[.*\])?$").IsMatch(expression))
            {
                String bracketExpression = "";
                Regex arrayMatcher = new Regex(@"(" + Tokenizer.identifierRegex + @")\[(.*)\]");
                Match arrayMatched;

                arrayMatched = arrayMatcher.Match(expression);

                if (arrayMatched.Groups.Count > 1)
                {
                    sw.WriteLine("push " + GetVariableReference(arrayMatched.Groups[1].ToString()));
                    bracketExpression = arrayMatched.Groups[2].ToString();
                    codeWrite(arrayMatched.Groups[2].ToString());
                    sw.WriteLine("add");
                    sw.WriteLine("pop pointer 1");
                    sw.WriteLine("push that 0");
                }
                else
                    sw.WriteLine("push " + GetVariableReference(expression));
            }
            else
            {
                Regex opmatcher = new Regex("^(" + Tokenizer.operatorRegex + ")(.*)");
                Match opmatched;

                opmatched = opmatcher.Match(expression);

                if (opmatched.Groups.Count > 2 && opmatched.Groups[2].ToString().Trim() != "")
                {
                    codeWrite(opmatched.Groups[2].ToString());
                    codeWrite(opmatched.Groups[1].ToString());
                }
                else
                {
                    Regex functionmatcher = new Regex("^(" + Tokenizer.functionRegex + @")\((.*)\)");
                    //Regex functionmatcher = new Regex(@"^(([A-Za-z]+[A-Za-z0-9_]*\.)?([A-Za-z]+[A-Za-z0-9_]))\((.*)\)$");
                    Match functionmatched;

                    functionmatched = functionmatcher.Match(expression);

                    for (int i = 0; i < functionmatched.Groups.Count; i++)
                        ;// Console.WriteLine(i + " - " + functionmatched.Groups[i].ToString());

                    if (functionmatched.Groups.Count > 1)
                    {
                        int parameterCount = 0;

                        if (functionmatched.Groups[5].ToString() != "")
                        {
                            String[] parameters = functionmatched.Groups[5].ToString().Split(',');

                            foreach (String currentExpression in parameters)
                            {
                                codeWrite(currentExpression);
                                parameterCount++;
                            }
                        }

                        String[] currentFunction = functionmatched.Groups[1].ToString().Split('.');
                        String instanceVariable = "", functionName = "";

                        functionName = functionmatched.Groups[1].ToString();

                        if (currentFunction.Length > 1)
                        {
                            instanceVariable = GetVariableReference(currentFunction[0]);

                            if (instanceVariable != currentFunction[0] + " not found")
                            {
                                sw.WriteLine("push " + instanceVariable);
                                functionName = GetVariableReference(currentFunction[0], true) + "." + currentFunction[1];
                                parameterCount++;
                            }
                        }

                        if (!functionName.Contains("."))
                        {
                            sw.WriteLine("push pointer 0");
                            parameterCount += 1;
                            functionName = currentClassName + "." + functionName;
                        }

                        sw.WriteLine("call " + functionName + " " + parameterCount);
                    }
                    else
                    {
                        Regex multiFunctionmatcher = new Regex("(.*)(" + Tokenizer.operatorRegex + ")(.*)");
                        Match multiFunctionmatched;

                        multiFunctionmatched = multiFunctionmatcher.Match(expression);

                        if (multiFunctionmatched.Groups.Count > 1)
                        {
                            //Write left half of expression
                            codeWrite(multiFunctionmatched.Groups[1].ToString().Trim());

                            //Write right half of expression
                            codeWrite(multiFunctionmatched.Groups[3].ToString().Trim());

                            //Write operator to operate on expressed halves
                            operatorDetail currentDetail = (operatorDetail)operatorDetail[multiFunctionmatched.Groups[2].ToString()];
                            sw.WriteLine(currentDetail.vmCommand);


                        }
                        else
                            sw.WriteLine("no match found in codewriter for " + expression);
                    }
                }
            }
        }

        public void Compile()
        {
            //codeWrite(@"Keyboard.readInt(argument1,argument2) + 2");
            //codeWrite("12345;".Replace(";",""));
            //codeWrite("nBalance");
            //codeWrite(@"readInt(""HOW MANY NUMBERS? "")");
            //codeWrite("-nBalance");
            //return;
            
            currentToken = tokens.First;

            while (currentToken != null && currentToken.Next != null)
            {
                if (currentToken.Value.sequence == "class")
                {
                    advanceToken();
                    currentClassName = currentToken.Value.sequence;
                    currentFunctionName = "";
                    advanceToken(2);
                    CompileClass();
                }
                else
                    advanceToken();
            }

            //writeVariables();
            
        }

        private void writeVariables()
        {
            int counter = 0;
            foreach (Hashtable currentTable in variablemaps)
            {
                Console.WriteLine(counter);
                counter++;

                foreach (string key in currentTable.Keys)
                {
                    VariableObject blah = (VariableObject)currentTable[key];
                    Console.WriteLine(String.Format("{0}: {1}", key, blah.Kind + " - " + blah.Type + " - " + blah.Number));
                }
            }

        }

        private void advanceToken(int advancecount = 1)
        {
            for (int i = 0; i < advancecount; i++)
                currentToken = currentToken.Next;
        }

        public void CompileClass()
        {
            variablemaps = new LinkedList<Hashtable>();
            CompileClassVarDec();
            CompileSubroutines();
        }
        
        private int CompileClassVarDec(Hashtable passedVariables = null)
        {
            String currentKind,currentType;
            int staticCount = 0, fieldCount = 0, varCount = 0;
            Hashtable variables;

            if (passedVariables != null)
                variables = passedVariables;
            else
                variables = new Hashtable();

            VariableObject currentVariable;

            while (getCurrentSequence() == "static" || getCurrentSequence() == "field" || getCurrentSequence() == "var")
            {
                currentKind = getCurrentSequence();

                advanceToken();
                currentType = getCurrentSequence();
                advanceToken();

                currentVariable = new VariableObject();
                currentVariable.Kind = currentKind;
                currentVariable.Type = currentType;
                currentVariable.ParentType = (currentFunctionName == "" ? @"class" : @"function");

                switch (currentKind)
                {
                    case "static":
                        currentVariable.Number = staticCount++;
                        break;
                    case "field":
                        currentVariable.Number = fieldCount++;
                        break;
                    case "var":
                        currentVariable.Number = varCount++;
                        break;
                }

    
                variables.Add(getCurrentSequence(), currentVariable);
                //Console.WriteLine("added " + getCurrentSequence());
                                
                while (currentToken.Next.Value.sequence == ",")
                {
                    currentVariable = new VariableObject();
                    currentVariable.Kind = currentKind;
                    currentVariable.Type = currentType;
                    currentVariable.ParentType = (currentFunctionName == "" ? @"class" : @"function");

                    switch (currentKind)
                    {
                        case "static":
                            currentVariable.Number = staticCount++;
                            break;
                        case "field":
                            currentVariable.Number = fieldCount++;
                            break;
                        case "var":
                            currentVariable.Number = varCount++;
                            break;
                    }

    
                    advanceToken(2);
                        
                    variables.Add(getCurrentSequence(), currentVariable);
                    //Console.WriteLine("added " + getCurrentSequence());
                }

                advanceToken(2);
            }

            if (variables.Count > 0)
                variablemaps.AddLast(variables);

            return varCount;
        }

        private String getCurrentSequence()
        {
            return currentToken.Value.sequence;
        }

        private void CompileSubroutines()
        {
            Hashtable arguments;
            String currentFunctionType = "", currentReturnType = "";
            int localVariableCount = 0, classVariables = 0;

            while (getCurrentSequence() == "method" || getCurrentSequence() == "function" || getCurrentSequence() == "constructor")
            {
                ifCounter = 0;
                whileCounter = 0;

                currentFunctionType = getCurrentSequence();
                advanceToken();
                currentReturnType = getCurrentSequence();
                advanceToken();
                currentFunctionName = getCurrentSequence();
                advanceToken(2);

                //How can I guarantee the last variable map has class variables?  What if there are no class variables?
                Boolean foundClass = false;
                VariableObject currentVariable;
                LinkedListNode<Hashtable> currentMap;

                if (variablemaps.Count > 0)
                {
                    currentMap = variablemaps.Last;

                    while (!foundClass)
                    {
                        foreach (DictionaryEntry entry in currentMap.Value)
                        {
                            currentVariable = (VariableObject)entry.Value;

                            if (currentVariable.ParentType == "class")
                            {
                                foundClass = true;

                                if (currentVariable.Kind != "static")
                                    classVariables++;
                            }
                        }

                        if (currentMap.Previous == null)
                            foundClass = true;

                        if (!foundClass)
                            currentMap = currentMap.Previous;
                    }

                }
                
                arguments = new Hashtable();

                CompileParameterList(arguments, currentFunctionType);
                advanceToken(2);
                
                localVariableCount = CompileClassVarDec(arguments);

                sw.WriteLine("function " + currentClassName + "." + currentFunctionName + " " + localVariableCount);

                if (currentFunctionType == "constructor")
                {
                    sw.WriteLine("push constant " + classVariables);
                    sw.WriteLine("call Memory.alloc 1");
                    sw.WriteLine("pop pointer 0");
                }
                else if (currentFunctionType == "method")
                {
                    sw.WriteLine("push argument 0");
                    sw.WriteLine("pop pointer 0");
                }

                CompileStatements("}");
            }
        }

        private void CompileStatements(String endToken = "return")
        {
            
            while (getCurrentSequence() != endToken && currentToken.Next != null)
            {
                if (getCurrentSequence() == "do")
                    CompileDo();
                else if (getCurrentSequence() == "let")
                    CompileLet();
                else if (getCurrentSequence() == "while")
                    CompileWhile();
                else if (getCurrentSequence() == "if")
                    CompileIf();
                else if (getCurrentSequence() == "return")
                {
                    CompileReturn();
                }
                else
                    advanceToken();
            }

            advanceToken();
        }

        private void CompileDo()
        {
            advanceToken();

            StartParse();
            WriteParseResult();

            sw.WriteLine("pop temp 0");
        }

        private void WriteParseResult()
        {
            String outputString = "";

            while (operators.Count > 0)
            {
                if (operators.Peek().ToString() == "(" || operators.Peek().ToString() == ")")
                {
                    Console.WriteLine("mismatched parentheses");
                    operators.Pop();
                }
                else
                    output.Enqueue(operators.Pop());
            }

            while (output.Count > 0)
            {
                outputString = output.Dequeue().ToString();
                //Console.WriteLine("output - " + outputString);
                sw.WriteLine(outputString);
            }

            while (operators.Count > 0)
                Console.WriteLine("operators - " + operators.Pop().ToString());

        }

        private void CompileLet()
        {
            String variableReference;

            advanceToken();
            
            variableReference = GetVariableReference(getCurrentSequence());
            
            if (currentToken.Next.Value.sequence == "[")
            {
                advanceToken(2);
                operators.Push("[");
                StartParse("]");
                
                //Console.WriteLine("after parsing " + variableReference);


                while (operators.Count > 0 && operators.Peek().ToString() != "[")
                {
                    //Console.WriteLine(operators.Peek().ToString());
                    output.Enqueue(operators.Pop());
                }

                if (operators.Count > 0)
                {
                    //Console.WriteLine("Deleting " + operators.Peek().ToString());
                    operators.Pop();
                }
            

                WriteParseResult();

                sw.WriteLine("push " + variableReference);
                sw.WriteLine("add");

                advanceToken(2);

                StartParse(";");

                WriteParseResult();
                
                sw.WriteLine("pop temp 0");
                sw.WriteLine("pop pointer 1");
                sw.WriteLine("push temp 0");
                sw.WriteLine("pop that 0");
            }
            else
            {
                while (getCurrentSequence() != "=")
                    advanceToken();

                advanceToken();

                StartParse(";");

                WriteParseResult();

                sw.WriteLine("pop " + variableReference);
            }

                
            return;

        }

        private String GetVariableReference(String variableName, Boolean getType = false)
        {
            LinkedListNode<Hashtable> currentVariableMap = variablemaps.Last;

            if (variableName == "this")
                return "pointer 0";
           
            while (currentVariableMap != null)
            {
                foreach (String currentkey in currentVariableMap.Value.Keys)
                {
                    if (currentkey == variableName)
                    {
                            VariableObject foundVariable = (VariableObject)currentVariableMap.Value[currentkey];

                            if (getType)
                                return foundVariable.Type;
                            else
                                return updateKind(foundVariable.Kind) + " " + foundVariable.Number;// +" (aka " + variableName + ")";
                    }
                }

                currentVariableMap = currentVariableMap.Previous;
            }

            return variableName + " not found";
        }

        private String updateKind(String kind)
        {
            if (kind == "var")
                kind = "local";
            else if (kind == "field")
                kind = "this";

            return kind;
        }

        private void CompileWhile()
        {
            int currentWhile;
            advanceToken();
            
            currentWhile = whileCounter;
            whileCounter++;

            sw.WriteLine("label WHILE_EXP" + currentWhile);

StartParse("{");
            WriteParseResult();
            
            sw.WriteLine("not");
            sw.WriteLine("if-goto WHILE_END" + currentWhile);
          
            advanceToken();

                CompileStatements("}");
            
            sw.WriteLine("goto WHILE_EXP" + currentWhile);
            sw.WriteLine("label WHILE_END" + currentWhile);

        }

        private void CompileIf()
        {
            String statement1 = "", bracketExpression = "";
            int currentIf = ifCounter;

            ifCounter++;

            advanceToken();

            StartParse("{");
            WriteParseResult();

            //while (getCurrentSequence() != ")")
            //{
            //    bracketExpression += getCurrentSequence();
            //    advanceToken();
            //}

            //codeWrite(bracketExpression);
            sw.WriteLine("if-goto IF_TRUE" + currentIf);
            sw.WriteLine("goto IF_FALSE" + currentIf);
            sw.WriteLine("label IF_TRUE" + currentIf);
            
            advanceToken();
            
            CompileStatements("}");

            
            if (getCurrentSequence() == "else")
            {
                sw.WriteLine("goto IF_END" + currentIf);
                sw.WriteLine("label IF_FALSE" + currentIf);
                
                advanceToken();

                if (getCurrentSequence() == "{")
                {
                    advanceToken();
                    CompileStatements("}");
                }
                else
                    CompileStatements(";");

                sw.WriteLine("label IF_END" + currentIf);
            }
            else
                sw.WriteLine("label IF_FALSE" + currentIf);
           

            
            
        }

        private void CompileReturn()
        {
            
            
            //String returnExpression = "";

            //while (getCurrentSequence() != ";") 
            //{
            //    returnExpression += getCurrentSequence();
            //    advanceToken();
            //}

            //if (returnExpression != "")
            //    codeWrite(returnExpression);
            //else
            //{
            //    sw.WriteLine("push constant 0");
            //}

            Console.WriteLine(getCurrentSequence());

            if (getCurrentSequence().ToLower() == "return")
                advanceToken();
              
            if (getCurrentSequence() == ";")
                sw.WriteLine("push constant 0");
            else
            {
                StartParse(";");
                WriteParseResult();
            }

            sw.WriteLine("return");
        }

        //private void CompileExpression(String terminator = ";")
        //{
        //    Stack operators = new Stack();
        //    Queue output = new Queue();

        //    while (getCurrentSequence() != terminator)
        //    {
        //        //Console.WriteLine(getCurrentSequence());
        //        if (new Regex(@"\+|\-|\*|/|&|\||<\>\=").IsMatch(getCurrentSequence()))
        //        {
        //            while (operators.Count > 0 && operatorLowerPrecedence(operators.Peek().ToString()))
        //                output.Enqueue(operators.Pop());

        //            operators.Push(getCurrentSequence());
        //            advanceToken();
        //        }
        //        else if (new Regex(@"[0-9]+").IsMatch(getCurrentSequence()))
        //        {
        //            output.Enqueue(getCurrentSequence());
        //            advanceToken();
        //        }
        //        else if (new Regex(@"").IsMatch(getCurrentSequence() + currentToken.Next.Value.sequence + currentToken.Next.Next.Value.sequence))
        //            ;
        //        else
        //            advanceToken();
        //        //    CompileTerm();
        //    }

            

        //    while (output.Count > 0)
        //        Console.WriteLine(output.Dequeue().ToString());


        //    while (operators.Count > 0)
        //        Console.WriteLine(operators.Pop().ToString());
        //}

        private Boolean operatorLowerPrecedence(String currentOperator, String checkOperator)
        {
            if (operatorDetail.ContainsKey(currentOperator) && operatorDetail.ContainsKey(checkOperator))
            {
                operatorDetail o1, o2;

                o1 = (operatorDetail)operatorDetail[checkOperator];
                o2 = (operatorDetail)operatorDetail[currentOperator];

                return (o1.associativity == "left" && o1.precedence <= o2.precedence) || (o1.associativity == "right" && o1.precedence < o2.precedence);
            }

            return false;
        }

        private void CompileTerm()
        {

        }

        private void CompileOp()
        {

        }

        private void CompileExpressionList()
        {

        }

        private void CompileParameterList(Hashtable arguments, String functionType)
        {
            int argumentCount = 0;
            VariableObject currentVariable;

            if (variablemaps.Count > 0)// && (String)variablemaps.Last.Value[0] == "")
            {
                foreach (DictionaryEntry entry in variablemaps.Last.Value)
                {
                    currentVariable = (VariableObject)entry.Value;

                    if (currentVariable.ParentType == "function")
                    {
                        variablemaps.RemoveLast();
                        break;
                    }
                }
            }
            
            if (functionType == "method")
            {
                currentVariable = new VariableObject();
                currentVariable.Kind = "argument";
                currentVariable.Type = currentClassName;
                currentVariable.Number = argumentCount++;
                currentVariable.ParentType = "function";
                arguments.Add("this", currentVariable);
            }
            
            if (getCurrentSequence() == "(")
                advanceToken();

            while (getCurrentSequence() != ")")
            {
                currentVariable = new VariableObject();
                currentVariable.Kind = "argument";
                currentVariable.Type = getCurrentSequence();
                currentVariable.Number = argumentCount++;
                currentVariable.ParentType = "function";
                //Console.WriteLine(getCurrentSequence());
                advanceToken();

                //Console.WriteLine(getCurrentSequence());
                
                arguments.Add(getCurrentSequence(), currentVariable);

                if (currentToken.Next.Value.sequence == ",")
                    advanceToken(2);
                else
                    advanceToken();
            }

            
        }

        private void CompileVarDec()
        {

        }
    }


}
