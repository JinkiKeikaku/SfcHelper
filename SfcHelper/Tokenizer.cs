using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    internal class Tokenizer
    {
        public enum TokenKind
        {
            Unknown,
            Eof,
            Identifier,
            Number,
            String,
            Parameter,
            LPar,
            RPar,
            Comma,
            Semicolon,
            Hash,
            Equal,
        }
        public class Token
        {
            public TokenKind Kind;
            public string Value;
            public bool IsEof => Kind == TokenKind.Eof;

            public Token()
            {
                Kind = TokenKind.Eof;
                Value = "";
            }

            public Token(TokenKind kind, string value)
            {
                Kind = kind;
                Value = value;
            }

            public void Set(TokenKind kind, string value)
            {
                Kind = kind;
                Value = value;
            }

            public string GetString() => Value;
            public override string ToString() => $"{Kind}::{GetString()}";
        }

        TextReader mReader;
        Stack<int> mCharStack = new();
        Stack<Token> mTokeenStack = new();

        StringBuilder mSb = new StringBuilder();

        public Token CurrentToken { get; private set; } = new();
        public Tokenizer(TextReader reader)
        {
            mReader = reader;
        }

        public Token GetNextToken()
        {
            if (mTokeenStack.Count > 0) return mTokeenStack.Pop();
            Skip();
            var c = GetChar();
            if (c < 0)
            {
                SetCurrentToken(TokenKind.Eof, "");
                return CurrentToken;
            }
            switch ((char)c)
            {
                case '(':
                    SetCurrentToken(TokenKind.LPar, "(");
                    return CurrentToken;
                case ')':
                    SetCurrentToken(TokenKind.RPar, ")");
                    return CurrentToken;
                case ',':
                    SetCurrentToken(TokenKind.Comma, ",");
                    return CurrentToken;
                case ';':
                    SetCurrentToken(TokenKind.Semicolon, ";");
                    return CurrentToken;
                case '#':
                    SetCurrentToken(TokenKind.Hash, "#");
                    return CurrentToken;
                case '=':
                    SetCurrentToken(TokenKind.Equal, "=");
                    return CurrentToken;
                case '\'':
                    ReadParameter();
                    return CurrentToken;
                case '\\':
                    var c1 = GetChar();
                    if (c1 == '\'')
                    {
                        ReadString();
                        return CurrentToken;
                    }
                    PushChar(c1);
                    SetCurrentToken(TokenKind.Unknown, ((char)c).ToString());
                    return CurrentToken;
                default:
                    if (char.IsNumber((char)c)){
                        mSb.Clear();
                        mSb.Append((char)c);
                        while (true)
                        {
                            c = GetChar();
                            if (!char.IsDigit((char)c)) break;
                            mSb.Append((char)c);
                        }
                        PushChar(c);
                        SetCurrentToken(TokenKind.Number, mSb.ToString());
                        return CurrentToken;
                    }
                    if (char.IsLetter((char)c) || c == '_')
                    {
                        mSb.Clear();
                        //                        var sb = new StringBuilder();
                        mSb.Append((char)c);
                        while (true)
                        {
                            c = GetChar();
                            if (!char.IsLetterOrDigit((char)c) && c != '_') break;
                            mSb.Append((char)c);
                        }
                        PushChar(c);
                        SetCurrentToken(TokenKind.Identifier, mSb.ToString());
                        return CurrentToken;
                    }
                    SetCurrentToken(TokenKind.Unknown, ((char)c).ToString());
                    return CurrentToken;
            }
        }



        public void PushToken(Token tok)
        {
            mTokeenStack.Push(new Token(tok.Kind, tok.Value));
        }

        void SetCurrentToken(TokenKind kind, string s)
        {
            CurrentToken.Set(kind, s);
        }

        void PushChar(int c)
        {
            mCharStack.Push(c);
        }

        int GetChar()
        {
            if (mCharStack.Count > 0) return mCharStack.Pop();
            var c = mReader.Read();
            return c;
        }


        void Skip()
        {
            while (true)
            {
                var c = GetChar();
                if (c < 0 || !char.IsWhiteSpace((char)c))
                {
                    PushChar(c);
                    break;
                }
            }
        }

        void ReadString()
        {
            mSb.Clear();
            while (true)
            {
                var c = GetChar();
                if (c < 0) throw new Exception($"Unexpected eof was found in string : {mSb}");
                var ch = (char)c;
                if (ch == '\\')
                {
                    c = GetChar();
                    if (c < 0) throw new Exception($"Unexpected eof was found in string : {mSb}");
                    ch = (char)c;
                    if (ch == '\'') break;
                    mSb.Append('\\');
                    PushChar(ch);
                }
                mSb.Append(ch);
            }
            SetCurrentToken(TokenKind.String, mSb.ToString());
        }
        void ReadParameter()
        {
            mSb.Clear();
            while (true)
            {
                var c = GetChar();
                if (c < 0) throw new Exception($"Unexpected eof was found in parameter : {mSb}");
                var ch = (char)c;
                if (ch == '\'') break;
                mSb.Append(ch);
            }
            SetCurrentToken(TokenKind.Parameter, mSb.ToString());
        }
    }
}
