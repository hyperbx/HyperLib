using HyperLib.Formats.Barracuda;
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
    if (inputPath.Contains("Base\\AkTimedEventAsset"))
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
                var tev = new TimedEvent();
                tev.Import(inputPath);
                tev.Write(outputPath);

                Console.WriteLine("Exported as a timed event asset.");
            }
            else
            {
                var ajb = new JsonBinary();
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
    var apf = new Archive();
    apf.Import(inputPath);

    Console.WriteLine("\nWriting archive...");

    apf.Write(outputPath + ".apf");
}

Console.WriteLine("\nDone.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();