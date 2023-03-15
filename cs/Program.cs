using System.Text.Json;
using OpenCvSharp;

using var cts = new CancellationTokenSource();
cts.Token.Register(() => Console.WriteLine("cancellation invoked."));
Console.CancelKeyPress += (s, e) => cts.Cancel();

var w = 640;
var h = 480;
using var frame = new Mat(h, w, MatType.CV_8UC3);
using var cap = new VideoCapture() { FrameWidth = w, FrameHeight = h, Fps = 30 };
cap.Open(0);

var exeFile = "C:/Users/husty/AppData/Local/Programs/Python/Python310/python.exe";
var entryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../py/main.py");
using var pipe = new PipeServer(exeFile, entryFile, w, h);
try
{
    while (cap.Read(frame) && !cts.IsCancellationRequested)
    {
        unsafe { pipe.Write(new Span<byte>(frame.DataPointer, frame.Width * frame.Height * 3)); }
        var json = pipe.Read();
        var results = JsonSerializer.Deserialize<DnnResult[]>(json);
        // unpack the results.
        foreach (var r in results.Select(r => r.AsBox()))
        {
            Cv2.Rectangle(frame, r.Value, Scalar.Black, 2);
            Cv2.PutText(frame, r.Key, r.Value.TopLeft, HersheyFonts.HersheyPlain, 2, Scalar.Black, 2);
        }
        Cv2.ImShow(" ", frame);
        Cv2.WaitKey(1);
    }
    Console.WriteLine("Successfully completed.");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
finally
{
    Console.WriteLine("Exit process.");
}