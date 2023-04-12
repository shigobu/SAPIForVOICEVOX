#r "System.Console"
#r "System.Runtime"

using System.IO;

public int main()
{
    if (Args.Count != 2)
    {
        Console.WriteLine("Invalid argument.");
        Console.WriteLine("[0] search directory");
        Console.WriteLine("[1] version");
        return -1;
    }
    string searchDirectory = Args[0];
    string varsionStr = Args[1];

    if (!Directory.Exists(searchDirectory))
    {
        Console.WriteLine("検索フォルダがありません。");
        return -1;
    }

    if (!Version.TryParse(varsionStr, out Version temp))
    {
        Console.WriteLine("バージョン文字列が不正です。");
        return -1;
    }

    List<string> files = new List<string>();
    files.AddRange(Directory.GetFiles(searchDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories));
    files.AddRange(Directory.GetFiles(searchDirectory, "*.vdproj", SearchOption.AllDirectories));

    foreach (var file in files)
    {
        string fileData = "";
        using (StreamReader reader = new StreamReader(file, Encoding.UTF8))
        {
            fileData = reader.ReadToEnd();
        }

        fileData = fileData.Replace("99.99.999", varsionStr);

        using (StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
        {
            writer.Write(fileData);
        }
    }

    Console.WriteLine("成功");
    return 0;
}

return main();
