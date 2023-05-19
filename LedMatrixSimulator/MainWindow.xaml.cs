using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using BdfFontParser;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using mColor = System.Windows.Media.Color;
using System.Threading;
using LedMatrixSimulator;

namespace RpiRgbLedMatrixSimulator;

public partial class MainWindow : INotifyPropertyChanged
{
    #region properties

    private static int _matrixWidth = ConfigHelper.GetValue(nameof(MatrixWidth), 64);

    public int MatrixWidth
    {
        get => _matrixWidth;
        set
        {
            if (ConfigHelper.SetValue(ref _matrixWidth, value))
            {
                _matrixController.SetSize(_matrixWidth, _matrixHeight);
                BuildShapes();
            }
        }
    }

    private static int _matrixHeight = ConfigHelper.GetValue(nameof(MatrixHeight), 32);

    public int MatrixHeight
    {
        get => _matrixHeight;
        set
        {
            if (ConfigHelper.SetValue(ref _matrixHeight, value))
            {
                _matrixController.SetSize(_matrixWidth, _matrixHeight);
                BuildShapes();
            }
        }
    }    
    
    private bool _lockRatio = ConfigHelper.GetValue(nameof(LockRatio), true);

    public bool LockRatio
    {
        get => _lockRatio;
        set
        {
            if (ConfigHelper.SetValue(ref _lockRatio, value))
                ResizeShapes();
        }
    }

    private bool _shape = ConfigHelper.GetValue(nameof(Shape), false);

    public bool Shape
    {
        get => _shape;
        set
        {
            if (ConfigHelper.SetValue(ref _shape, value))
                BuildShapes();
        }
    }

    private double _size = ConfigHelper.GetValue(nameof(Size), 77);

    public double Size
    {
        get => _size;
        set
        {
            if (ConfigHelper.SetValue(ref _size, value))
                ResizeShapes();
        }
    }
    
    #endregion properties

    private static bool _isInitialized;
    private static MatrixController _matrixController;
    private readonly FaceController _faceController;
    private static mColor _backgroundLedColor = mColor.FromRgb(30,30,30);
    private static SolidColorBrush backgroundLedBrush = new SolidColorBrush(_backgroundLedColor);
    internal Shape?[,] Matrix;

    // [DllImport("user32.dll")]
    //
    // public static extern bool LockWindowUpdate(IntPtr hWndLock);

    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();

        this.SizeChanged += (_, _) => ResizeShapes();

        _matrixController = new MatrixController(_matrixWidth, _matrixHeight, Refresh);
        BuildShapes();
        
        // sample font
        _matrixController.AddFont("7x13B", new BdfFont("fonts/7x13B.bdf"));
        _faceController = new FaceController();
        var server = new Server();

        // simple performance test
        // Task.Factory.StartNew(() =>
        // {
        //     var lastSecond = 0;
        //     var x = 0;
        //     var disp = Application.Current.Dispatcher;
        //
        //     while (true)
        //     {
        //         if (DateTime.Now.Second != lastSecond)
        //         {
        //             Debug.WriteLine($"Count: {x}");
        //             x = 0;
        //             lastSecond = DateTime.Now.Second;
        //         }
        //
        //         disp.Invoke(() => Refresh());
        //         x++;
        //     
        //     }
        // });
    }

    #region Matric Canvas
    
    // private async Task Refresh()
    private async Task BuildShapes()
    {
        // using (Dispatcher.DisableProcessing())
        // {
            _isInitialized = false;

            Matrix = new Shape[_matrixWidth, _matrixHeight];

            MatrixCanvas.Children.Clear();

            for (int x = 0; x < _matrixWidth; x++)
            {
                for (int y = 0; y < _matrixHeight; y++)
                {
                    Shape shape = !_shape ? new Ellipse() : new Rectangle();
                    MatrixCanvas.Children.Add(shape);
                    Matrix[x, y] = shape;
                }
            }

            ResizeShapes();
        // }

        Refresh();
    }

    private async Task ResizeShapes()
    {
        _isInitialized = false;
        
        var xRatio = MatrixCanvas.ActualWidth / _matrixWidth;
        var yRatio = MatrixCanvas.ActualHeight / _matrixHeight;

        var ratio = Math.Min(xRatio, yRatio);

        if (LockRatio)
        {
            xRatio = ratio;
            yRatio = ratio;
        }

        for (int x = 0; x < _matrixWidth; x++)
        {
            for (int y = 0; y < _matrixHeight; y++)
            {
                var shape = Matrix[x,y];

                shape.SetValue(Canvas.LeftProperty, x * xRatio);
                shape.SetValue(Canvas.TopProperty, y * yRatio);

                shape.Width = xRatio - xRatio * (1 - Size/100);
                shape.Height = yRatio - yRatio * (1 - Size/100);
            }
        }

        _isInitialized = true;
    }

    // ~5000fps vs ~30 on full rebuild
    private async Task Refresh()
    {
        if(!_isInitialized)
            return;

        for (int x = 0; x < _matrixWidth; x++)
        {
            for (int y = 0; y < _matrixHeight; y++)
            {
                Matrix[x,y].Fill = _matrixController.MatrixArray[x, y] ?? backgroundLedBrush;
            }
        }
    }

    #endregion Matrix Canvas

    #region commands

    private void OnClick_Text(object sender, RoutedEventArgs e)
    {
        _faceController.Mode = Mode.time;
        _faceController.ShowTime();
    }

    private void OnClick_Line(object sender, RoutedEventArgs e)
    {
        _faceController.Mode = Mode.line;
        _faceController.ShowLine();
    }

    private void OnClick_Image(object sender, RoutedEventArgs e)
    {
        _faceController.Mode = Mode.image;
        _faceController.DrawImage("NmJgg.jpg");
    }
    private void OnClick_Gif(object sender, RoutedEventArgs e)
    {
        _faceController.Mode = Mode.gif;
        _faceController.DrawGif("nyan.gif");
    }
    private void OnClick_Random(object sender, RoutedEventArgs e)
    {
        _faceController.Mode = Mode.random;
        _faceController.SampleRandom();
    }

    #endregion commands

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion property changed
}