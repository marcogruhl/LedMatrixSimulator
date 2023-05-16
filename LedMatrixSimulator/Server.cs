using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BdfFontParser;

namespace RpiRgbLedMatrixSimulator;

internal class Server
{
    private static bool _clientConnected;
    private static bool _showClientData = true;
    private const char sep = '|';
    private const string PipeName = "MatrixSimulator";

    private static readonly BlockingCollection<string> _data = new();
    private static Task _dataHandlingTask;
    private static Task _serverHandlingTask;
    // Um Task abzubrechen. Std. damit Dispose/cancel keine null exception werfen
    private static CancellationTokenSource _cancellationTokenSource = new();

    private static MatrixController _controller = MatrixController.Instance;

    public Server()
    {
        _serverHandlingTask = Task.Factory.StartNew(StartServer, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _dataHandlingTask = Task.Factory.StartNew(DataHandling, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private static void StartServer()
    {
        var server = new NamedPipeServer(PipeName);
        server.newRequestEvent += (sender, s) => _data.Add(s);
        server.newServerInstanceCreated += (sender, s) => _data.Add($"New Server Instance created. Count: {s}");
        server.newServerInstanceEvent += (sender, s) => _data.Add($"New Client connected");
    }

    private async void DataHandling()
    {
        foreach (var data in _data.GetConsumingEnumerable())
        {
            _clientConnected = true;

            // Auto switch to server, when receiving data
            FaceController.Instance.Mode = Mode.server;

            var type = data.Split(sep)[0];

            // Type entfernen
            var values = data.Substring(data.IndexOf(sep) + 1);

            var fieldValues = new Dictionary<string, string>(values.Split(sep).ToDictionary(x => x.Split('=').First(), x => x.Split('=').Last()));

            await Application.Current.Dispatcher.Invoke(async () => 
            {
                switch (type)
                {
                    case "Clear": _controller.Clear(); break;
                    case "SwapOnVsync": await _controller.Refresh(); break;
                    case "DrawText":
                        _controller.DrawText(
                            fieldValues["font"],
                            Convert.ToInt16(fieldValues["x"]),
                            Convert.ToInt16(fieldValues["y"]),
                            Color.FromArgb(Convert.ToByte(fieldValues["colorR"]), Convert.ToByte(fieldValues["colorG"]), Convert.ToByte(fieldValues["colorB"])),
                            fieldValues["text"]
                        );
                        break;
                    case "DrawPixel":
                        _controller.DrawPixel(
                            Color.FromArgb(Convert.ToByte(fieldValues["colorR"]), Convert.ToByte(fieldValues["colorG"]), Convert.ToByte(fieldValues["colorB"])),
                            Convert.ToInt16(fieldValues["x"]),
                            Convert.ToInt16(fieldValues["y"])
                        );
                        break;
                    case "DrawPixels":

                        var pixels = JsonSerializer.Deserialize<int?[][]>(fieldValues["pixels"]);

                        var bli = new Color?[pixels.Length].Select(x => new Color?[pixels[0].Length].ToArray()).ToArray(); ;
                        for (int i = 0; i <= pixels.Length - 1; i++)
                        {
                            for (int j = 0; j <= pixels[0].Length - 1; j++)
                            {
                                bli[i][j] = pixels[i][j] == null ? null : ColorTranslator.FromWin32(pixels[i][j].Value);
                            }
                        }

                        _controller.DrawPixels(
                            bli,
                            Convert.ToInt16(fieldValues["x"]),
                            Convert.ToInt16(fieldValues["y"])
                        );
                        break;
                    case "AddFont":

                        try
                        {
                            if(!_controller.ContainsFont(fieldValues["font"]))
                            {
                                var fontLines = JsonSerializer.Deserialize<IEnumerable<string>>(fieldValues["data"]);
                                var fontName = fieldValues["font"];
                                var font = new BdfFont(fontLines);
                                _controller.AddFont(fontName, font);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                    case "SetBrightness": break;
                    default:
                        if(!_showClientData)
                            Debug.WriteLine(data);
                        break;
                }
            });
                
            if(_showClientData)
                Debug.WriteLine(data);
        }
    }
}