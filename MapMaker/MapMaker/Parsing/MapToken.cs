namespace MapMaker.Core.Parsing;

public enum MapTokenType
{
    LeftBrace,
    RightBrace,
    LeftParen,
    RightParen,
    String,
    Number,
    Identifier,
    Comment,
    EndOfFile
}

public sealed class MapToken
{
    public MapTokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public MapToken(MapTokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Type} '{Value}' at {Line}:{Column}";
    }
}