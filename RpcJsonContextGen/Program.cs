namespace RpcJsonContextGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args is null || args.Length == 0)
            {
                Console.Error.WriteLine("Usage: ConsoleApp1 <file1.cs> [file2.cs ...]");
                return;
            }

            var typeSet = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in ExpandArgsToFiles(args))
            {
                if (!File.Exists(path)) continue;

                var src = File.ReadAllText(path);
                var file = SimpleCSharpStringParser.ParseFile(src);
                foreach (var t in SimpleCSharpStringParser.CollectMethodTypes(file.Types))
                {
                    typeSet.Add(t);
                }
            }

            var output = SimpleCSharpStringParser.GenerateJsonSerializableAttributes(typeSet);
            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine("No types found.");
                return;
            }

            if (!ClipboardUtil.TrySetText(output))
            {
                Console.WriteLine(output);
                Console.Error.WriteLine("(Could not set clipboard; printed to stdout.)");
            }
        }

        private static IEnumerable<string> ExpandArgsToFiles(string[] args)
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

            yield break;
        }
    }
}
