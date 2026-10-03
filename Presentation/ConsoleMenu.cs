using WeatherConsoleClient.Application.DTOs;
using WeatherConsoleClient.Application.Interfaces;

namespace WeatherConsoleClient.Presentation;

public class ConsoleMenu
{
    private readonly IWeatherService _weatherService;
    private readonly IWeatherFormatter _weatherFormatter;

    public ConsoleMenu(
    IWeatherService weatherService,
    IWeatherFormatter weatherFormatter)
    {
        _weatherService = weatherService;
        _weatherFormatter = weatherFormatter;
    }

    private bool _useFahrenheit;

    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DisplayMenu();

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ShowCurrentWeatherAsync(
                        cancellationToken);
                    break;

                case "2":
                    await ShowForecastAsync(
                        cancellationToken);
                    break;

                case "3":
                    await ShowDashboardAsync(
                        cancellationToken);
                    break;
                
                case "4":
                    SelectTemperatureUnit();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine(
                        "Invalid option.");

                    break;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press ENTER to continue...");

            Console.ReadLine();
        }
    }

    private static void DisplayMenu()
    {
        Console.Clear();

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "       WEATHER CONSOLE CLIENT");

        Console.WriteLine(
            "========================================");

        Console.WriteLine();

        Console.WriteLine(
            "1. Current Weather");

        Console.WriteLine(
            "2. 5-Day / 3-Hour Forecast");

        Console.WriteLine(
            "3. Weather Dashboard");

        Console.WriteLine(
            "4. Select Temperature Unit");

        Console.WriteLine(
            "0. Exit");

        Console.WriteLine();

        Console.Write(
            "Enter your choice: ");
    }

    private async Task ShowCurrentWeatherAsync(
    CancellationToken cancellationToken)
    {
        Console.WriteLine();

        Console.Write("Enter city: ");

        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine(
                "City is required.");

            return;
        }

        var weather =
            await _weatherService
                .GetCurrentWeatherAsync(
                    city,
                    cancellationToken);

        if (weather is null)
        {
            Console.WriteLine(
                "City not found.");

            return;
        }

        Console.WriteLine();

        Console.WriteLine(
            _weatherFormatter.FormatCurrentWeather(
                weather,
                _useFahrenheit));

        DisplayHotWeatherAlert(weather);
    }

    private async Task ShowForecastAsync(
    CancellationToken cancellationToken)
    {
        Console.WriteLine();

        Console.Write("Enter city: ");

        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine(
                "City is required.");

            return;
        }

        var forecast =
            await _weatherService
                .GetForecastAsync(
                    city,
                    cancellationToken);

        if (forecast is null)
        {
            Console.WriteLine(
                "City not found.");

            return;
        }

        Console.WriteLine();

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "5-DAY / 3-HOUR FORECAST");

        Console.WriteLine(
            "========================================");

        Console.WriteLine();

        Console.WriteLine(
            $"City: {forecast.City.Name}");

        Console.WriteLine();

        var itemsToDisplay = SelectForecastItems(forecast.Items);

        if (itemsToDisplay.Count == 0)
        {
            Console.WriteLine("No forecast entries found for that selection.");
            return;
        }

        DisplayRainAlert(itemsToDisplay);

        foreach (var item in itemsToDisplay)
        {
            var condition =
                item.Weather.Count > 0
                    ? item.Weather[0].Description
                    : "Unknown";

            var precipitation =
                item.ProbabilityOfPrecipitation * 100;

            Console.WriteLine(
                $"{item.DateTimeText,-20}" +
                $"{FormatTemperature(item.Main.Temperature),10}   " +
                $"{condition,-18}" +
                $"{precipitation,5:F0}%");
        }
        DisplayForecastSummary(itemsToDisplay);
    }

    private async Task ShowDashboardAsync(
    CancellationToken cancellationToken)
    {
        Console.WriteLine();

        Console.Write("Enter city: ");

        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine(
                "City is required.");

            return;
        }

        var currentWeatherTask =
            _weatherService.GetCurrentWeatherAsync(
                city,
                cancellationToken);

        var forecastTask =
            _weatherService.GetForecastAsync(
                city,
                cancellationToken);

        await Task.WhenAll(
            currentWeatherTask,
            forecastTask);

        var currentWeather =
            await currentWeatherTask;

        var forecast =
            await forecastTask;

        if (currentWeather is null ||
            forecast is null)
        {
            Console.WriteLine(
                "Unable to retrieve weather information.");

            return;
        }

        DisplayDashboard(
            currentWeather,
            forecast);
    }

    private void DisplayDashboard(
    CurrentWeatherDto currentWeather,
    ForecastDto forecast)
    {
        Console.WriteLine();

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "WEATHER DASHBOARD");

        Console.WriteLine(
            "========================================");

        Console.WriteLine();

        Console.WriteLine(
            $"City: {currentWeather.Name}");

        Console.WriteLine();

        Console.WriteLine(
            "CURRENT WEATHER");

        Console.WriteLine(
            "----------------------------------------");
        
        DisplayHotWeatherAlert(currentWeather);

        Console.WriteLine(
            $"Temperature : " +
            $"{FormatTemperature(currentWeather.Main.Temperature)}");

        Console.WriteLine(
            $"Feels Like  : " +
            $"{FormatTemperature(currentWeather.Main.FeelsLike)}");

        Console.WriteLine(
            $"Humidity    : " +
            $"{currentWeather.Main.Humidity}%");

        if (currentWeather.Weather.Count > 0)
        {
            Console.WriteLine(
                $"Condition   : " +
                $"{currentWeather.Weather[0].Description}");
        }

        Console.WriteLine();

        Console.WriteLine(
            "FORECAST");

        Console.WriteLine(
            "----------------------------------------");

        DisplayRainAlert(forecast.Items);

        foreach (var item in forecast.Items)
        {
            var condition =
                item.Weather.Count > 0
                    ? item.Weather[0].Description
                    : "Unknown";

            var precipitation =
                item.ProbabilityOfPrecipitation * 100;

            Console.WriteLine(
                $"{item.DateTimeText,-20}" +
                $"{FormatTemperature(item.Main.Temperature),10}   " +
                $"{condition,-18}" +
                $"{precipitation,4:F0}%");
        }
        DisplayForecastSummary(forecast.Items);
    }

    private void DisplayForecastSummary(
    IEnumerable<ForecastItemDto> items)
    {
        var forecastItems = items.ToList();

        if (forecastItems.Count == 0)
        {
            return;
        }

        var highestTemperature =
            forecastItems.Max(item => item.Main.Temperature);

        var lowestTemperature =
            forecastItems.Min(item => item.Main.Temperature);

        var averageTemperature =
            forecastItems.Average(item => item.Main.Temperature);

        var highestRainChance =
            forecastItems.Max(
                item => item.ProbabilityOfPrecipitation) * 100;

        Console.WriteLine();
        Console.WriteLine("FORECAST SUMMARY");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine(
            $"Highest Temperature : {FormatTemperature(highestTemperature)}");
        Console.WriteLine(
            $"Lowest Temperature  : {FormatTemperature(lowestTemperature)}");
        Console.WriteLine(
            $"Average Temperature : {FormatTemperature(averageTemperature)}");
        Console.WriteLine(
            $"Highest Rain Chance : {highestRainChance:F0} %");
    }

    private void SelectTemperatureUnit()
    {
        Console.WriteLine();
        Console.WriteLine("1. Celsius");
        Console.WriteLine("2. Fahrenheit");
        Console.Write("Choose temperature unit: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                _useFahrenheit = false;
                Console.WriteLine("Temperature unit set to Celsius.");
                break;

            case "2":
                _useFahrenheit = true;
                Console.WriteLine("Temperature unit set to Fahrenheit.");
                break;

            default:
                Console.WriteLine("Invalid temperature unit.");
                break;
        }
    }

    private string FormatTemperature(decimal celsius)
    {
        if (_useFahrenheit)
        {
            var fahrenheit = celsius * 9 / 5 + 32;
            return $"{fahrenheit:F1} °F";
        }

        return $"{celsius:F1} °C";
    }

    private static List<ForecastItemDto> SelectForecastItems(
        IEnumerable<ForecastItemDto> items)
    {
        Console.WriteLine("1. Show all forecast entries");
        Console.WriteLine("2. Show today's forecast");
        Console.WriteLine("3. Show tomorrow's forecast");
        Console.Write("Choose forecast display: ");

        var choice = Console.ReadLine();
        var forecastItems = items.ToList();

        return choice switch
        {
            "1" => forecastItems,
            "2" => forecastItems
                .Where(item => DateTime.TryParse(item.DateTimeText, out var date) &&
                               date.Date == DateTime.Today)
                .ToList(),
            "3" => forecastItems
                .Where(item => DateTime.TryParse(item.DateTimeText, out var date) &&
                               date.Date == DateTime.Today.AddDays(1))
                .ToList(),
            _ => ShowAllAfterInvalidSelection(forecastItems)
        };
    }

    private static List<ForecastItemDto> ShowAllAfterInvalidSelection(
        List<ForecastItemDto> forecastItems)
    {
        Console.WriteLine("Invalid selection. Showing all forecast entries.");
        return forecastItems;
    }

    private static void DisplayRainAlert(
    IEnumerable<ForecastItemDto> items)
    {
        if (items.Any(item =>
            item.ProbabilityOfPrecipitation >= 0.60m))
        {
            Console.WriteLine();
            Console.WriteLine(
                "RAIN ALERT: High probability of precipitation.");
            Console.WriteLine();
        }
    }

    private static void DisplayHotWeatherAlert(
    CurrentWeatherDto weather)
    {
        if (weather.Main.Temperature > 35m)
        {
            Console.WriteLine();
            Console.WriteLine("WEATHER ALERT:");
            Console.WriteLine("High temperature detected.");
            Console.WriteLine();
        }
    }
}
