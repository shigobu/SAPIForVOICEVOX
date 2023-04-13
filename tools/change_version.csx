#r "System.Console"
#r "System.Runtime"

using System.IO;
using System.Text.RegularExpressions;

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

    //フォルダの存在確認
    if (!Directory.Exists(searchDirectory))
    {
        Console.WriteLine("検索フォルダがありません。");
        return -1;
    }

    //バージョン文字列の確認
    if (!Version.TryParse(varsionStr, out Version temp))
    {
        Console.WriteLine("バージョン文字列が不正です。");
        return -1;
    }

    //AssemblyInfo.csのバージョン文字列置換
    string[] csFiles = Directory.GetFiles(searchDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories).ToArray();
    foreach (var file in csFiles)
    {
        ReplaceVersion(file, varsionStr);
    }

    //インストーラのバージョン置換。同時にProductCodeとPackageCodeの更新も必要
    string[] vdprojFiles = Directory.GetFiles(searchDirectory, "*.vdproj", SearchOption.AllDirectories).ToArray();
    foreach (var file in vdprojFiles)
    {
        ReplaceVersion(file, varsionStr);
        SetNewProductCode(file);
    }

    Console.WriteLine("成功");
    return 0;
}

/// <summary>
/// バージョンを置換します。
/// </summary>
/// <param name="fileName">ファイル名</param>
/// <param name="version">バージョンを表す文字列</param>
public void ReplaceVersion(string fileName, string version)
{
    string fileData = "";
    using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
    {
        fileData = reader.ReadToEnd();
    }

    fileData = fileData.Replace("99.99.999", version);

    using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
    {
        writer.Write(fileData);
    }
}

/// <summary>
/// vdprojファイルのProductCodeとPackageCodeを新しいものに更新します。
/// </summary>
/// <param name="fileName">更新対象のファイル名</param>
public void SetNewProductCode(string fileName)
{
    string fileData = "";
    using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
    {
        fileData = reader.ReadToEnd();
    }

    fileData = SetNewCode(fileData, "ProductCode");
    fileData = SetNewCode(fileData, "PackageCode");

    using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
    {
        writer.Write(fileData);
    }
}

/// <summary>
/// 指定のコードを更新します。
/// </summary>
/// <param name="input">vdprojファイルの中身</param>
/// <param name="codeName">対象のコード名</param>
/// <returns>コードが更新された文字列</returns>
public string SetNewCode(string input, string codeName)
{
    //コード名をダブルコーテーションで囲う
    string code = "\"" + codeName + "\"";
    string matchPattern = code + " = \"8:{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}}\"";
    string replacePattern = code + " = \"8:" + Guid.NewGuid().ToString("B").ToUpper() + "\"";    //B書式指定子は、ハイフン区切りの中かっこ囲み
    return Regex.Replace(input, matchPattern, replacePattern);
}

return main();
