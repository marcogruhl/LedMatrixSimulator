using Color = System.Drawing.Color;

namespace AspSample.Interfaces;

public interface IMatrixRepository
{
    void Clear();
    void SwapOnVsync();
    void DrawText(string font, int x, int y, Color color, string text);
    void SetBrightness(byte id);
    void DrawPixels(Color?[][] pixels, int x, int y);
    void DrawPixel(int x, int y, Color color);
    void DrawLine(int xStart, int yStart, int xEnd, int yEnd, Color color);
}