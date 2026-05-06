using Lab01.Logic.Interfaces;

namespace Lab01.Logic;

public sealed class ProtocolSaver : IProtocolSaver
{
    public void Save(string content, string path = "protocol.txt")
        => File.WriteAllText(path, content);
}
