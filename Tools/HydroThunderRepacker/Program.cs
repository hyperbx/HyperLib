using HyperLib.Formats.Barracuda;

Console.WriteLine
(
    "Hydro Thunder Repacker\n" +
    "Written by Hyper\n"
);

if (args.Length <= 0)
{
    Console.WriteLine("Usage: HydroThunderRepacker.exe \"Base.apf\" [opt: outputDir]");
    Console.WriteLine("       HydroThunderRepacker.exe \"Base\" [opt: outputFile] [opt: /PC]");
    Console.WriteLine("       HydroThunderRepacker.exe \"JsonBinary.0|4.bin\" [opt: outputFile] [opt: /PC]");
    Console.WriteLine("       HydroThunderRepacker.exe \"JsonBinary.0|4.json\" [opt: outputFile] [opt: /PC]\n");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

var isPCVersion = false;
var isCustomOutputDir = false;

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
        isCustomOutputDir = true;
    }
}

if (File.Exists(inputPath))
{
    switch (Path.GetExtension(inputPath))
    {
        case ".apf":
            new Archive(inputPath).Export(outputPath);
            break;

        case ".bin":
            new JsonBinary(inputPath).Export(isCustomOutputDir ? outputPath : outputPath + ".json");
            break;

        case ".json":
        {
            var ajb = new JsonBinary();
            ajb.Import(inputPath);
            ajb.Write(isCustomOutputDir ? outputPath : outputPath + ".bin");

            break;
        }
    }
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