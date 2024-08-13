using HyperLib.Formats.Barracuda;

Console.WriteLine
(
    "Hydro Thunder Repacker\n" +
    "Written by Hyper\n"
);

if (args.Length <= 0)
{
    Console.WriteLine("Usage: HydroThunderRepacker.exe \"Base.apf\" [opt: outputDir]");
    Console.WriteLine("       HydroThunderRepacker.exe \"Base\" [opt: outputFile] [opt: /PC]\n");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

var isPCVersion = false;

var inputPath = args[0];
var outputPath = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath));

if (Directory.Exists(inputPath))
    outputPath += ".apf";

if (args.Length > 1)
{
    if (args[1].Equals("/PC", StringComparison.InvariantCultureIgnoreCase))
    {
        isPCVersion = true;
    }
    else
    {
        outputPath = args[1];
    }
}

if (File.Exists(inputPath))
{
    new Archive(inputPath).Export(outputPath);
}
else if (Directory.Exists(inputPath))
{
    var apf = new Archive()
    {
        IsPCVersion = isPCVersion
    };

    apf.Import(inputPath);

    Console.WriteLine("\nWriting archive...");

    apf.Write(outputPath);
}