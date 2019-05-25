using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JackCompiler
{
    class Program
    {
        public void writeline(String text)
        {
            Console.WriteLine(text);
        }

        static void Main(string[] args)
        {
           
            Compiler compiler;
            Tokenizer tokenizer = new Tokenizer();
            //tokenizer.add(@"(/\*\*.*[\r\n]+)|^( \* .*[\r\n]+)|( \*/.*[\r\n]+)|(//.*[\r]+)", Tokenizer.COMMENT);
//            tokenizer.add(@"/\*\*.*[\r\n]+(^.*\*.*[\r\n]+)+(^.*\*/)", Tokenizer.COMMENT);
         
  //          tokenizer.add(@"(/\*\*.*\*/[\r\n]+)|(//.*[\n\r]+)", Tokenizer.COMMENT);
            tokenizer.add(@"(//.*[\r\n]+)|(?s)/\*\*.*?\*/", Tokenizer.COMMENT); 
            tokenizer.add(Tokenizer.negativeIntRegex, Tokenizer.NEGATIVEINT_CONST);
            tokenizer.add(Tokenizer.symbolRegex, Tokenizer.SYMBOL);
            tokenizer.add(Tokenizer.keywordRegex, Tokenizer.KEYWORD);
            tokenizer.add(Tokenizer.stringRegex, Tokenizer.STRING_CONST);
            tokenizer.add(Tokenizer.intRegex, Tokenizer.INT_CONST);
            tokenizer.add(Tokenizer.identifierRegex, Tokenizer.IDENTIFIER);

            string currentPath = Directory.GetCurrentDirectory();

            string[] fileEntries = Directory.GetFiles(currentPath);

            foreach (string fileName in fileEntries)
                        {
                if (fileName.Substring(fileName.Length - 5, 5) == ".jack")
                {
                    Console.WriteLine(fileName);
                    tokenizer.setFile(fileName);
                    tokenizer.tokenize();

                    compiler = new Compiler(tokenizer.getTokens(), fileName.Replace(".jack", ".vm"));

                    
            //        return;
                    compiler.Compile();
                    //compiler.ParseExpression("a[a[5]] * b[((7 - a[3]) - Main.double(2)) + 1]");
                    //compiler.ParseExpression("b[a[3 + c] + 2]");
                    compiler.sw.Close();




                    //foreach (Token tok in tokenizer.getTokens())
                    //{
                    //    Console.WriteLine("" + tok.token + " " + tok.sequence);
                    //}
                }
            }
        }
    }


}
