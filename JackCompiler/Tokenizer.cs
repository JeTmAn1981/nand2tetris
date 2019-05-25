using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace JackCompiler
{
    public class Tokenizer
{
        private StreamReader sr;
        private string inputString;
        public static string COMMENT = "comment";
        public static string KEYWORD = "keyword";
        public static string SYMBOL = "symbol";
        public static string IDENTIFIER = "identifier";
        public static string INT_CONST = "integer constant";
        public static string NEGATIVEINT_CONST = "negative integer constant";
        public static string STRING_CONST = "string constant";
        public static String identifierRegex = @"[A-Za-z]+[A-Za-z0-9_]*";
        static public String intRegex = @"[0-9]+";
        static public String negativeIntRegex = @"\-[0-9]+";
        static public String symbolRegex = @"{|}|\(|\)|\[|\]|\.|,|;|\+|\-|\*|/|&|\||<|>|=|~";
        static public String operatorRegex = @"\+|\-|\*|\/|&|\||<|>|=|~";
        static public String keywordRegex = @"(class|constructor|function|method|field|static|var|int|char|boolean|void|true|false|null|this|let|do|if|else|while|return)^[A-Za-z0-9]";
        static public String stringRegex = @""".*""";
        //static public String functionRegex = "(" + identifierRegex + @"(\[.*\])?\.)?(" + identifierRegex + @"(\[.*\])?)";
        //static public String functionRegex = "(" + identifierRegex + @"(\[.*\])?\.)?(" + identifierRegex + ")";
        static public String functionRegex = "(" + identifierRegex + @"\.)?(" + identifierRegex + @")\(.*\)";
  private class TokenInfo
  {
    public  Regex regex;
    public  String token;

    public TokenInfo(Regex regex, String token)
    {
      this.regex = regex;
      this.token = token;
    }
  }
  
  
  private LinkedList<TokenInfo> tokenInfos;
  private LinkedList<Token> tokens;
  
  public Tokenizer()
  {
    tokenInfos = new LinkedList<TokenInfo>();
    tokens = new LinkedList<Token>();
  }

  public void setFile(String inputFile)
  {
      sr = new StreamReader(inputFile);
      inputString = sr.ReadToEnd();
  }
  
  public void add(String regex, String token)
  {
    tokenInfos.AddLast(new TokenInfo(new Regex("^("+regex+")"), token));
  }
  
  public void tokenize()
  {
    String s = inputString.Trim();
    String matchedToken = "";

    tokens.Clear();
    
    Boolean match = false;
    
      while (!s.Trim().Equals(""))
      {
          foreach (TokenInfo info in tokenInfos)
          {
            if (info.regex.IsMatch(s))
              {
              //    Console.WriteLine("matched " + info.regex);
                  matchedToken = info.regex.Matches(s)[0].ToString();

                  if (info.token != COMMENT)
                    tokens.AddLast(new Token(info.token, matchedToken));
 
                      s = info.regex.Replace(s, "", 1).Trim();
              //        Console.WriteLine("after replace: " + s);
                  
                  match = true;
                  break;
              }
          }

          if (!match)
              break;

          match = false;

 //         Console.WriteLine("no match on: " + s);
      }
  }
  
  public LinkedList<Token> getTokens()
  {
    return tokens;
  }
  
}


}
