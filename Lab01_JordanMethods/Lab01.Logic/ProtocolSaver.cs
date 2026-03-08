namespace Lab01.Logic;

public class ProtocolSaver : Interfaces.IProtocolSaver
{
    public void Save(string content, string path = "protocol.txt")
    {
        File.WriteAllText(path, content);
    }
}
