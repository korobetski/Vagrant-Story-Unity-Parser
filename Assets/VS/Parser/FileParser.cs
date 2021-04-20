using System.IO;

public class FileParser
{
    private string _filePath;
    private string _fileName = "";
    private string _ext;
    private long _fileSize;
    private FileStream _fileStream;
    public BinaryReader buffer;

    public string FilePath { get => _filePath; set => _filePath = value; }
    public string FileName { get => _fileName; set => _fileName = value; }
    public string Ext { get => _ext; }
    public long FileSize { get => _fileSize; }

    public void Read(string filePath)
    {
        _filePath = filePath;
        string[] h1 = filePath.Split("/"[0]);
        string[] h2 = h1[h1.Length - 1].Split("."[0]);
        _fileName = h2[0];
        _ext = h2[1];
        _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        buffer = new BinaryReader(_fileStream);
        _fileSize = buffer.BaseStream.Length;
    }

    public void Close()
    {
        buffer.Close();
        _fileStream.Close();
    }
}
