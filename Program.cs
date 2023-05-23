using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Start;

public static partial class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintErrorAndExit("No arguments provided.", true);
            return;
        }

        if (Environment.OSVersion.Platform != PlatformID.Unix)
        { 
            PrintErrorAndExit("This script can only be run on Linux.", false);
            return;
        }

        switch (args[0])
        {
            case "--help":
            case "-h":
                PrintHelp();
                break;
            case "-r":
                if (args.Length < 2)
                {
                    PrintErrorAndExit("No script path provided.", true);
                    return;
                }
                else if (args.Length > 2)
                {
                    PrintErrorAndExit("Too many arguments.", true);
                    return;
                }

                RunLauncher(args[1], true);
                break;
            default:
                if (args.Length > 1)
                {
                    PrintErrorAndExit("Too many arguments.", true);
                    return;
                }

                RunLauncher(args[0], false);
                break;
        }
    }

    static void PrintErrorAndExit(string message, bool printHelp)
    {
        Console.WriteLine(message);
        if (printHelp)
            PrintHelp();

        Environment.Exit(1);
    }

    static void PrintHelp()
    { 
        Console.WriteLine("Start - A simple scenario runner for Linux.");
        Console.WriteLine("Usage: start [option] <scriptPath>");
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help    Print this help message.");
        Console.WriteLine("  -r            Require root privileges to run the script.");
    }

    static void RunLauncher(string scriptPath, bool requireRoot)
    {
        if (requireRoot && Environment.GetEnvironmentVariable("USER") != "root")
        {
            PrintErrorAndExit("This script requires root privileges.", false);
            return;
        }

        if (!File.Exists(scriptPath))
        {
            PrintErrorAndExit($"Script file '{scriptPath}' does not exist.", false);
            return;
        }

        string[] lines = File.ReadAllLines(scriptPath);
        if (lines.Length == 0)
        {
            PrintErrorAndExit($"Script file at '{scriptPath}' is empty.", false);
            return;
        }

        Console.WriteLine($"Running script '{scriptPath}'...");
        for (int i = 0; i < lines.Length; i++)
        {
            try
            {
                ParseAndExecuteLine(lines[i]);
            }
            catch (Exception ex)
            {
                PrintErrorAndExit($"Error while executing line '{i+1}': {ex.Message}", false);
                return;
            }
        }
    }

    static void ParseAndExecuteLine(string line)
    { 
		if (line.StartsWith("//"))
			return;

		//Split string into command and arguments but dont split arguments with spaces or double or single quotes
		string[] parts = LineRegex().Split(line.TrimStart()).Select(x => x.Trim()).ToArray();

		if (parts.Length == 0)
            return;

        string command = parts[0];
        string[] args = parts[1..];

        switch (command)
        {
            case "cd":
                if (args.Length == 0)
                    throw new Exception("No directory provided.");

                string path = args[0];
                if (!Directory.Exists(path))
                    throw new Exception($"Directory '{path}' does not exist.");

                Directory.SetCurrentDirectory(path);
                break;
			case "run":
            case "exec":
                if (args.Length == 0)
                    throw new Exception("No command provided.");

                Process.Start(args[0], string.Join(' ', args.Skip(1))).WaitForExit();
                break;
            case "mkdir":
                if (args.Length == 0)
                    throw new Exception("No directory provided.");

                Directory.CreateDirectory(args[0]);
                break;
            case "rm":
                if (args.Length == 0)
                    throw new Exception("No file provided.");

                File.Delete(args[0]);
                break;
            case "rmdir":
                if (args.Length == 0)
                    throw new Exception("No directory provided.");

                Directory.Delete(args[0], true);
                break;
            case "sleep":
                if (args.Length == 0)
                    throw new Exception("No time provided.");

                Thread.Sleep(int.Parse(args[0]));
                break;
            default:
                throw new Exception($"Unknown command '{command}'.");
        }
    }

	[GeneratedRegex("\\s(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)(?=(?:[^']*'[^']*')*[^']*$)")]
	private static partial Regex LineRegex();
}