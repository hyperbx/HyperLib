using HyperLib.Formats.Barracuda;
using HyperLib.Helpers;
using Spectre.Console;

Console.WriteLine
(
    "Hydro Thunder Tool\n" +
    "Written by Hyper\n"
);

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    Console.WriteLine("An unhandled exception has occurred.\n");
    Console.WriteLine(e.ExceptionObject.ToString());
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
};

if (args.Length <= 0)
{
    Console.WriteLine("Usage: drag and drop a supported file into HydroThunderTool.exe\n");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

var inputPath = args[0];
var outputPath = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath));

Console.WriteLine($"Path: {inputPath}\n");

bool IsTimedEventAsset()
{
    if (inputPath.Contains("AkTimedEventAsset"))
        return true;

    var path = inputPath;

    while (!string.IsNullOrEmpty(path))
    {
        path = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(path))
            break;

        /* Back out of the containing directory until we
           reach the game's root directory, then confirm
           if "Base.apf" is present.
        
           If it is present, it's safe to assume the user
           is not in a directory where there wouldn't be any
           timed event assets.
        
           Otherwise, we should prompt the user to confirm. */
        if (File.Exists(Path.Combine(path, "Base.apf")))
            return false;
    }

    var result = AnsiConsole.Confirm("Is this a timed event asset?", false);

    Console.WriteLine();

    return result;
}

bool IsPCVersion()
{
    var path = inputPath;

    while (!string.IsNullOrEmpty(path))
    {
        path = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(path))
            break;

        /* Back out of the containing directory until we
           reach the game's root directory, then confirm
           if "Base.apf" is present.
        
           If it is present, check if any of the archive's
           root directories start with "Vu". If they do,
           this directory is likely an extracted PC archive.
        
           Otherwise, we should prompt the user to confirm. */
        if (File.Exists(Path.Combine(path, "Base.apf")))
        {
            path = Path.GetRelativePath(path, inputPath);
            path = FileSystemHelper.OmitFirstDirectory(path);

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                var fileName = Path.GetFileName(dir);

                if (fileName == "Assets")
                    continue;

                if (fileName.StartsWith("Vu"))
                    return true;
            }

            return false;
        }
    }

    var result = AnsiConsole.Confirm("Is this the PC version?", false);

    Console.WriteLine();

    return result;
}

var isPCVersion = IsPCVersion();

if (File.Exists(inputPath))
{
    switch (Path.GetExtension(inputPath))
    {
        case ".apf":
            new Archive(inputPath).Export(outputPath);
            break;

        case ".bin":
        {
            outputPath += ".json";

            if (IsTimedEventAsset())
            {
                new TimedEvent(inputPath).Export(outputPath);

                Console.WriteLine("Exported as a timed event asset.");
            }
            else
            {
                new JsonBinary(inputPath).Export(outputPath);

                Console.WriteLine("Exported as a JSON binary asset.");
            }

            break;
        }

        case ".json":
        {
            outputPath += ".bin";

            if (IsTimedEventAsset())
            {
                var tev = new TimedEvent() { IsPCVersion = isPCVersion };
                tev.Import(inputPath);
                tev.Write(outputPath);

                Console.WriteLine("Exported as a timed event asset.");
            }
            else
            {
                var ajb = new JsonBinary() { IsPCVersion = isPCVersion };
                ajb.Import(inputPath);
                ajb.Write(outputPath);

                Console.WriteLine("Exported as a JSON binary asset.");
            }

            break;
        }

        default:
            Console.WriteLine("Unsupported file format.");
            break;
    }
}
else if (Directory.Exists(inputPath))
{
    var apf = new Archive() { IsPCVersion = isPCVersion };
    apf.Import(inputPath);

    Console.WriteLine("\nWriting archive...");

    apf.Write(outputPath + ".apf");
}

Console.WriteLine("\nDone.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();