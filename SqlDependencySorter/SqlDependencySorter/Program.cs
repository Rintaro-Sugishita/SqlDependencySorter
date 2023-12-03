using SqlDependencySorter;

if (args.Length > 0)
{
    if (args.Contains("--g"))
    {
        new Setting().Save();
        Console.WriteLine("sample setting file is generated.");
        return;
    }
    else if (args.Contains("--help") || args.Contains("/?"))
    {
        Console.WriteLine(
            "Sort DDL files and merge a file.\n" +
            "\n" +
            "SqlDependencySorter [--g] [-c] [--help] [/?]" +
            "options: \n" +
            "          --g: generate sample setting file.\n" +
            "          --c: exit application without waiting for user key input.\n" +
            "" +
            "" +
            "");
        return;
    }

}

if (File.Exists(Setting.GetSavePath()))
{
    var file = Sorter.Run(Setting.Load());
    Console.WriteLine(file);
    Console.WriteLine($"sql file is generated.");
}
else
{
    Console.WriteLine("setting file is not found.\n" +
        "please run with --gen_setting option.");
    if (!args.Contains("--c"))
    {
        Console.ReadKey();
    }
}