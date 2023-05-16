using BdfFontParser;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;
using mColor = System.Windows.Media.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace RpiRgbLedMatrixSimulator;

internal class MatrixController
{
    internal SolidColorBrush?[,] MatrixArray;

    internal int MatrixWidth = 64;
    internal int MatrixHeight = 32;
    private readonly Func<Task> _refreshAction;

    private readonly Dictionary<string, BdfFont> fonts= new();

    public static MatrixController Instance;

    public MatrixController(int matrixWidth, int matrixHeight, Func<Task> refresh)
    {
        MatrixWidth = matrixWidth;
        MatrixHeight = matrixHeight;
        _refreshAction = refresh;

        MatrixArray = new SolidColorBrush[matrixWidth, matrixHeight];

        Instance = this;
    }

    public void AddFont(string fontName, BdfFont bdfFont)
    {
        if (!fonts.ContainsKey(fontName))
            fonts.Add(fontName, bdfFont);
    }

    public bool ContainsFont(string fieldValue) => fonts.ContainsKey(fieldValue);

    internal void DrawPixel(Color color, short x, short y)
    {
        MatrixArray[x, y] = new SolidColorBrush(ToColor(color));
    }

    internal async Task DrawImage(Image<Rgba32> image)
    {
        MatrixArray = new SolidColorBrush[MatrixWidth, MatrixHeight];

        var ri = ImageHelper.resizeImage(image, new Size(MatrixWidth, MatrixHeight));

        DrawPixels(ImageHelper.ImageToByteArray(ri), 0, 0);
        await _refreshAction.Invoke();
    }

    internal async Task DrawGif(Image gifImg)
    {
        MatrixArray = new SolidColorBrush[MatrixWidth, MatrixHeight];


        foreach (var frame in gifImg.Frames.Cast<ImageFrame<Rgba32>>())
        {
            // if (Mode != Mode.gif)
            //     continue;

            var gm = frame.Metadata.GetGifMetadata();
            var img = await ImageHelper.GetImageFromFrameAsync(frame, gifImg.Frames.RootFrame as ImageFrame<Rgba32>);

            await DrawImage(img);
            await Task.Delay(gm.FrameDelay * 10);
        }
    }

    public Color?[][] DrawLine(Point startPoint, Point endPoint, Color lineColor)
    {
        int width = Math.Max(startPoint.X, endPoint.X) + 1;
        int height = Math.Max(startPoint.Y, endPoint.Y) + 1;

        Color?[][] pixels = new Color?[width][];

        for (int i = 0; i < width; i++)
        {
            pixels[i] = new Color?[height];
        }

        int dx = Math.Abs(endPoint.X - startPoint.X);
        int dy = Math.Abs(endPoint.Y - startPoint.Y);
        int sx = startPoint.X < endPoint.X ? 1 : -1;
        int sy = startPoint.Y < endPoint.Y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            pixels[startPoint.X][startPoint.Y] = lineColor;
            if (startPoint.X == endPoint.X && startPoint.Y == endPoint.Y) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                startPoint.X += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                startPoint.Y += sy;
            }
        }

        return pixels;
    }

    internal void DrawPixels(Color?[][] pixels, int xStart, int yStart)
    {
        var y = pixels[0].Length;
        var x = pixels.Length;

        for (int line = 0; line < y; line++)
        {
            for (int pixel = 0; pixel < x; pixel++)
            {
                if(pixel+xStart >= 0 && line+yStart >= 0 && pixel+xStart <= MatrixWidth-1 && line+yStart <= MatrixHeight-1 && pixels[pixel][line] != null && pixels[pixel][line].Value.A != 0)
                    MatrixArray[pixel + xStart, line + yStart] = new SolidColorBrush(ToColor(pixels[pixel][line].Value));
            }
        }
    }

    private static mColor ToColor(Color color) => mColor.FromArgb(color.A, color.R, color.G, color.B);

    internal void DrawText(string fontName, int xStart, int yStart, Color color, string text)
    {
        var font = fonts[fontName];

        var map = font.GetMapOfString(text);
        var width = map.GetLength(0);
        var height = map.GetLength(1);

        // draw line by line
        for (int line = 0; line < height; line++)
        {
            // iterate through every bit
            for (int bit = 0; bit < width; bit++)
            {
                var charX = bit + xStart;
                var charY = line + (yStart - font.BoundingBox.Y - font.BoundingBox.OffsetY);

                if(map[bit,line] && charX >= 0 && charY >= 0 && charX <= MatrixWidth-1 && charY <= MatrixHeight-1)
                {
                    try
                    {
                        MatrixArray[charX,charY] = new SolidColorBrush(ToColor(color));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }

    internal void Clear()
    {
        MatrixArray = new SolidColorBrush[MatrixWidth, MatrixHeight];
    }

    public async Task Refresh() => await _refreshAction.Invoke();

    public void SetSize(int matrixWidth, int matrixHeight)
    {
        MatrixWidth = matrixWidth;
        MatrixHeight = matrixHeight;

        Clear();
    }

}