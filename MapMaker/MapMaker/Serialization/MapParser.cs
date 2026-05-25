using System.Globalization;
using System.Numerics;
using MapMaker.Core.Logger;
using MapMaker.Core.Models;
using MapMaker.Core.Parsing;

namespace MapMaker.Core.IO
{
    public static class MapParser
    {
        public static Map Load(string path, ILogger? logger = null)
        {
            var result = LoadResult(path);

            if (logger != null)
            {
                foreach (var issue in result.Issues)
                {
                    logger.Error(issue.ToString());
                }
            }

            return result.Map;
        }

        public static MapParseResult LoadResult(string path)
        {
            string text = File.ReadAllText(path);
            return Parse(text);
        }

        public static MapParseResult Parse(string text)
        {
            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            var parser = new TokenMapParser(tokens);
            return parser.Parse();
        }

        private sealed class TokenMapParser
        {
            private readonly List<MapToken> _tokens;
            private readonly List<MapParseIssue> _issues = new();

            private int _position;

            public TokenMapParser(List<MapToken> tokens)
            {
                _tokens = tokens;
            }

            public MapParseResult Parse()
            {
                var map = new Map();

                while (!IsAtEnd())
                {
                    if (Match(MapTokenType.Comment))
                    {
                        continue;
                    }

                    if (Check(MapTokenType.LeftBrace))
                    {
                        var entity = ParseEntity();

                        if (entity != null)
                        {
                            map.Entities.Add(entity);
                        }

                        continue;
                    }

                    AddWarning(Current(), "Unknown token outside entity.");
                    Advance();
                }

                return new MapParseResult(map, _issues);
            }

            private Entity? ParseEntity()
            {
                if (!Consume(MapTokenType.LeftBrace, "Expected '{' before entity."))
                {
                    return null;
                }

                var entity = new Entity();

                while (!Check(MapTokenType.RightBrace) && !IsAtEnd())
                {
                    if (Match(MapTokenType.Comment))
                    {
                        continue;
                    }

                    if (Check(MapTokenType.String))
                    {
                        var key = Advance();

                        if (!Check(MapTokenType.String))
                        {
                            AddError(Current(), $"Expected value after entity key '{key.Value}'.");
                            continue;
                        }

                        var value = Advance();
                        entity.Properties[key.Value] = value.Value;
                        continue;
                    }

                    if (Check(MapTokenType.LeftBrace))
                    {
                        var brush = ParseBrush();

                        if (brush != null)
                        {
                            entity.Brushes.Add(brush);
                        }

                        continue;
                    }

                    AddWarning(Current(), "Unknown token inside entity.");
                    Advance();
                }

                Consume(MapTokenType.RightBrace, "Expected '}' after entity.");

                return entity;
            }

            private Brush? ParseBrush()
            {
                if (!Consume(MapTokenType.LeftBrace, "Expected '{' before brush."))
                {
                    return null;
                }

                var brush = new Brush();

                while (!Check(MapTokenType.RightBrace) && !IsAtEnd())
                {
                    if (Match(MapTokenType.Comment))
                    {
                        continue;
                    }

                    if (Check(MapTokenType.LeftParen))
                    {
                        var face = ParseFace();

                        if (face != null)
                        {
                            brush.Faces.Add(face);
                        }

                        continue;
                    }

                    AddWarning(Current(), "Unknown token inside brush.");
                    Advance();
                }

                Consume(MapTokenType.RightBrace, "Expected '}' after brush.");

                return brush;
            }

            private Face? ParseFace()
            {
                var startToken = Current();

                if (!TryReadVector(out var p1))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadVector(out var p2))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadVector(out var p3))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadTexture(out var texture))
                {
                    AddError(Current(), "Expected texture name after face points.");
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadFloat(out var shiftX, "Expected face shiftX value."))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadFloat(out var shiftY, "Expected face shiftY value."))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadFloat(out var rotation, "Expected face rotation value."))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadFloat(out var scaleX, "Expected face scaleX value."))
                {
                    SynchronizeFace();
                    return null;
                }

                if (!TryReadFloat(out var scaleY, "Expected face scaleY value."))
                {

                    SynchronizeFace();

                    return null;
                }
                var extraParameters = new List<string>();

                while (
                    !IsAtEnd() &&
                    !Check(MapTokenType.LeftParen) &&
                    !Check(MapTokenType.RightBrace) &&
                    !Check(MapTokenType.Comment)
                )
                {
                    extraParameters.Add(Advance().Value);
                }
                return new Face(
                    p1,
                    p2,
                    p3,
                    texture,
                    shiftX,
                    shiftY,
                    rotation,
                    scaleX,
                    scaleY,
                    extraParameters
                );
            }

            private bool TryReadVector(out Vector3 vector)
            {
                vector = Vector3.Zero;

                if (!Consume(MapTokenType.LeftParen, "Expected '(' before vector."))
                {
                    return false;
                }

                if (!TryReadFloat(out var x, "Expected vector X value."))
                {
                    return false;
                }

                if (!TryReadFloat(out var y, "Expected vector Y value."))
                {
                    return false;
                }

                if (!TryReadFloat(out var z, "Expected vector Z value."))
                {
                    return false;
                }

                if (!Consume(MapTokenType.RightParen, "Expected ')' after vector."))
                {
                    return false;
                }

                vector = new Vector3(x, y, z);
                return true;
            }

            private bool TryReadTexture(out string texture)
            {
                texture = string.Empty;

                if (
                    !Check(MapTokenType.Identifier) &&
                    !Check(MapTokenType.String) &&
                    !Check(MapTokenType.Number)
                )
                {
                    return false;
                }

                texture = Advance().Value;
                return true;
            }

            private bool TryReadFloat(out float value, string errorMessage)
            {
                value = 0f;

                if (!Check(MapTokenType.Number))
                {
                    AddError(Current(), errorMessage);
                    return false;
                }

                var token = Advance();

                if (!float.TryParse(token.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    AddError(token, $"Invalid number value '{token.Value}'.");
                    return false;
                }

                return true;
            }

            private void SynchronizeFace()
            {
                while (!IsAtEnd())
                {
                    if (Check(MapTokenType.RightBrace))
                    {
                        return;
                    }

                    if (Check(MapTokenType.LeftParen))
                    {
                        return;
                    }

                    Advance();
                }
            }

            private bool Match(MapTokenType type)
            {
                if (!Check(type))
                {
                    return false;
                }

                Advance();
                return true;
            }

            private bool Consume(MapTokenType type, string message)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }

                AddError(Current(), message);
                return false;
            }

            private bool Check(MapTokenType type)
            {
                if (IsAtEnd())
                {
                    return type == MapTokenType.EndOfFile;
                }

                return Current().Type == type;
            }

            private MapToken Advance()
            {
                if (!IsAtEnd())
                {
                    _position++;
                }

                return Previous();
            }

            private MapToken Current()
            {
                return _tokens[_position];
            }

            private MapToken Previous()
            {
                return _tokens[_position - 1];
            }

            private bool IsAtEnd()
            {
                return Current().Type == MapTokenType.EndOfFile;
            }

            private void AddError(MapToken token, string message)
            {
                _issues.Add(new MapParseIssue(
                    MapParseIssueSeverity.Error,
                    message,
                    token.Line,
                    token.Column
                ));
            }

            private void AddWarning(MapToken token, string message)
            {
                _issues.Add(new MapParseIssue(
                    MapParseIssueSeverity.Warning,
                    message,
                    token.Line,
                    token.Column
                ));
            }
        }
    }
}