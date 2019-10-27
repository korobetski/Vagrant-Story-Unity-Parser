using System.IO;

public class FileParser
{
    private string _filePath;
    private string _fileName;
    private string _ext;
    private long _fileSize;
    public FileStream fileStream;
    public BinaryReader buffer;
    public bool UseDebug = false;
    public string FilePath { get => _filePath; set => _filePath = value; }
    public string FileName { get => _fileName; set => _fileName = value; }
    public string Ext { get => _ext; }
    public long FileSize { get => _fileSize; }

    public void PreParse(string filePath)
    {
        _filePath = filePath;
        string[] h1 = filePath.Split("/"[0]);
        string[] h2 = h1[h1.Length - 1].Split("."[0]);
        _fileName = h2[0];
        _ext = h2[1];
        fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        buffer = new BinaryReader(fileStream);
        _fileSize = buffer.BaseStream.Length;
    }
    public void PreParse(BinaryReader buffer)
    {
        this.buffer = buffer;
    }
}
