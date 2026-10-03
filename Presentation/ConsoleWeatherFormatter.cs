using System.Text;
using WeatherConsoleClient.Application.DTOs;
using WeatherConsoleClient.Application.Interfaces;

namespace WeatherConsoleClient.Presentation;

public class ConsoleWeatherFormatter : IWeatherFormatter
{
    public string FormatCurrentWeather(
        CurrentWeatherDto weather,
        bool useFahrenheit)
    {
        var condition =
            weather.Weather.Count > 0
                ? weather.Weather[0].Description
                : "Unknown";

        return
            "========================================" +
            Environment.NewLine +
            "CURRENT WEATHER" +
            Environment.NewLine +
            "========================================" +
            Environment.NewLine +
            Environment.NewLine +
            $"City        : {weather.Name}" +
            Environment.NewLine +
            $"Temperature : {FormatTemperature(weather.Main.Temperature, useFahrenheit)}" +
            Environment.NewLine +
            $"Feels Like  : {FormatTemperature(weather.Main.FeelsLike, useFahrenheit)}" +
            Environment.NewLine +
            $"Condition   : {condition}" +
            Environment.NewLine +
            $"Humidity    : {weather.Main.Humidity} %" +
            Environment.NewLine +
            $"Pressure    : {weather.Main.Pressure} hPa" +
            Environment.NewLine +
            $"Wind Speed  : {weather.Wind.Speed:F1} m/s";
    }

    public string FormatForecast(
        ForecastDto forecast)
    {
        var output = new StringBuilder();

        output.AppendLine(
            "========================================");

        output.AppendLine(
            "5-DAY / 3-HOUR FORECAST");

        output.AppendLine(
            "========================================");

        output.AppendLine();

        output.AppendLine(
            $"City: {forecast.City.Name}");

        output.AppendLine();

        output.AppendLine(
            "Date & Time          Temp      Condition          Rain");

        output.AppendLine(
            "--------------------------------------------------------");

        foreach (var item in forecast.Items)
        {
            var condition =
                item.Weather.Count > 0
                    ? item.Weather[0].Description
                    : "Unknown";

            var precipitation =
                item.ProbabilityOfPrecipitation * 100;

            output.AppendLine(
                $"{item.DateTimeText,-20}" +
                $"{item.Main.Temperature,7:F1} °C   " +
                $"{condition,-18}" +
                $"{precipitation,5:F0}%");
        }

        return output.ToString();
    }

    private static string FormatTemperature(
        decimal celsius,
        bool useFahrenheit)
    {
        if (useFahrenheit)
        {
            var fahrenheit = celsius * 9 / 5 + 32;
            return $"{fahrenheit:F1} °F";
        }

        return $"{celsius:F1} °C";
    }
}
