using MapMaker.Core.Models;

namespace MapMaker.Core.Parsing
{
    public sealed class MapParseResult
    {
        public Map Map { get; }
        public List<MapParseIssue> Issues { get; }

        public bool Success => Issues.All(x => x.Severity != MapParseIssueSeverity.Error);

        public MapParseResult(Map map, List<MapParseIssue> issues)
        {
            Map = map;
            Issues = issues;
        }
    }
}