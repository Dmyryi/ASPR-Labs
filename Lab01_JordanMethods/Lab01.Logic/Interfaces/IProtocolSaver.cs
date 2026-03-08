namespace Lab01.Logic.Interfaces;

public interface IProtocolSaver
{
    void Save(string content, string path = "protocol.txt");
}
