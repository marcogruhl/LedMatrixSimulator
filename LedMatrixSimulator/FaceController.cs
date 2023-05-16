using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;
using mColor = System.Windows.Media.Color;
using Point = System.Drawing.Point;

namespace RpiRgbLedMatrixSimulator;

internal class FaceController
{
    private static MatrixController _controller = MatrixController.Instance;
    Random r = new ();
    public Mode Mode { get; set; }
    public static FaceController Instance;

    public FaceController()
    {
        Instance = this;
        ShowTime();
    }

    #region faces

    internal async Task DrawGif(string path)
    { 
        Image gifImg = Image.Load(path);

        while (Mode == Mode.gif)
        {
            await _controller.DrawGif(gifImg);
        }
    }

    public async Task DrawImage(string path)
    {
        while (Mode == Mode.image)
        {
            await _controller.DrawImage(Image.Load<Rgba32>(path));
            await Task.Delay(2000);
        }
    }

    internal async Task SampleRandom()
    {
        while (Mode == Mode.random)
        {
            for (int y = 0; y < _controller.MatrixHeight; y++) {
                for (int x = 0; x < _controller.MatrixWidth; x++) {
                    _controller.MatrixArray[x, y] = new SolidColorBrush(mColor.FromArgb(255, (byte)r.Next(254), (byte)r.Next(254), (byte)r.Next(254)));
                }
            }
            
            await _controller.Refresh();
            await Task.Delay(1000/10);
        }
    }

    internal async Task ShowTime()
    {
        while (Mode == Mode.time)
        {
            _controller.Clear();
            _controller.DrawText("7x13B", 0, 9, Color.FromArgb(255, 128, 0), DateTime.Now.ToString("T"));
            _controller.DrawText("7x13B", 0, 20, Color.FromArgb(255, 128, 0), DateTime.Now.ToString("M", CultureInfo.CurrentCulture));
            
            var size = new Size(32, 32);
            var ri = ImageHelper.resizeImage(Image.Load<Rgba32>($"icons/11d@2x_72.png"), size);
            _controller.DrawPixels(ImageHelper.ImageToByteArray(ri), 36, 4);

            var picture = new Color?[100].Select(x => new Color?[100].ToArray()).ToArray();

            picture[0][0] = Color.Aqua;
            picture[0][1] = Color.Red;
            picture[1][0] = Color.LimeGreen;
            picture[1][1] = Color.Yellow;
            picture[99][99] = Color.Yellow;
            picture[10][3] = Color.Yellow;
            picture[61][3] = Color.Yellow;

            _controller.DrawPixels(picture, 2, 28);

            // DrawText(fonts["7x13B"], 0, 32, new RPiRgbLEDMatrix.Color(255, 128, 0), "WAITING FOR CLIENT");
                
            await _controller.Refresh();
            await Task.Delay(1000/1);
        }
    }

    internal async Task ShowLine()
    {
        while (Mode == Mode.line)
        {
            _controller.Clear();

            _controller.DrawPixels(_controller.DrawLine(new Point(0, 0), new Point(0,31), Color.Aqua), 0,0);
            _controller.DrawPixels(_controller.DrawLine(new Point(0, 0), new Point(63,0), Color.Red), 0,0);
            _controller.DrawPixels(_controller.DrawLine(new Point(5, 5), new Point(5,5), Color.White), 0,0);
            _controller.DrawPixels(_controller.DrawLine(new Point(10, 10), new Point(15,15), Color.Blue), 0,0);
            _controller.DrawPixels(_controller.DrawLine(new Point(3, 3), new Point(61,5), Color.GreenYellow), 0,0);

            _controller.DrawPixels(_controller.DrawLine(new Point(50, 7), new Point(8,8), Color.GreenYellow), 0,0);
            
            await _controller.Refresh();
            await Task.Delay(1000/1);
        }
    }

    #endregion faces
}

public enum Mode
{
    time,
    image,
    gif,
    random,
    server,
    line
}