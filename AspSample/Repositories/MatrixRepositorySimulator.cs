using System.Collections.Concurrent;
using System.Drawing;
using System.Text.Json;
using AspSample.Interfaces;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace AspSample.Repositories;

public class MatrixRepositorySimulator : IMatrixRepository
{
    private static readonly BlockingCollection<string> _data = new BlockingCollection<string>();
    private const string PipeName = "MatrixSimulator";
    private static bool _showClientData = false;

    private static Task _dataHandlingTask;
    // Um Task abzubrechen. Std. damit Dispose/cancel keine null exception werfen
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private static List<string> fontList = new();

    private static void DataHandling()
    {
        using (var client = new NamedPipeClient(PipeName))
        {
            client.ClientConnected += (sender, args) => fontList.Clear();

            foreach (var data in _data.GetConsumingEnumerable())
            {

                if(_showClientData)
                    Console.WriteLine(data);

                try
                {
                    if(client.ServerStarted)    
                        client.SendData(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }    

    private const char sep = '|';

    public MatrixRepositorySimulator()
    {
        var clientId = Path.GetRandomFileName().Replace(".", "");
        _dataHandlingTask = Task.Factory.StartNew(DataHandling, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        System.Timers.Timer timer = new System.Timers.Timer(1000);
        timer.Elapsed += (sender, args2) => _data.Add($"Tick{sep}{clientId}{sep}{DateTime.Now}"); ;
        timer.Start();
    }

    public void Clear()
    {
        _data.Add($"Clear{sep}");
    }

    public void SwapOnVsync()
    {
        _data.Add($"SwapOnVsync{sep}");
    }

    public void DrawText(string font, int x, int y, Color color, string text)
    {
        if (!fontList.Contains(font))
        {
            IEnumerable<string> lines = File.ReadLines($"fonts/{font}.bdf");
            _data.Add($"AddFont{sep}font={font}{sep}data={JsonSerializer.Serialize(lines)}");
            fontList.Add(font);
        }
        _data.Add($"DrawText{sep}font={font}{sep}x={x}{sep}y={y}{sep}colorR={color.R}{sep}colorG={color.G}{sep}colorB={color.B}{sep}text={text}");
    }

    public void DrawPixels(Color?[][] pixels, int xStart, int yStart)
    {
        var bli = new int?[pixels.Length].Select(x => new int?[pixels[0].Length].ToArray()).ToArray();;
        for (int i = 0; i <= pixels.Length - 1; i++)
        {
            for (int j = 0; j <= pixels[0].Length - 1; j++)
            {
                bli[i][j] = pixels[i][j] == null ? null : ColorTranslator.ToWin32(pixels[i][j].Value);
            }
        }
        _data.Add($"DrawPixels{sep}|pixels={JsonSerializer.Serialize(bli)}|x={xStart}|y={yStart}");
    }
    public void DrawPixel(int x, int y, Color color)
    {
        _data.Add($"DrawPixel{sep}x={x}{sep}y={y}{sep}colorR={color.R}{sep}colorG={color.G}{sep}colorB={color.B}");
    }

    public void DrawLine(int xStart, int yStart, int xEnd, int yEnd, Color color)
    {
        var pm = ImageHelper.DrawLine(new Point(xStart, yStart), new Point(xEnd, yEnd), color);
        DrawPixels(pm,0,0);
    }


    public void SetBrightness(byte id)
    {
        _data.Add($"SetBrightness|value={id}");
    }
}