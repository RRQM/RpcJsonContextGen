// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace RpcJsonContextGen;

public static class SimpleCSharpStringParser
{
    public sealed record ParameterInfo(string Type, string Name, List<string> Attributes);
    public sealed record MethodInfo(string ReturnType, string Name, List<ParameterInfo> Parameters, List<string> Attributes);
    public sealed record TypeInfo(string Kind, string TypeName, List<MethodInfo> Methods, List<string> Attributes);
    public sealed record FileInfo(List<string> Usings, List<TypeInfo> Types);

    public static FileInfo ParseFile(string src)
    {
        var sanitized = RemoveCommentsAndStrings(src);
        var usings = ParseUsings(sanitized);
        var types = ParsePublicTypesFromSanitized(sanitized);
        return new FileInfo(usings, types);
    }

    public static List<TypeInfo> ParsePublicTypes(string src) => ParseFile(src).Types;

    private static List<TypeInfo> ParsePublicTypesFromSanitized(string src)
    {
        var types = new List<TypeInfo>();
        int i = 0;
        while (i < src.Length)
        {
            var next = FindNextTypeKeyword(src, i);
            if (next.index < 0) break;

            var name = ParseTypeNameAt(src, next.index, next.keyword);
            var (open, close) = TryGetTypeBodyRange(src, next.index);
            if (!string.IsNullOrWhiteSpace(name) && open >= 0 && close > open)
            {
                var body = src.Substring(open + 1, close - open - 1);
                var typeAttributes = ParseTypeAttributes(src, next.index);
                var methods = next.keyword == "interface"
                    ? ParseInterfaceMethodsInBody(body)
                    : ParsePublicMethodsInBody(body);

                types.Add(new TypeInfo(next.keyword, name, methods, typeAttributes));
                i = close + 1;
            }
            else
            {
                i = next.index + next.keyword.Length;
            }
        }

        return types;
    }

    private static List<string> ParseTypeAttributes(string src, int typeKeywordIndex)
    {
        // Attributes are typically before modifiers: [A] public class X
        // Scan backwards for a previous run of attribute blocks and ensure only modifiers/whitespace in-between.
        var attrs = new List<string>();

        int i = typeKeywordIndex - 1;
        while (i >= 0 && char.IsWhiteSpace(src[i])) i--;

        // Walk backwards skipping modifier tokens and whitespace between attributes and the type keyword.
        while (i >= 0)
        {
            while (i >= 0 && char.IsWhiteSpace(src[i])) i--;
            if (i < 0) break;

            // If we are right after an attribute block, collect it.
            if (src[i] == ']')
            {
                var before = ParseAttributesBeforeIndex(src, i + 1);
                attrs.InsertRange(0, before);
                return attrs;
            }

            // Skip identifier/modifier token (e.g. public, partial, sealed, abstract, internal)
            if (char.IsLetter(src[i]) || src[i] == '_')
            {
                var end = i;
                var start = i;
                while (start >= 0 && (char.IsLetterOrDigit(src[start]) || src[start] == '_')) start--;
                var word = src.Substring(start + 1, end - start);
                if (word is "public" or "internal" or "private" or "protected" or "partial" or "sealed" or "abstract" or "static" or "new")
                {
                    i = start;
                    continue;
                }
            }

            break;
        }

        return attrs;
    }

    private static (int index, string keyword) FindNextTypeKeyword(string src, int start)
    {
        var c = IndexOfWord(src, "class", start);
        var itf = IndexOfWord(src, "interface", start);
        if (c < 0 && itf < 0) return (-1, string.Empty);
        if (c >= 0 && (itf < 0 || c < itf)) return (c, "class");
        return (itf, "interface");
    }

    private static string ParseTypeNameAt(string src, int keywordIndex, string keyword)
    {
        var idx = keywordIndex + keyword.Length;
        idx = SkipWs(src, idx);
        var start = idx;
        while (idx < src.Length && (char.IsLetterOrDigit(src[idx]) || src[idx] == '_')) idx++;
        return idx > start ? src.Substring(start, idx - start) : string.Empty;
    }

    private static (int openBrace, int closeBrace) TryGetTypeBodyRange(string src, int typeKeywordIndex)
    {
        var braceOpen = src.IndexOf('{', typeKeywordIndex);
        if (braceOpen < 0) return (-1, -1);
        var braceClose = FindMatchingBrace(src, braceOpen);
        if (braceClose < 0) return (-1, -1);
        return (braceOpen, braceClose);
    }

    private static List<MethodInfo> ParsePublicMethodsInBody(string body)
    {
        var results = new List<MethodInfo>();
        int i = 0;
        while (i < body.Length)
        {
            var pub = IndexOfWord(body, "public", i);
            if (pub < 0) break;

            var methodAttributes = ParseAttributesBeforeIndex(body, pub);

            var scan = pub + "public".Length;
            scan = SkipWs(body, scan);

            while (true)
            {
                var word = ReadWord(body, scan);
                if (word is null) break;
                if (word is "static" or "virtual" or "override" or "async" or "partial" or "new")
                {
                    scan = SkipWs(body, scan + word.Length);
                    continue;
                }
                break;
            }

            var returnType = ReadTypeToken(body, scan);
            if (string.IsNullOrWhiteSpace(returnType)) { i = pub + 6; continue; }
            scan = SkipWs(body, scan + returnType.Length);

            var name = ReadWord(body, scan);
            if (string.IsNullOrWhiteSpace(name)) { i = pub + 6; continue; }
            scan += name.Length;
            scan = SkipWs(body, scan);

            if (scan >= body.Length || body[scan] != '(') { i = pub + 6; continue; }
            var parenClose = FindMatchingParen(body, scan);
            if (parenClose < 0) { i = pub + 6; continue; }

            var paramList = body.Substring(scan + 1, parenClose - scan - 1);
            var parameters = ParseParameters(paramList);
            results.Add(new MethodInfo(returnType.Trim(), name.Trim(), parameters, methodAttributes));

            i = parenClose + 1;
        }

        return results;
    }

    private static List<MethodInfo> ParseInterfaceMethodsInBody(string body)
    {
        var results = new List<MethodInfo>();
        int i = 0;
        while (i < body.Length)
        {
            i = SkipWs(body, i);
            var memberAttributes = ParseAttributesAtIndex(ref i, body);

            var semi = body.IndexOf(';', i);
            if (semi < 0) break;

            var stmt = body.Substring(i, semi - i).Trim();
            i = semi + 1;

            if (stmt.Length == 0) continue;
            if (stmt.Contains('{')) continue;
            if (!stmt.Contains('(') || !stmt.Contains(')')) continue;

            if (stmt.StartsWith("public ", StringComparison.Ordinal))
            {
                stmt = stmt.Substring("public ".Length).TrimStart();
            }

            var openParen = stmt.IndexOf('(');
            var closeParen = FindMatchingParen(stmt, openParen);
            if (openParen < 0 || closeParen < 0) continue;

            var left = stmt.Substring(0, openParen).Trim();
            var paramList = stmt.Substring(openParen + 1, closeParen - openParen - 1);

            var leftTokens = SplitByWhitespaceTopLevel(left);
            if (leftTokens.Count < 2) continue;
            var name = leftTokens[^1];
            var returnType = string.Join(" ", leftTokens.Take(leftTokens.Count - 1));

            results.Add(new MethodInfo(returnType.Trim(), name.Trim(), ParseParameters(paramList), memberAttributes));
        }

        return results;
    }

    private static List<ParameterInfo> ParseParameters(string paramList)
    {
        var list = new List<ParameterInfo>();
        if (string.IsNullOrWhiteSpace(paramList)) return list;

        foreach (var raw in SplitTopLevel(paramList, ','))
        {
            var p = raw.Trim();
            if (p.Length == 0) continue;

            var paramAttributes = new List<string>();

            var eq = p.IndexOf('=');
            if (eq >= 0) p = p.Substring(0, eq).Trim();

            while (p.StartsWith("[", StringComparison.Ordinal))
            {
                var end = FindMatchingSquare(p, 0);
                if (end < 0) break;
                var attrBlock = p.Substring(0, end + 1).Trim();
                paramAttributes.AddRange(ExtractAttributeNames(attrBlock));
                p = p.Substring(end + 1).Trim();
            }

            foreach (var mod in new[] { "in ", "out ", "ref ", "params " })
            {
                if (p.StartsWith(mod, StringComparison.Ordinal))
                {
                    p = p.Substring(mod.Length).TrimStart();
                    break;
                }
            }

            var tokens = SplitByWhitespaceTopLevel(p);
            if (tokens.Count < 2) continue;

            var name = tokens[^1];
            var type = string.Join(" ", tokens.Take(tokens.Count - 1));
            list.Add(new ParameterInfo(type.Trim(), name.Trim(), paramAttributes));
        }

        return list;
    }

    private static List<string> ParseAttributesBeforeIndex(string src, int index)
    {
        var list = new List<string>();
        if (index <= 0) return list;

        int i = index - 1;
        while (i >= 0 && char.IsWhiteSpace(src[i])) i--;

        while (i >= 0)
        {
            while (i >= 0 && char.IsWhiteSpace(src[i])) i--;
            if (i < 0 || src[i] != ']') break;

            int depth = 0;
            int j = i;
            for (; j >= 0; j--)
            {
                if (src[j] == ']') depth++;
                else if (src[j] == '[')
                {
                    depth--;
                    if (depth == 0) break;
                }
            }
            if (j < 0) break;

            var attrBlock = src.Substring(j, i - j + 1).Trim();
            foreach (var a in ExtractAttributeNames(attrBlock)) list.Insert(0, a);
            i = j - 1;
        }

        return list;
    }

    private static List<string> ParseAttributesAtIndex(ref int index, string src)
    {
        var list = new List<string>();
        index = SkipWs(src, index);
        while (index < src.Length && src[index] == '[')
        {
            var end = FindMatchingSquare(src, index);
            if (end < 0) break;
            var attrBlock = src.Substring(index, end - index + 1).Trim();
            list.AddRange(ExtractAttributeNames(attrBlock));
            index = SkipWs(src, end + 1);
        }
        return list;
    }

    private static IEnumerable<string> ExtractAttributeNames(string attrBlock)
    {
        int i = 0;
        while (i < attrBlock.Length)
        {
            if (attrBlock[i] == '[' || attrBlock[i] == ',' || char.IsWhiteSpace(attrBlock[i])) { i++; continue; }
            if (attrBlock[i] == ']') break;

            var start = i;
            int angle = 0;
            while (i < attrBlock.Length)
            {
                var c = attrBlock[i];
                if (c == '<') angle++;
                else if (c == '>') angle = Math.Max(0, angle - 1);
                if (angle == 0 && (c == '(' || c == ',' || c == ']')) break;
                i++;
            }
            var name = attrBlock.Substring(start, i - start).Trim();
            if (name.Length > 0) yield return name;

            if (i < attrBlock.Length && attrBlock[i] == '(')
            {
                var close = FindMatchingParen(attrBlock, i);
                i = close > i ? close + 1 : i + 1;
            }
        }
    }

    private static string RemoveCommentsAndStrings(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        bool inLineComment = false, inBlockComment = false, inString = false, inChar = false, verbatim = false;

        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            var next = i + 1 < s.Length ? s[i + 1] : '\0';

            if (inLineComment)
            {
                if (c == '\n') { inLineComment = false; sb.Append(c); }
                else sb.Append(' ');
                continue;
            }
            if (inBlockComment)
            {
                if (c == '*' && next == '/') { inBlockComment = false; sb.Append("  "); i++; }
                else sb.Append(c == '\n' ? '\n' : ' ');
                continue;
            }

            if (inString)
            {
                if (verbatim)
                {
                    if (c == '"' && next == '"') { sb.Append("  "); i++; continue; }
                    if (c == '"') { inString = false; verbatim = false; sb.Append(' '); continue; }
                    sb.Append(c == '\n' ? '\n' : ' ');
                    continue;
                }
                if (c == '\\') { sb.Append("  "); i++; continue; }
                if (c == '"') { inString = false; sb.Append(' '); continue; }
                sb.Append(c == '\n' ? '\n' : ' ');
                continue;
            }
            if (inChar)
            {
                if (c == '\\') { sb.Append("  "); i++; continue; }
                if (c == '\'') { inChar = false; sb.Append(' '); continue; }
                sb.Append(' ');
                continue;
            }

            if (c == '/' && next == '/') { inLineComment = true; sb.Append("  "); i++; continue; }
            if (c == '/' && next == '*') { inBlockComment = true; sb.Append("  "); i++; continue; }
            if (c == '@' && next == '"') { inString = true; verbatim = true; sb.Append("  "); i++; continue; }
            if (c == '"') { inString = true; sb.Append(' '); continue; }
            if (c == '\'') { inChar = true; sb.Append(' '); continue; }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static int SkipWs(string s, int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        return i;
    }

    private static string? ReadWord(string s, int i)
    {
        i = SkipWs(s, i);
        if (i >= s.Length) return null;
        if (!(char.IsLetter(s[i]) || s[i] == '_')) return null;
        var start = i;
        while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
        return s.Substring(start, i - start);
    }

    private static string ReadTypeToken(string s, int i)
    {
        i = SkipWs(s, i);
        var start = i;
        int angle = 0;
        while (i < s.Length)
        {
            var c = s[i];
            if (c == '<') angle++;
            else if (c == '>') angle = Math.Max(0, angle - 1);
            else if (angle == 0)
            {
                if (char.IsWhiteSpace(c)) break;
                if (c == '(') break;
            }
            i++;
        }
        return s.Substring(start, i - start);
    }

    private static int FindMatchingBrace(string s, int openIndex)
    {
        int depth = 0;
        for (int i = openIndex; i < s.Length; i++)
        {
            if (s[i] == '{') depth++;
            else if (s[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static int FindMatchingParen(string s, int openIndex)
    {
        int depth = 0;
        for (int i = openIndex; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static int FindMatchingSquare(string s, int openIndex)
    {
        int depth = 0;
        for (int i = openIndex; i < s.Length; i++)
        {
            if (s[i] == '[') depth++;
            else if (s[i] == ']')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static int IndexOfWord(string s, string word, int startIndex = 0)
    {
        for (int i = startIndex; i <= s.Length - word.Length; i++)
        {
            if (i > 0 && (char.IsLetterOrDigit(s[i - 1]) || s[i - 1] == '_')) continue;
            if (!s.AsSpan(i, word.Length).SequenceEqual(word)) continue;
            var end = i + word.Length;
            if (end < s.Length && (char.IsLetterOrDigit(s[end]) || s[end] == '_')) continue;
            return i;
        }
        return -1;
    }

    private static List<string> SplitTopLevel(string s, char separator)
    {
        var list = new List<string>();
        int start = 0;
        int angle = 0, paren = 0, square = 0;

        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '<') angle++;
            else if (c == '>') angle = Math.Max(0, angle - 1);
            else if (c == '(') paren++;
            else if (c == ')') paren = Math.Max(0, paren - 1);
            else if (c == '[') square++;
            else if (c == ']') square = Math.Max(0, square - 1);
            else if (c == separator && angle == 0 && paren == 0 && square == 0)
            {
                list.Add(s.Substring(start, i - start));
                start = i + 1;
            }
        }

        list.Add(s.Substring(start));
        return list;
    }

    private static List<string> SplitByWhitespaceTopLevel(string s)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < s.Length)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            if (i >= s.Length) break;

            var start = i;
            int angle = 0;
            while (i < s.Length)
            {
                var c = s[i];
                if (c == '<') angle++;
                else if (c == '>') angle = Math.Max(0, angle - 1);
                else if (angle == 0 && char.IsWhiteSpace(c)) break;
                i++;
            }
            tokens.Add(s.Substring(start, i - start));
        }
        return tokens;
    }

    private static List<string> ParseUsings(string src)
    {
        var list = new List<string>();

        int braceDepth = 0;
        for (int i = 0; i < src.Length; i++)
        {
            var c = src[i];
            if (c == '{') braceDepth++;
            else if (c == '}') braceDepth = Math.Max(0, braceDepth - 1);

            if (braceDepth != 0) continue;

            int j = i;
            while (j < src.Length && char.IsWhiteSpace(src[j])) j++;

            if (j < src.Length && IndexOfWord(src, "using", j) == j)
            {
                var semi = src.IndexOf(';', j);
                if (semi < 0) break;
                var text = src.Substring(j, semi - j).Trim();
                if (text.Length > 0) list.Add(text);
                i = semi;
            }
        }

        return list;
    }
}
