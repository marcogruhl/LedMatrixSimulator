using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace AspSample;

public static class ImageHelper
{
    public static Color?[][] ImageToByteArray(Image<Rgba32> image)
    {
        var array = new Color?[image.Width].Select(x => new Color?[image.Height].ToArray()).ToArray();

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var c = image[x, y];
                    
                if(c.A != 0)
                    array[x][y] = Color.FromArgb(c.A,c.R,c.G,c.B);
            }
        }

        return array;
    }

    internal static Image<Rgba32> resizeImage(Image<Rgba32> imgToResize, Size size)
    {
        //Get the image current width  
        int sourceWidth = imgToResize.Width;
        //Get the image current height  
        int sourceHeight = imgToResize.Height;  
        float nPercent = 0;  
        float nPercentW = 0;  
        float nPercentH = 0;  
        //Calulate  width with new desired size  
        nPercentW = ((float)size.Width / (float)sourceWidth);  
        //Calculate height with new desired size  
        nPercentH = ((float)size.Height / (float)sourceHeight);  
        if (nPercentH < nPercentW)  
            nPercent = nPercentH;  
        else  
            nPercent = nPercentW;  
        //New Width  
        int destWidth = (int)(sourceWidth * nPercent);  
        //New Height  
        int destHeight = (int)(sourceHeight * nPercent);  
        // Image<Rgba32> b = new Image<Rgba32>(destWidth, destHeight);  
        imgToResize.Mutate(x => x.Resize(destWidth, destHeight));
        return imgToResize;  
    }

    public static Color?[][] DrawLine(Point startPoint, Point endPoint, Color lineColor)
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
    
    internal static async Task<Image<Rgba32>> GetImageFromFrameAsync(ImageFrame<Rgba32> imageFrame, ImageFrame<Rgba32> rootFrame)
    {
        var image = new Image<Rgba32>(imageFrame.Width, imageFrame.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image[x, y] = imageFrame[x, y].A == 0 ? rootFrame[x, y]: imageFrame[x, y];
            }
        }

        return image;
    }
}