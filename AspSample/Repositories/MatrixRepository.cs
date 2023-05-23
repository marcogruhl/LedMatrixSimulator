// Implementation for the matrix control unit (e.g. RaspberryPi). It needs the self-compiled librgbmatrix.so.1 and RPiRgbLEDMatrix.dll

using AspSample.Interfaces;
// using RPiRgbLEDMatrix;
using Color = System.Drawing.Color;
//
namespace AspSample.Repositories;

public class MatrixRepository : IMatrixRepository
{
     private static int _matrixWidth = 64;
     private static int _matrixHeight = 32;
//     
//     private static readonly RGBLedMatrix matrix = new(new RGBLedMatrixOptions()
//     {
//         Rows = _matrixHeight,
//         Cols = _matrixWidth,
//         ChainLength = 1,
//         HardwareMapping = "adafruit-hat-pwm",
//         Brightness = 30
//     });
//
//     private static readonly RGBLedCanvas canvas= matrix.CreateOffscreenCanvas();
//     private static readonly Dictionary<string, RGBLedFont> fonts= new();
//
//     public MatrixRepository()
//     {
//         string [] fileEntries = Directory.GetFiles("fonts", "*.bdf");
//             
//         foreach (var fileS in fileEntries)
//         {
//             var file = new FileInfo(fileS);
//             var name = file.Name.Substring(0, file.Name.Length - 4);
//             var path = $"fonts/{file.Name}";
//             var font = new RGBLedFont(path);
//             fonts.Add(name, font);
//         }
//     }
//
     public void Clear()
     {
//         canvas.Clear();
     }

     public void SwapOnVsync()
     {
//         matrix.SwapOnVsync(canvas);
     }

     public void DrawText(string font, int x, int y, Color color, string text)
     {
//         canvas.DrawText(fonts[font], x, y, new RPiRgbLEDMatrix.Color(color.R, color.G, color.B), text);
     }

     public void DrawPixel(int x, int y, Color color)
     {
//         canvas.SetPixel(x, y, new RPiRgbLEDMatrix.Color(color.R, color.G, color.B));
     }

     public void DrawLine(int xStart, int yStart, int xEnd, int yEnd, Color color)
     {
//         canvas.DrawLine(xStart,yStart,xEnd,yEnd,new RPiRgbLEDMatrix.Color(color.R, color.G, color.B));
     }

     public void SetBrightness(byte id)
     {
//         matrix.Brightness = id;
     }

     public void DrawPixels(Color?[][] pixels, int xStart, int yStart)
     {
//         var y = pixels[0].Length;
//         var x = pixels.Length;
//
//         for (int line = 0; line < y; line++)
//         {
//             for (int pixel = 0; pixel < x; pixel++)
//             {
//                 if(pixel+xStart >= 0 && line+yStart >= 0 && pixel+xStart <= _matrixWidth-1 && line+yStart <= _matrixHeight-1 && pixels[pixel][line] != null && pixels[pixel][line].Value.A != 0)
//                     canvas.SetPixel(pixel + xStart, line + yStart, new RPiRgbLEDMatrix.Color(pixels[pixel][line].Value.R, pixels[pixel][line].Value.G, pixels[pixel][line].Value.B));
//             }
//         }
     }
}