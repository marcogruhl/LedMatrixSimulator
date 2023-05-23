using System.Diagnostics;
using System.Globalization;
using AspSample.Interfaces;
using AspSample.Repositories;
using OpenMeteo;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace AspSample;

public class MatrixService : IHostedService
{
    private static bool IsInfo { get; set; }
    private static bool WasInfo { get; set; } = true;
    private static System.Timers.Timer _timer;
    private static System.Timers.Timer _updateTime;

    const char NonBreakingSpace = ' ';

    Random r = new ();

    private static OpenMeteo.WeatherForecast? weatherData;

    private IMatrixRepository _matrix;

    private string weatherString;

    private OpenMeteoClient _openMeteoClient;

    Stopwatch runningStopwatch = new Stopwatch();

    private int selectedFace = 0;

    private bool _isDebug;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        runningStopwatch.Start();

        _isDebug = AppDomain.CurrentDomain.BaseDirectory.Contains("C:");

        _matrix = _isDebug ? new MatrixRepositorySimulator() : new MatrixRepository();

        _openMeteoClient = new OpenMeteoClient();

        UpateWeather();

        Task.Run(() =>
        {
            var oldSecond = DateTime.Now.Second;
            var oldMinute = DateTime.Now.Minute;

            while (true)
            {
                if (!IsInfo)
                {
                    switch (selectedFace)
                    {
                        case 0:
                            if(oldMinute != DateTime.Now.Minute || (_isDebug && oldSecond != DateTime.Now.Second) || WasInfo)
                                FaceTime(weatherData);
                            break;                        
                        case 1:
                            if(oldSecond != DateTime.Now.Second)
                                FaceWeather(weatherData);
                            break;
                        default:
                            break;
                    }

                    WasInfo = false;
                }                    
                
                oldSecond = DateTime.Now.Second;
                oldMinute = DateTime.Now.Minute;

                Thread.Sleep(240);
            }
        });

        // // Create a timer with a two second interval.
        _timer = new System.Timers.Timer(2000);
        // Hook up the Elapsed event for the timer. 
        _timer.Elapsed += (_, _) => IsInfo = false;
        _timer.AutoReset = false;
        _timer.Enabled = true;

        _updateTime = new System.Timers.Timer(1000 * 60 * 15);
        // Hook up the Elapsed event for the timer. 
        _updateTime.Elapsed += (_, _) => UpateWeather();
        _updateTime.AutoReset = true;
        _updateTime.Enabled = true;

        return Task.CompletedTask;
    }

    public void writeStatus(string switchStatus, string brightness)
    {
        IsInfo = WasInfo = true;
        _matrix.Clear();
        _matrix.DrawText("7x13", 1, 9, Color.FromArgb(255, 0, 0, 255), switchStatus);
        _matrix.DrawText("7x13", 1, 21, Color.FromArgb(255, 0, 255, 0), brightness);
        _matrix.DrawText("7x13", 15, 9, Color.FromArgb(255, 0, 0, 255), $"W {DateTime.Parse(weatherData.CurrentWeather.Time).ToString("t")}");
        _matrix.DrawText("5x7", 1, 32, Color.FromArgb(255, 0, 255, 0), runningStopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss"));
        _matrix.SwapOnVsync();
        _timer.Stop();
        _timer.Interval = 2_000;
        _timer.Start();
    }

    private Color?[][] _weatherIconCache;
    private float _lastWeatherCode = -1;

    public void FaceTime(OpenMeteo.WeatherForecast? weatherData)
    {
        _matrix.Clear();

        var xStart = 0;
        var yStart = 10;

        // _matrix.DrawText("8x13B", xStart + 1, yStart + 1, Color.DarkGray, DateTime.Now.ToString("HH:mm"));
        // _matrix.DrawText("8x13B", xStart, yStart, Color.OrangeRed, DateTime.Now.ToString("HH:mm"));

        // _matrix.DrawText("9x15B", xStart + 1, yStart + 1, Color.DarkGray, DateTime.Now.ToString("HH:mm"));
        // _matrix.DrawText("9x15B", xStart, yStart, Color.OrangeRed, DateTime.Now.ToString("HH:mm"));

        // _matrix.DrawPixels(GetWeatherIcon(weatherData,24,24), -3, 11);
        _matrix.DrawPixels(GetWeatherIcon(weatherData,20,20), -1, 15);

        xStart = -1;
        yStart = 13;
        _matrix.DrawText("10x20custom", xStart + 1, yStart + 1, Color.DarkGray, DateTime.Now.ToString("HH:mm"));
        _matrix.DrawText("10x20custom", xStart, yStart, Color.OrangeRed, DateTime.Now.ToString("HH:mm"));
        // _matrix.DrawText("10x20", xStart + 1, yStart + 1, Color.DarkGray, DateTime.Now.ToString("HH:mm"));
        // _matrix.DrawText("10x20", xStart, yStart, Color.OrangeRed, DateTime.Now.ToString("HH:mm"));

        // xStart = -1;
        // yStart = 22;
        // _matrix.DrawText("texgyre-27", xStart + 1, yStart + 1, Color.DarkGray, DateTime.Now.ToString("HH:mm"));
        // _matrix.DrawText("texgyre-27", xStart, yStart, Color.OrangeRed, DateTime.Now.ToString("HH:mm"));

        _matrix.DrawText("4x6custom", 47, 5, Color.White, DateTime.Now.ToString("dd.MM", CultureInfo.CurrentCulture));
        _matrix.DrawText("6x12", 53, 13, Color.MediumPurple, DateTime.Now.ToString("ddd", CultureInfo.CurrentCulture).ToUpper());
        // _matrix.DrawText("4x6c", 0, 24, Color.FromArgb(255, 80, 80, 80), DateTime.Parse(weatherData.CurrentWeather.Time).ToString("t"));

        var xActualTemperature = 12;

        // _matrix.DrawText("7x13O", xActualTemperature + 1, 25 + 1, Color.DarkGray, $"{weatherData.CurrentWeather.Temperature, 3:N0}\u00b0");
        _matrix.DrawText("7x13", xActualTemperature, 25, Color.FromArgb(255, 255, 128, 0), $"{weatherData.CurrentWeather.Temperature, 3:N0}\u00b0");
        // _matrix.DrawText("4x6custom", xActualTemperature + 7, 32, Color.DarkGray, $"{maxTemp:N0}°");
        // _matrix.DrawText("4x6custom", xActualTemperature + 14, 32, Color.DarkGray, $"{minTemp.ToString("N0").PadLeft(3,NonBreakingSpace)}°");
        // _matrix.DrawText("7x13", xActualTemperature, 25, Color.FromArgb(255, 255, 128, 0), $"{(-21), 3:N0}\u00b0");




        // maxTemp = -22f;
        // minTemp = -10f;

        maxTemp = weatherData.Daily.Temperature_2m_max[0];
        minTemp = weatherData.Daily.Temperature_2m_min[0];

        var maxShift = maxTemp < 0 ? 3 : 7;

        _matrix.DrawText("4x6custom", xActualTemperature + maxShift, 32, Color.DarkGray, $"{maxTemp:N0}°");
        _matrix.DrawText("4x6custom", xActualTemperature + 15, 32, Color.DarkGray, $"{minTemp.ToString("N0").PadLeft(3,NonBreakingSpace)}°");


        // DrawRainChart(39, 0, 63, 31, false, true);
        DrawRainChart(41, 15, 63, 31, false, true);

        _matrix.SwapOnVsync();
    }

    private Color?[][] GetWeatherIcon(OpenMeteo.WeatherForecast weatherData, int width = 16, int height = 16)
    {
        if (weatherData.CurrentWeather.Weathercode != _lastWeatherCode || _isDebug)
        {
            var path = "error_a.webp";

            switch (weatherData.CurrentWeather.Weathercode)
                // switch (61)
            {
                case 0:
                    path = "01d@2x_72.png";
                    break;
                case 1:
                    path = "01d@2x_72.png";
                    break;
                case 2:
                    path = "02d@2x_72.png";
                    break;
                case 3:
                    path = "03d@2x_72.png";
                    break;
                case 45:
                    path = "50d@2x_72.png";
                    break;
                case 48:
                    path = "50d@2x_72.png";
                    break;
                case 51:
                    path = "04d@2x_72.png";
                    break;
                case 53:
                    path = "04d@2x_72.png";
                    break;
                case 55:
                    path = "04d@2x_72.png";
                    break;
                case 61:
                    path = "10d@2x_72.png";
                    break;
                case 63:
                    path = "09d@2x_72.png";
                    break;
                case 65:
                    path = "09d@2x_72.png";
                    break;
                case 80:
                    path = "10d@2x_72.png";
                    break;
                case 81:
                    path = "09d@2x_72.png";
                    break;
                case 83:
                    path = "11d@2x_72.png";
                    break;
                case 85:
                    path = "11d@2x_72.png";
                    break;
                case 95:
                    path = "11d@2x_72.png";
                    break;
                case 96:
                    path = "11d@2x_72.png";
                    break;
                case 99:
                    path = "11d@2x_72.png";
                    break;
            }

            var size = path == "error_a.webp" ? new Size(24, 25) : new Size(width, height);
            var ri = ImageHelper.resizeImage(Image.Load<Rgba32>($"icons/{path}"), size);
            _weatherIconCache = ImageHelper.ImageToByteArray(ri);
            _lastWeatherCode = weatherData.CurrentWeather.Weathercode;
        }

        return _weatherIconCache;
    }


    public void FaceWeather(OpenMeteo.WeatherForecast? weatherData)
    {
        _matrix.Clear();

        _matrix.DrawText("4x6custom", 24, 5, Color.White, DateTime.Now.ToString("T"));
        _matrix.DrawText("4x6custom", 0, 5, Color.Gray, DateTime.Now.ToString("dd-MM", CultureInfo.CurrentCulture));
        _matrix.DrawText("4x6custom", 57, 5, Color.MediumPurple, DateTime.Now.ToString("ddd", CultureInfo.CurrentCulture).ToUpper());

        _matrix.DrawText("7x13B", 19, 18, Color.FromArgb(255, 255, 128, 0), $"{weatherData.CurrentWeather.Temperature, 3:N0}\u00b0");
        _matrix.DrawText("4x6custom", 46, 13, Color.DarkGray, $"{maxTemp.ToString("N0").PadLeft(3,NonBreakingSpace)}°");
        _matrix.DrawText("4x6custom", 46, 19, Color.DarkGray, $"{minTemp.ToString("N0").PadLeft(3,NonBreakingSpace)}°");

        _matrix.DrawPixels(GetWeatherIcon(weatherData), 5, 6);

        DrawRainChart(0, 21, 63, 31);

        _matrix.SwapOnVsync();
    }

    private Dictionary<string, (float value, Color color)> rainIntensities = new()
    {
        // { "L", (0.1f / 4, Color.Blue) },
        // { "M", (1.0f / 4, Color.Orange) },
        // { "S", (2.5f / 4, Color.Red) }
        { "L", (0.1f, Color.Blue) },
        { "M", (1.0f, Color.Orange) },
        { "S", (2.5f, Color.Red) }

    };

    private void DrawRainChart(int xStart, int yStart, int xEnd, int yEnd, bool drawScale = true, bool showForeCast = false)
    {
        var xEndOffset = drawScale ? 11 : 0;
        var xStartOffset = 5;
        var xChartStart = xStart + xStartOffset;
        var xChartEnd = xEnd - xEndOffset;
        var height = yEnd - yStart;
        var bars = xChartEnd - xChartStart;
        // double hours = (double)bars / 4;
        double hours = (double)bars;

        // var perceptionArray = weatherData.Minutely15.Precipitation;
        var perceptionArray = weatherData.Hourly.Precipitation;

        if (_isDebug)
        {
            // perceptionArray = new float?[weatherData.Minutely15.Precipitation.Length];
            // perceptionArray = r.GenerateTrend(0, 0.22, true).Take(weatherData.Minutely15.Precipitation.Length).Select(x => (float?)Convert.ToSingle(x)).ToArray(); 
        }

        // Max Line Test
        // Array.Fill(perceptionArray, 0.001f);

        // bars = 100;

        var startBar = 0;

        // for (var index = 0; index < weatherData.Minutely15.Time.Length; index++)
        for (var index = 0; index < weatherData.Hourly.Time.Length; index++)
        {
            // var s = weatherData.Minutely15.Time[index];
            var s = weatherData.Hourly.Time[index];
            startBar = index;
            if (DateTime.Parse(s) > DateTime.Now)
                break;
        }

        var maxRain = perceptionArray[startBar..(bars + startBar)].Max();

        // maxRain = 0;

        if (maxRain == 0)
        {
            if(showForeCast)
            {
                for (int i = 0; i < 2; i++)
                {
                    _matrix.DrawText("4x6custom", xStart + 2 + (i * 12), yStart+5, Color.MediumPurple, DateTime.Now.AddDays(i + 1).ToString("ddd", CultureInfo.CurrentCulture).ToUpper());
                    _matrix.DrawText("4x6custom", xStart - 2 + (i * 12), yStart+11, Color.DarkGray, $"{weatherData.Daily.Temperature_2m_max[i+1].ToString("N0").PadLeft(3,NonBreakingSpace)}°");
                    _matrix.DrawText("4x6custom", xStart - 2 +(i * 12), yStart+17, Color.DarkGray, $"{weatherData.Daily.Temperature_2m_min[i+1].ToString("N0").PadLeft(3,NonBreakingSpace)}°");
                }
            }
            else
            {
                _matrix.DrawText("4x6custom", xStart + 2, yStart + 5, Color.DarkGray, $"no rain for the");
                _matrix.DrawText("4x6custom", xStart + 2, yStart + 11, Color.DarkGray, $"next {hours}h");
            }
            
            return;
        }

        _matrix.DrawLine(xChartStart,yStart,xChartStart,yEnd,Color.DarkGray);
        // _matrix.DrawPixel(xChartStart - 1,yStart+2,Color.DarkGray);



        // _matrix.DrawText("4x6custom", xStart, yStart + 5, Color.Gray, $"M");
        // _matrix.DrawText("4x6custom", xStart, yStart + 11, Color.Gray, $"L");

        // _matrix.DrawLine(4,23,63,23,Color.Red);

        maxRain = maxRain < 1.01f / 4 ? 1.01f / 4 : maxRain; // Min Intesity to M

        var rainIntensity = "M";

        foreach (var intensity in rainIntensities)
        {
            if (intensity.Value.value < maxRain)
            {
                rainIntensity = intensity.Key;
            }
        }
        
        var intensityLine = rainIntensities[rainIntensity].value * height / maxRain;
        
        // Min Line Test
        // intensityLine = 0.000001F;
        
        if(intensityLine >= height)
            intensityLine = height;

        if(intensityLine < 0)
            intensityLine = 0;
        
        var intensityScale = yEnd - (int)intensityLine + 3;

        if(intensityScale >= yEnd + 1 )
            intensityScale = yEnd + 1;

        if(intensityScale <= yStart + 5)
            intensityScale = yStart + 5;

        _matrix.DrawText("4x6custom", xStart, intensityScale, Color.Gray, rainIntensity);
        
        if(drawScale)
            _matrix.DrawText("4x6custom", xChartEnd + 3, intensityScale, Color.Gray, (rainIntensities[rainIntensity].value * 4).ToString("0.0", CultureInfo.InvariantCulture));
        // _matrix.DrawText("4x6custom", xChartEnd + 3, yStart + 11, Color.Gray, (maxRain * 4).ToString());

        // _matrix.DrawLine(xChartEnd,yStart,xChartEnd,yEnd,Color.DarkGray);
        _matrix.DrawLine(xChartStart - 1, yEnd - (int)intensityLine, xChartEnd + (drawScale ? 1 : 0),yEnd - (int)intensityLine,Color.DarkGray);

        for (var index = 0; index < bars; index++)
        {
            var bar = perceptionArray[index + startBar];

            if(bar <= 0)
                continue;

            var y = bar * height / maxRain;

            var color = Color.Blue;

            foreach (var intensity in rainIntensities)
            {
                if (intensity.Value.value < bar)
                {
                    color = intensity.Value.color;
                }
            }

            if (y == 0 && bar > 0)
                y = 1;

            _matrix.DrawLine(xStart + 6 + index, yEnd - (int)y, xStart + 6 + index, yEnd, color);
        }
    }

    public void SetBrightness(byte id)
    {
        _matrix.SetBrightness(id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private float maxTemp;
    private float minTemp;

    // private float? maxRain;
    
    public void UpateWeather()
    {
        HourlyOptions hop = new() { new []{ HourlyOptionsParameter.temperature_2m, HourlyOptionsParameter.weathercode, HourlyOptionsParameter.precipitation} };
        // Minutely15Options mop = new() { new[] { Minutely15OptionsParameter.precipitation } };

        DailyOptions dop = new() { new[] { DailyOptionsParameter.temperature_2m_max, DailyOptionsParameter.temperature_2m_min } };

        WeatherForecastOptions options = new ()
        {
            Start_date = DateTime.Now.ToString("yyyy-MM-dd"),
            End_date = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd"),
            // Error with 15min:
            // "Cannot initialize VariableOrDerived<SurfaceAndPressureVariable<IconSurfaceVariable, IconPressureVariable>, SurfaceAndPressureVariable<IconSurfaceVariableDerived, IconPressureVariableDerived>> from invalid String value visibility for key "
            // Visibility ist das fehlerhafte element
            Hourly = hop, 
            Daily = dop, 
            Latitude = (float)52.5200, //e.g. Berlin
            Longitude = (float)13.4050, //e.g. Berlin
            // Minutely15 = mop,
            // Timezone = "Europe/Berlin,
            Timezone = "auto"
        };

        weatherData = _openMeteoClient.QueryAsync(options ).Result;

        UpateWeatherValues();
    }

    public void UpateWeatherValues()
    {
        maxTemp = weatherData.Daily.Temperature_2m_max[0];
        minTemp = weatherData.Daily.Temperature_2m_min[0];
        
        // maxRain = weatherData.Minutely15.Precipitation[..16].Max();
        //
        //
        //
        // maxRain = weatherData.Minutely15.Precipitation.Max();


        // weatherString = _openMeteoClient.WeathercodeToString((int)weatherData.CurrentWeather.Weathercode);
    }

    public async void Gif(string path = "nyan.gif", int skip = 1)
    {
        _matrix.Clear();

        Image gifImg = Image.Load(path);
        IsInfo = WasInfo = true;
        
        _timer.Stop();
        _timer.Interval = 10_000;
        _timer.Start();

        while(IsInfo)
        {
            foreach (var frame in gifImg.Frames.Cast<ImageFrame<Rgba32>>())
            {
                var gm = frame.Metadata.GetGifMetadata();

                if(!IsInfo)
                    break;

                if(gifImg.Frames.IndexOf(frame) % skip != 0)
                {
                    await Task.Delay(gm.FrameDelay * 10);
                    continue;
                }

                var img = await ImageHelper.GetImageFromFrameAsync(frame, gifImg.Frames.RootFrame as ImageFrame<Rgba32>);

                await DrawImage(img);
                await Task.Delay(gm.FrameDelay * 10);
            }
        }
    }    
    
    public async void SelectFace(int face)
    {
        selectedFace = face;
    }

    private async Task DrawImage(Image<Rgba32> image)
    {
        var ri = ImageHelper.resizeImage(image, new Size(64, 32));

        _matrix.DrawPixels(ImageHelper.ImageToByteArray(ri), 0, 0);
        _matrix.SwapOnVsync();
    }
}