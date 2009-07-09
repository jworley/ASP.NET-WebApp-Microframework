using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JawTek.Web.Utility.HTML;

namespace JawTek.Web.Utility.Javascript.Tools
{
    public class Javascriptformatter
    {
        #region Private Members
        private int pos = 0;
        private string input = String.Empty;
        private bool preserveNewLines = false;
        private string indentString = String.Empty;
        private int indentLevel = 0;
        private Stack<string> output = new Stack<string>();
        private TokenStruct lastToken = new TokenStruct();
        private string lastWord = string.Empty;
        private bool ifFlag = false;
        private bool doJustClosed = false;
        private bool varLine = false;
        private bool varLineTainted = false;
        private bool inCase = false;
        private TokenStruct currentToken = new TokenStruct();
        private Mode currentMode = Mode.BLOCK;
        private Stack<Mode> modes = new Stack<Mode>();
        private readonly string[] punct = "+ - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ! !! , : ? ^ ^= |= ::".Split(' ');
        private readonly string[] lineStarters = "continue,try,throw,return,var,if,switch,case,default,for,while,break,function".Split(',');
        #endregion

        #region Enums and Struct
        private enum Token
        {
            EOF,
            WORD,
            OPERATOR,
            START_EXPR,
            END_EXPR,
            START_BLOCK,
            END_BLOCK,
            SEMICOLON,
            BLOCK_COMMENT,
            COMMENT,
            STRING,
            UNKNOWN
        }

        private enum Mode
        {
            BLOCK,
            DO_BLOCK,
            EXPRESSION
        }

        private enum Prefix
        {
            NONE,
            NEWLINE,
            SPACE
        }

        private struct TokenStruct
        {
            public Token tokenType;
            public string tokenText;

            public TokenStruct(Token tokenType, string tokenText)
            {
                this.tokenType = tokenType;
                this.tokenText = tokenText;
            }
        }
        #endregion

        #region Public Methods
        public string Format(string jsScript)
        {
            indentString = HTMLUtility.IndentString;
            input = jsScript;

            lastWord = string.Empty;
            lastToken = new TokenStruct(Token.START_EXPR, string.Empty);
            output = new Stack<string>();
            doJustClosed = false;
            varLine = false;
            varLineTainted = false;
            inCase = false;
            currentToken = new TokenStruct();

            modes = new Stack<Mode>();
            setMode(Mode.BLOCK);

            pos = 0;
            while (true)
            {
                currentToken = getNextToken();
                var t = currentToken;
                if (t.tokenType == Token.EOF)
                    break;
                switch (t.tokenType)
                {
                    case Token.WORD:
                        var arr = new string[] { "else", "catch", "finally" };
                        if (doJustClosed)
                        {
                            printSpace();
                            printToken();
                            printSpace();
                            doJustClosed = false;
                            break;
                        }

                        if (t.tokenText == "case" || t.tokenText == "default")
                        {
                            if (lastToken.tokenText == ":")
                            {
                                removeIndent();
                            }
                            else
                            {
                                unindent();
                                printNewLine();
                                indent();
                            }
                            printToken();
                            inCase = true;
                            break;
                        }
                        Prefix prefix = Prefix.NONE;
                        if (lastToken.tokenType == Token.END_BLOCK)
                        {
                            if (!(arr).Contains(t.tokenText.ToLower()))
                            {
                                prefix = Prefix.NEWLINE;
                            }
                            else
                            {
                                prefix = Prefix.SPACE;
                                printSpace();
                            }
                        }
                        else if (lastToken.tokenType == Token.SEMICOLON && (currentMode == Mode.BLOCK || currentMode == Mode.DO_BLOCK))
                        {
                            prefix = Prefix.NEWLINE;
                        }
                        else if (lastToken.tokenType == Token.SEMICOLON && currentMode == Mode.EXPRESSION)
                        {
                            prefix = Prefix.SPACE;
                        }
                        else if (lastToken.tokenType == Token.STRING)
                        {
                            prefix = Prefix.NEWLINE;
                        }
                        else if (lastToken.tokenType == Token.WORD)
                        {
                            prefix = Prefix.SPACE;
                        }
                        else if (lastToken.tokenType == Token.START_BLOCK)
                        {
                            prefix = Prefix.NEWLINE;
                        }
                        else if (lastToken.tokenType == Token.END_EXPR)
                        {
                            printSpace();
                            prefix = Prefix.NEWLINE;
                        }

                        if (lastToken.tokenType != Token.END_BLOCK && arr.Contains(t.tokenText.ToLower()))
                        {
                            printNewLine();
                        }
                        else if (lineStarters.Contains(t.tokenText) || prefix == Prefix.NEWLINE)
                        {
                            if (lastToken.tokenText == "else")
                            {
                                printSpace();
                            }
                            else if ((lastToken.tokenType == Token.START_EXPR || lastToken.tokenText == "=" ||
                                lastToken.tokenText == ",") && t.tokenText == "function")
                            {

                            }
                            else if (lastToken.tokenType == Token.WORD && (lastToken.tokenText == "return" || lastToken.tokenText == "throw"))
                            {
                                printSpace();
                            }
                            else if (lastToken.tokenType != Token.END_EXPR)
                            {
                                if ((lastToken.tokenType != Token.START_EXPR || t.tokenText != "var") &&
                                    lastToken.tokenText != ":")
                                {
                                    if (t.tokenText == "if" && lastToken.tokenType == Token.WORD
                                        && lastWord == "else")
                                    {
                                        printSpace();
                                    }
                                    else
                                    {
                                        printNewLine();
                                    }
                                }
                            }
                            else
                            {
                                if (lineStarters.Contains(t.tokenText) && lastToken.tokenText != ")")
                                {
                                    printNewLine();
                                }
                            }
                        }
                        else if (prefix == Prefix.SPACE)
                        {
                            printSpace();
                        }
                        printToken();
                        lastWord = t.tokenText;

                        if (t.tokenText == "var")
                        {
                            varLine = true;
                            varLineTainted = false;
                        }
                        if (t.tokenText == "if" || t.tokenText == "else")
                        {
                            ifFlag = true;
                        }

                        break;
                    case Token.OPERATOR:
                        bool startDelim = true;
                        bool endDelim = true;
                        if (varLine && t.tokenText != ",")
                        {
                            varLineTainted = true;
                            if (t.tokenText == ":")
                            {
                                varLine = false;
                            }
                        }
                        if (varLine && t.tokenText == "," && currentMode == Mode.EXPRESSION)
                        {
                            varLineTainted = false;
                        }
                        if (t.tokenText == ":" && inCase)
                        {
                            printToken();
                            printNewLine();
                            break;
                        }
                        if (t.tokenText == "::")
                        {
                            printToken();
                            break;
                        }

                        inCase = false;

                        if (t.tokenText == ",")
                        {
                            if (varLine)
                            {
                                if (varLineTainted)
                                {
                                    printToken();
                                    printNewLine();
                                    varLineTainted = false;
                                }
                                else
                                {
                                    printToken();
                                    printSpace();
                                }
                            }
                            else if (lastToken.tokenType == Token.END_BLOCK)
                            {
                                printToken();
                                printNewLine();
                            }
                            else
                            {
                                if (currentMode == Mode.BLOCK)
                                {
                                    printToken();
                                    printNewLine();
                                }
                                else
                                {
                                    printToken();
                                    printSpace();
                                }
                            }
                            break;
                        }
                        else if (t.tokenText == "--" || t.tokenText == "++")
                        {
                            if (lastToken.tokenText == ";")
                            {
                                startDelim = true;
                                endDelim = false;
                            }
                            else
                            {
                                startDelim = endDelim = false;
                            }
                        }else if ( t.tokenText == "!" && lastToken.tokenType == Token.START_EXPR)
                        {
                            startDelim = endDelim = false;
                        }
                        else if (lastToken.tokenType == Token.OPERATOR)
                        {
                            startDelim = endDelim = false;
                        }
                        else if (lastToken.tokenType == Token.END_EXPR)
                        {
                            startDelim = endDelim = true;
                        }
                        else if (t.tokenText == ".")
                        {
                            startDelim = endDelim = false;
                        }
                        else if (t.tokenText == ":")
                        {
                            startDelim = Regex.IsMatch(lastToken.tokenText, @"^\d+?");
                        }
                        if(startDelim)
                            printSpace();
                        printToken();
                        if(endDelim)
                            printSpace();
                        break;
                    case Token.START_EXPR:
                        varLine = false;
                        setMode(Mode.EXPRESSION);
                        if (lastToken.tokenText == ";")
                        {
                            printNewLine();
                        }
                        else if (lastToken.tokenType == Token.END_EXPR || lastToken.tokenType == Token.START_EXPR)
                        {
                        }
                        else if (lastToken.tokenType != Token.WORD && lastToken.tokenType != Token.OPERATOR)
                        {
                            printSpace();
                        }
                        else if (lineStarters.Contains(lastWord) && lastWord != "function")
                        {
                            printSpace();
                        }
                        printToken();
                        break;
                    case Token.END_EXPR:
                        printToken();
                        restoreMode();
                        break;
                    case Token.START_BLOCK:
                        if (lastWord == "do")
                            setMode(Mode.DO_BLOCK);
                        else
                            setMode(Mode.BLOCK);
                        if (lastToken.tokenType != Token.OPERATOR && lastToken.tokenType != Token.START_EXPR)
                        {
                            if (lastToken.tokenType == Token.START_BLOCK)
                                printNewLine();
                            else
                                printSpace();
                        }
                        printToken();
                        indent();
                        break;
                    case Token.END_BLOCK:
                        if (lastToken.tokenType == Token.START_BLOCK)
                        {
                            trimOutput();
                            unindent();
                        }
                        else {
                            unindent();
                            printNewLine();
                        }
                        printToken();
                        restoreMode();
                        break;
                    case Token.SEMICOLON:
                        printToken();
                        varLine = false;
                        break;
                    case Token.BLOCK_COMMENT:
                        printNewLine();
                        printToken();
                        printNewLine();
                        break;
                    case Token.COMMENT:
                        printSpace();
                        printToken();
                        printNewLine();
                        break;
                    case Token.STRING:
                        if (lastToken.tokenType == Token.START_BLOCK ||
                            lastToken.tokenType == Token.END_BLOCK ||
                            lastToken.tokenType == Token.SEMICOLON)
                        {
                            printNewLine();
                        }
                        else if (lastToken.tokenType == Token.WORD)
                        {
                            printSpace();
                        }
                        printToken();
                        break;
                    case Token.UNKNOWN:
                        printToken();
                        break;
                    default:
                        break;
                }
                lastToken = currentToken;
            }
            string r = output.ToArray().Aggregate("",(a,s) => s+a);
            r = Regex.Replace(r, @"\n+$", "");
            return r;
        }
        #endregion

        #region Private Methods
        private void removeIndent()
        {
            if (output.Peek() == indentString)
                output.Pop();
        }

        private void indent()
        {
            indentLevel++;
        }
        private void unindent()
        {
            if (indentLevel > 0)
                indentLevel--;
        }

        private void printToken()
        {
            output.Push(currentToken.tokenText);
        }

        private void printSpace()
        {
            string lastOutput = " ";
            if (output.Count() > 0)
                lastOutput = output.Peek();
            if (lastOutput != " " && lastOutput != "\n" && lastOutput != indentString)
            {
                output.Push(" ");
            }
        }

        private void trimOutput()
        {
            while ((output.Count() > 0) && ((output.Peek() == " ") || (output.Peek() == indentString)))
            {
                output.Pop();
            }
        }

        private void setMode(Mode mode)
        {
            modes.Push(mode);
            currentMode = mode;
        }

        private void restoreMode()
        {
            doJustClosed = currentMode == Mode.DO_BLOCK;
            modes.Pop();
            currentMode = modes.Peek();
        }

        private void printNewLine()
        {
            this.printNewLine(true);
        }
        private void printNewLine(bool ignoreRepeated)
        {
            ifFlag = false;
            trimOutput();

            if (output.Count() == 0)
                return;

            if ((output.Peek() != "\n") || (ignoreRepeated))
                output.Push("\n");
            for (int i = 0; i < indentLevel; i++)
                output.Push(indentString);
        }

        private TokenStruct getNextToken()
        {
            int newLines = 0;

            if (pos >= input.Length)
                return new TokenStruct(Token.EOF, "");

            string c = input[pos].ToString();
            pos++;

            while (Regex.IsMatch(c, @"[\n\r\t ]"))
            {
                if (pos >= input.Length)
                    return new TokenStruct(Token.EOF, "");
                if (c == "\n")
                    newLines++;

                c = input[pos].ToString();
                pos++;
            }

            bool wantedNewline = false;

            if (this.preserveNewLines)
            {
                if (newLines > 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        printNewLine(i == 0);
                    }
                }
                wantedNewline = (newLines == 1);
            }

            //Token.WORD
            if (Regex.IsMatch(c, @"[a-zA-Z_0-9\$]"))
            {
                if (pos < input.Length)
                {
                    while (Regex.IsMatch(input[pos].ToString(), @"[a-zA-Z_0-9\$]"))
                    {
                        c += input[pos].ToString();
                        pos++;
                        if (pos == input.Length)
                            break;
                    }
                }

                if ((pos != input.Length) && (Regex.IsMatch(c, "^[0-9]+[Ee]$"))
                    && ((input[pos] == '-') || (input[pos] == '+')))
                {
                    string sign = input[pos].ToString();
                    pos++;

                    TokenStruct t = this.getNextToken();
                    c += sign + t.tokenText;

                    return new TokenStruct(Token.WORD, c);
                }

                if (c == "in")
                {
                    return new TokenStruct(Token.OPERATOR, c);
                }

                if (wantedNewline && lastToken.tokenType != Token.OPERATOR && !ifFlag)
                {
                    printNewLine();
                }
                return new TokenStruct(Token.WORD, c);
            }

            if (c == "(" || c == "[")
                return new TokenStruct(Token.START_EXPR, c);
            if (c == ")" || c == "]")
                return new TokenStruct(Token.END_EXPR, c);
            if (c == "{")
                return new TokenStruct(Token.START_BLOCK, c);
            if (c == "}")
                return new TokenStruct(Token.END_BLOCK, c);
            if (c == ";")
                return new TokenStruct(Token.SEMICOLON, c);

            if (c == "/")
            {
                string comment = String.Empty;
                if (input[pos] == '*')
                {
                    pos++;
                    if (pos < input.Length)
                    {
                        while (!(input[pos] == '*' && input[pos + 1] == '/') &&
                            pos < input.Length)
                        {
                            comment += input[pos].ToString();
                            pos++;
                            if (pos >= input.Length)
                                break;
                        }
                    }
                    pos += 2;
                    return new TokenStruct(Token.BLOCK_COMMENT, "/*" + comment + "*/");
                }

                if (input[pos] == '/')
                {
                    comment = c;
                    while (input[pos] != '\x0d' && input[pos] != '\x0a')
                    {
                        comment += input[pos].ToString();
                        pos++;
                        if (pos >= input.Length)
                            break;
                    }
                    pos++;
                    if (wantedNewline)
                        printNewLine();
                    return new TokenStruct(Token.COMMENT, comment);
                }
            }

            if (c == "'" ||
                c == "\"" ||
                (c == "/" &&
                ((lastToken.tokenType == Token.WORD && lastToken.tokenText == "return") ||
                 (lastToken.tokenType == Token.START_EXPR ||
                  lastToken.tokenType == Token.START_BLOCK ||
                  lastToken.tokenType == Token.END_BLOCK ||
                  lastToken.tokenType == Token.OPERATOR ||
                  lastToken.tokenType == Token.EOF ||
                  lastToken.tokenType == Token.SEMICOLON))))
            {
                string sep = c;
                bool esc = false;
                string result = c;

                if (pos < input.Length)
                {
                    while (esc || input[pos].ToString() != sep)
                    {
                        result += input[pos].ToString();
                        if (!esc)
                        {
                            esc = input[pos] == '\\';
                        }
                        else
                        {
                            esc = false;
                        }
                        pos++;
                        if (pos >= input.Length)
                        {
                            return new TokenStruct(Token.STRING, result);
                        }
                    }
                }
                pos++;

                result += sep;

                if (sep == "/")
                {
                    while (pos < input.Length && Regex.IsMatch(input[pos].ToString(), @"[a-zA-Z_0-9\$]"))
                    {
                        result += input[pos].ToString();
                        pos++;
                    }
                }
                return new TokenStruct(Token.STRING, result);
            }

            if (c == "#")
            {
                string sharp = "#";
                if (pos < input.Length && Regex.IsMatch(input[pos].ToString(), @"[0-9]"))
                {
                    do
                    {
                        c = input[pos].ToString();
                        sharp += c;
                        pos++;
                    } while (pos < input.Length && c != "#" && c != "=");
                    if (c == "#")
                        return new TokenStruct(Token.WORD, sharp);
                    else
                        return new TokenStruct(Token.OPERATOR, sharp);
                }
            }
            if (c == "<" && input.Substring(pos - 1, pos + 3) == "<!--")
            {
                pos += 3;
                return new TokenStruct(Token.COMMENT, "<!--");
            }

            if (c == "-" && input.Substring(pos - 1, pos + 2) == "-->")
            {
                pos += 2;
                if (wantedNewline)
                    printNewLine();
                return new TokenStruct(Token.COMMENT, "-->");
            }

            if (punct.Contains(c))
            {
                while (pos < input.Length && punct.Contains(c + input[pos].ToString()))
                {
                    c += input[pos].ToString();
                    pos++;
                    if (pos >= input.Length)
                        break;
                }
                return new TokenStruct(Token.OPERATOR, c);
            }

            return new TokenStruct(Token.UNKNOWN, c);
        } 
        #endregion

        public Javascriptformatter() :this(0) { }
        public Javascriptformatter(int indentLevel)
        {
            this.indentLevel = indentLevel;
        }
    }
}
