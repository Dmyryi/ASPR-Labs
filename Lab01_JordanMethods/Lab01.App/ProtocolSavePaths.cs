using System.IO;

namespace Lab01.App;

public static class ProtocolSavePaths
{
    public static string ResolveLab01AppProjectDirectory()
    {
        string start = AppContext.BaseDirectory;
        for (var di = new DirectoryInfo(start); di != null; di = di.Parent)
        {
            if (File.Exists(Path.Combine(di.FullName, "Lab01.App.csproj")))
                return di.FullName;
        }

        return start;
    }
}
