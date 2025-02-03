
using System.Threading.Tasks;
using Xunit;
using TestTasks.WeatherFromAPI;
namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task CompareWeather_ValidCities_ReturnsResult()
        {
            var manager = new WeatherManager();
            var result = await manager.CompareWeather("kyiv,ua", "lviv,ua", 3);

            Assert.NotNull(result);
            Assert.Equal("kyiv,ua", result.CityA);
            Assert.Equal("lviv,ua", result.CityB);
            Assert.InRange(result.WarmerDaysCount, 0, 3);
            Assert.InRange(result.RainierDaysCount, 0, 3);
        }

        [Fact]
        public async Task CompareWeather_InvalidCity_ThrowsArgumentException()
        {
            var manager = new WeatherManager();

            await Assert.ThrowsAsync<ArgumentException>(() => manager.CompareWeather("invalidcity,ua", "kyiv,ua", 3));
        }

        [Fact]
        public async Task CompareWeather_InvalidDayCount_ThrowsArgumentException()
        {
            var manager = new WeatherManager();

            await Assert.ThrowsAsync<ArgumentException>(() => manager.CompareWeather("kyiv,ua", "lviv,ua", 6));
        }
    }
}