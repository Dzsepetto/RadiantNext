using System.Globalization;
using System.Text;

namespace MapMaker.Core.Parsing;

public sealed class MapTokenizer
{
    private readonly string _text;
    private readonly List<MapToken> _tokens = new();

    private int _index;
    private int _line = 1;
    private int _column = 1;

    public MapTokenizer(string text)
    {
        _text = text ?? string.Empty;
    }

    public List<MapToken> Tokenize()
    {
        while (!IsAtEnd())
        {
            char c = Peek();

            if (char.IsWhiteSpace(c))
            {
                ReadWhitespace();
                continue;
            }

            if (c == '/' && PeekNext() == '/')
            {
                ReadComment();
                continue;
            }

            if (c == '{')
            {
                AddSingle(MapTokenType.LeftBrace);
                continue;
            }

            if (c == '}')
            {
                AddSingle(MapTokenType.RightBrace);
                continue;
            }

            if (c == '(')
            {
                AddSingle(MapTokenType.LeftParen);
                continue;
            }

            if (c == ')')
            {
                AddSingle(MapTokenType.RightParen);
                continue;
            }

            if (c == '"')
            {
                ReadString();
                continue;
            }

            if (char.IsDigit(c) || c == '-' || c == '+')
            {
                ReadNumberOrIdentifier();
                continue;
            }

            ReadIdentifier();
        }

        _tokens.Add(new MapToken(MapTokenType.EndOfFile, string.Empty, _line, _column));
        return _tokens;
    }

    private void ReadWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
        {
            Advance();
        }
    }

    private void ReadComment()
    {
        int startLine = _line;
        int startColumn = _column;

        var sb = new StringBuilder();

        while (!IsAtEnd() && Peek() != '\n')
        {
            sb.Append(Advance());
        }

        _tokens.Add(new MapToken(MapTokenType.Comment, sb.ToString(), startLine, startColumn));
    }

    private void ReadString()
    {
        int startLine = _line;
        int startColumn = _column;

        Advance();

        var sb = new StringBuilder();

        while (!IsAtEnd() && Peek() != '"')
        {
            if (Peek() == '\\' && PeekNext() == '"')
            {
                Advance();
                sb.Append(Advance());
                continue;
            }

            sb.Append(Advance());
        }

        if (!IsAtEnd())
        {
            Advance();
        }

        _tokens.Add(new MapToken(MapTokenType.String, sb.ToString(), startLine, startColumn));
    }

    private void ReadNumberOrIdentifier()
    {
        int startLine = _line;
        int startColumn = _column;

        var sb = new StringBuilder();

        while (!IsAtEnd() && !char.IsWhiteSpace(Peek()) && "{}()".IndexOf(Peek()) == -1)
        {
            sb.Append(Advance());
        }

        string value = sb.ToString();

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            _tokens.Add(new MapToken(MapTokenType.Number, value, startLine, startColumn));
        }
        else
        {
            _tokens.Add(new MapToken(MapTokenType.Identifier, value, startLine, startColumn));
        }
    }

    private void ReadIdentifier()
    {
        int startLine = _line;
        int startColumn = _column;

        var sb = new StringBuilder();

        while (!IsAtEnd() && !char.IsWhiteSpace(Peek()) && "{}()".IndexOf(Peek()) == -1)
        {
            sb.Append(Advance());
        }

        _tokens.Add(new MapToken(MapTokenType.Identifier, sb.ToString(), startLine, startColumn));
    }

    private void AddSingle(MapTokenType type)
    {
        int startLine = _line;
        int startColumn = _column;
        string value = Advance().ToString();

        _tokens.Add(new MapToken(type, value, startLine, startColumn));
    }

    private char Peek()
    {
        return _text[_index];
    }

    private char PeekNext()
    {
        if (_index + 1 >= _text.Length)
        {
            return '\0';
        }

        return _text[_index + 1];
    }

    private char Advance()
    {
        char c = _text[_index];
        _index++;

        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return c;
    }

    private bool IsAtEnd()
    {
        return _index >= _text.Length;
    }
}