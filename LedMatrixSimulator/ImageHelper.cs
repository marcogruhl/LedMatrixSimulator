using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace RpiRgbLedMatrixSimulator;

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

    // static void DrawLine(int x0, int y0, int x1, int y1, Color color) {
    //     int dy = y1 - y0, dx = x1 - x0, gradient, x, y, shift = 0x10;

        // if (abs(dx) > abs(dy)) {
        //     // x variation is bigger than y variation
        //     if (x1 < x0) {
        //         std::swap(x0, x1);
        //         std::swap(y0, y1);
        //     }
        //     gradient = (dy << shift) / dx ;
        //
        //     for (x = x0 , y = 0x8000 + (y0 << shift); x <= x1; ++x, y += gradient) {
        //         c->SetPixel(x, y >> shift, color.r, color.g, color.b);
        //     }
        // } else if (dy != 0) {
        //     // y variation is bigger than x variation
        //     if (y1 < y0) {
        //         std::swap(x0, x1);
        //         std::swap(y0, y1);
        //     }
        //     gradient = (dx << shift) / dy;
        //     for (y = y0 , x = 0x8000 + (x0 << shift); y <= y1; ++y, x += gradient) {
        //         c->SetPixel(x >> shift, y, color.r, color.g, color.b);
        //     }
        // } else {
        //     c->SetPixel(x0, y0, color.r, color.g, color.b);
        // }
    // }

    // internal static Color?[][] drawLine(int xStart, int yStart, int xEnd, int yEnd, Color color)
    // {
    //     var width = Math.Abs(xStart - xEnd) + 1;
    //     var height = Math.Abs(yStart - yEnd) + 1;
    //     
    //     var image = new Image<Rgba32>(width, height);
    //
    //     // var startX0 = xStart < xEnd ? xEnd - xStart : xStart - xEnd;
    //     
    //     image.Mutate(x => x.DrawLines(
    //         SixLabors.ImageSharp.Color.FromRgba(color.R, color.G, color.B, color.A), 
    //         1,
    //         new PointF[] {
    //             new Vector2(0,0),
    //             new Vector2(width, height)
    //         }));
    //
    //
    //     return (ImageToByteArray(image));
    // }

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
