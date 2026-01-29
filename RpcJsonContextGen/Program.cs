namespace RpcJsonContextGen;

internal class Program
{
    static void Main(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            Console.Error.WriteLine("Usage: ConsoleApp1 <file1.cs> [file2.cs ...]");
            return;
        }

        var files = RpcJsonContextGenerator.ExpandArgsToFiles(args);
        var output = RpcJsonContextGenerator.GenerateFromFiles(files);

        if (output is null)
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
}
