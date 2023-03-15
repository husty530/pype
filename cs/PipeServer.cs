using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

public sealed class PipeServer : IDisposable
{

    private readonly NamedPipeServerStream _pipe;
    private readonly Process _python;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;

    public PipeServer(string exeFile, string entryFile, params object[] args)
    {
        var pipeName = Guid.NewGuid().ToString();
        _pipe = new(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
        _python = new();
        _python.StartInfo.FileName = exeFile;
        _python.StartInfo.Arguments = $"{entryFile} {pipeName} {string.Join(" ", args)}";
        // using following two lines, you can hide python console output.
        //_python.StartInfo.UseShellExecute = false;
        //_python.StartInfo.CreateNoWindow = true;
        _python.Start();
        _pipe.WaitForConnection();
        _writer = new(_pipe);
        _reader = new(_pipe);
    }

    public void Dispose()
    {
        _reader.Dispose();
        _writer.Dispose();
        _python.Dispose();
        _pipe.Dispose();
    }

    public bool Write(ReadOnlySpan<byte> data)
    {
        try
        {
            _writer.Write(data.Length);
            _writer.Write(data);
        }
        catch { return false; }
        return true;
    }

    public string? Read()
    {
        var buf = new byte[4096];
        while (true)
        {
            var size = _reader.Read(buf, 0, buf.Length);
            if (size > 0)
                return Encoding.UTF8.GetString(buf.AsSpan(0, size));
        }
    }

}
