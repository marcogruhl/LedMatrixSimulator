using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LedMatrixSimulator;

public static class ConfigHelper
{
    static readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static T GetValue<T>(string propertyName, T defaultValue = default)
    {
        var filePath = $"{GetMainFolder()}{propertyName}.json";

        if (!File.Exists(filePath))
        {
            return defaultValue;
        }

        using (var isoStream = new StreamReader(filePath))
        {
            var returnValue = JsonSerializer.Deserialize<T>(isoStream.ReadToEnd(), _options);

            if (returnValue != null)
                return returnValue;

            return defaultValue;
        }
    }

    public static bool SetValue<T>(ref T outValue, T value, [CallerMemberName] string propertyName = "", Action propertyChangedAction = null)
    {
        var filePath = $"{GetMainFolder()}{propertyName}.json";

        string jsonString = JsonSerializer.Serialize(value, _options);
        File.WriteAllText(filePath, jsonString);

        var equal = (outValue != null && !outValue.Equals(value)) || (outValue == null && value != null);

        outValue = value;

        if(equal && propertyChangedAction != null)
            propertyChangedAction.Invoke();

        return equal;
    }

    public static string GetMainFolder()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory + "config" + "\\";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}