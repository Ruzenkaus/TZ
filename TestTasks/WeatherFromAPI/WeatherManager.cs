using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TestTasks.WeatherFromAPI.Models;

namespace TestTasks.WeatherFromAPI
{
    public class WeatherManager
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "";
        private const string GeocodingUrl = "http://api.openweathermap.org/geo/1.0/direct";
        private const string ForecastUrl = "https://api.openweathermap.org/data/2.5/forecast";

        public WeatherManager()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherComparisonResult> CompareWeather(string cityA, string cityB, int dayCount)
        {
            if (dayCount < 1 || dayCount > 5)
                throw new ArgumentException("Day count must be between 1 and 5.");

            var coordsA = await GetCoordinates(cityA);
            var coordsB = await GetCoordinates(cityB);

            var weatherDataA = await GetWeatherData(coordsA);
            var weatherDataB = await GetWeatherData(coordsB);

            var dailyWeatherA = GroupWeatherDataByDay(weatherDataA, dayCount);
            var dailyWeatherB = GroupWeatherDataByDay(weatherDataB, dayCount);

            int warmerDays = 0, rainierDays = 0;

            for (int i = 0; i < dayCount; i++)
            {
                double avgTempA = dailyWeatherA[i].Average(w => w.Temp);
                double avgTempB = dailyWeatherB[i].Average(w => w.Temp);

                double totalRainA = dailyWeatherA[i].Sum(w => w.Rain);
                double totalRainB = dailyWeatherB[i].Sum(w => w.Rain);

                if (avgTempA > avgTempB) warmerDays++;
                if (totalRainA > totalRainB) rainierDays++;
            }

            return new WeatherComparisonResult(cityA, cityB, warmerDays, rainierDays);
        }

        private async Task<(double lat, double lon)> GetCoordinates(string city)
        {
            var response = await _httpClient.GetAsync($"{GeocodingUrl}?q={city}&limit=1&appid={_apiKey}");
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException($"City '{city}' not found.");

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<GeoLocation>>(json);

            if (data == null || !data.Any())
                throw new ArgumentException($"City '{city}' not found.");

            return (data[0].Lat, data[0].Lon);
        }

        private async Task<List<WeatherData>> GetWeatherData((double lat, double lon) coords)
        {
            var response = await _httpClient.GetAsync($"{ForecastUrl}?lat={coords.lat}&lon={coords.lon}&appid={_apiKey}&units=metric");

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new InvalidOperationException("API rate limit exceeded.");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var forecast = JsonSerializer.Deserialize<WeatherForecastResponse>(json);

            return forecast.List.Select(f => new WeatherData
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(f.Dt).DateTime,
                Temp = f.Main.Temp,
                Rain = f.Rain?.Volume ?? 0
            }).ToList();
        }

        private List<List<WeatherData>> GroupWeatherDataByDay(List<WeatherData> data, int dayCount)
        {
            var groupedData = data.GroupBy(w => w.DateTime.Date)
                                  .Take(dayCount)
                                  .Select(g => g.ToList())
                                  .ToList();

            if (groupedData.Count < dayCount)
                throw new ArgumentException("Insufficient forecast data for the specified day count.");

            return groupedData;
        }
    }

    public class GeoLocation
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class WeatherForecastResponse
    {
        public List<ForecastEntry> List { get; set; }
    }

    public class ForecastEntry
    {
        public long Dt { get; set; }
        public MainData Main { get; set; }
        public RainData Rain { get; set; }
    }

    public class MainData
    {
        public double Temp { get; set; }
    }

    public class RainData
    {
        public double Volume { get; set; }
    }

    public class WeatherData
    {
        public DateTime DateTime { get; set; }
        public double Temp { get; set; }
        public double Rain { get; set; }
    }
}
