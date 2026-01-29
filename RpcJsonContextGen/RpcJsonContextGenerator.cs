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

public static class RpcJsonContextGenerator
{
    /// <summary>
    /// 不需要生成 JsonSerializable 的类型列表
    /// </summary>
    private static readonly string[] s_excludedTypes = 
    [
        // 空类型和异步类型（非泛型）
        "void",
        "Void",
        "Task",
        "ValueTask",
        "Threading.Tasks.Task",
        "Threading.Tasks.ValueTask",
        
        // 基本数据类型（小写关键字）
        "bool",
        "byte",
        "sbyte",
        "char",
        "decimal",
        "double",
        "float",
        "int",
        "uint",
        "long",
        "ulong",
        "short",
        "ushort",
        "string",
        "object",
        
        // 基本数据类型（大写类型名）
        "Boolean",
        "Byte",
        "SByte",
        "Char",
        "Decimal",
        "Double",
        "Single",
        "Int32",
        "UInt32",
        "Int64",
        "UInt64",
        "Int16",
        "UInt16",
        "String",
        "Object",
        
        // 常用的内置类型
        "DateTime",
        "DateTimeOffset",
        "TimeSpan",
        "Guid",
        "Uri"
    ];

    public static string? GenerateFromFiles(IEnumerable<string> filePaths)
    {
        var typeSet = new HashSet<string>(StringComparer.Ordinal);
        
        foreach (var path in filePaths)
        {
            if (!File.Exists(path)) continue;

            var src = File.ReadAllText(path);
            var file = SimpleCSharpStringParser.ParseFile(src);
            foreach (var t in CollectMethodTypes(file.Types))
            {
                typeSet.Add(t);
            }
        }

        var output = GenerateJsonSerializableAttributes(typeSet);
        return string.IsNullOrWhiteSpace(output) ? null : output;
    }

    public static IEnumerable<string> ExpandArgsToFiles(string[] args)
    {
        foreach (var a in args)
        {
            if (string.IsNullOrWhiteSpace(a)) continue;

            var p = a.Trim('"');
            if (File.Exists(p))
            {
                yield return p;
                continue;
            }

            if (Directory.Exists(p))
            {
                foreach (var f in Directory.EnumerateFiles(p, "*.cs", SearchOption.AllDirectories))
                    yield return f;
            }
        }
    }

    public static IEnumerable<string> CollectMethodTypes(IEnumerable<SimpleCSharpStringParser.TypeInfo> types)
    {
        foreach (var t in types)
        {
            foreach (var m in t.Methods)
            {
                if (!string.IsNullOrWhiteSpace(m.ReturnType))
                    yield return NormalizeTypeForJsonSerializable(m.ReturnType);

                foreach (var p in m.Parameters)
                {
                    if (!string.IsNullOrWhiteSpace(p.Type))
                        yield return NormalizeTypeForJsonSerializable(p.Type);
                }
            }
        }
    }

    public static string GenerateJsonSerializableAttributes(IEnumerable<string> distinctTypes)
    {
        var list = distinctTypes
            .Select(NormalizeTypeForJsonSerializable)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t => !s_excludedTypes.Contains(t, StringComparer.Ordinal))
            .Where(t => !IsCallContextType(t))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToList();

        return string.Join(Environment.NewLine, list.Select(t => $"[JsonSerializable(typeof({t}))]"));
    }
    
    private static bool IsCallContextType(string type)
    {
        // 提取类型名称（去除泛型参数）
        var typeName = type;
        var genericIndex = typeName.IndexOf('<');
        if (genericIndex > 0)
        {
            typeName = typeName.Substring(0, genericIndex);
        }
        
        // 去除命名空间，只检查类型名称
        var lastDotIndex = typeName.LastIndexOf('.');
        if (lastDotIndex >= 0)
        {
            typeName = typeName.Substring(lastDotIndex + 1);
        }
        
        return typeName.EndsWith("CallContext", StringComparison.Ordinal);
    }

    private static string NormalizeTypeForJsonSerializable(string type)
    {
        type = type.Trim();
        if (type.Length == 0) return type;

        // Drop Nullable marker
        if (type.Contains('?')) type = type.Replace("?", string.Empty);

        // Remove common namespace qualifiers to reduce duplicates.
        type = type.Replace("global::", string.Empty);
        type = type.Replace("System.", string.Empty);

        // Collapse whitespace.
        type = string.Join(" ", SplitByWhitespaceTopLevel(type));

        // Extract inner type from Task<T> or ValueTask<T>
        // This needs to handle both with and without namespace prefixes
        type = ExtractAsyncTaskInnerType(type);

        return type;
    }

    private static string ExtractAsyncTaskInnerType(string type)
    {
        var trimmed = type.Trim();
        
        // Check if it's Task<T> or ValueTask<T>
        // Handle: Task<T>, Threading.Tasks.Task<T> (after System. removal)
        if (trimmed.StartsWith("Task<") && trimmed.EndsWith(">"))
        {
            return ExtractGenericArgument(trimmed, "Task<");
        }
        
        if (trimmed.StartsWith("Threading.Tasks.Task<") && trimmed.EndsWith(">"))
        {
            return ExtractGenericArgument(trimmed, "Threading.Tasks.Task<");
        }
        
        if (trimmed.StartsWith("ValueTask<") && trimmed.EndsWith(">"))
        {
            return ExtractGenericArgument(trimmed, "ValueTask<");
        }
        
        if (trimmed.StartsWith("Threading.Tasks.ValueTask<") && trimmed.EndsWith(">"))
        {
            return ExtractGenericArgument(trimmed, "Threading.Tasks.ValueTask<");
        }
        
        return type;
    }

    private static string ExtractGenericArgument(string type, string prefix)
    {
        var innerStart = prefix.Length;
        var innerEnd = type.Length - 1;
        
        if (innerStart >= innerEnd) return type;
        
        var inner = type.Substring(innerStart, innerEnd - innerStart);
        return inner.Trim();
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
}
