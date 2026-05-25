namespace MapMaker.Core.Parsing;

public enum MapParseIssueSeverity
{
    Warning,
    Error
}

public sealed class MapParseIssue
{
    public MapParseIssueSeverity Severity { get; }
    public string Message { get; }
    public int Line { get; }
    public int Column { get; }

    public MapParseIssue(
        MapParseIssueSeverity severity,
        string message,
        int line,
        int column)
    {
        Severity = severity;
        Message = message;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Severity}: {Message} at {Line}:{Column}";
    }
}