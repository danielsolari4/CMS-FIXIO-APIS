using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers.Core;
using Rino.Utils.Configuration;
using Rino.Utils.Extensions;
using Rino.Utils.Helpers;

namespace Rino.Managers
{
    public interface IWeatherManager : IManager<WeatherDto>
    {
        Task<List<WeatherDto>> GetByCity(string city);
        Task<List<string>> GetAllowedCities();
    }

    public class WeatherManager : IWeatherManager
    {
        private readonly AppSettings _appSettings;

        public WeatherManager(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        public async Task<List<WeatherDto>> GetByCity(string city)
        {
            var allowedCities = _appSettings.Weather.AllowCities.Split(";");
            var allowedCitiesIds = _appSettings.Weather.AllowCitiesIds.Split(";");

            if (!allowedCities.Any(s => s.Contains(city, StringComparison.OrdinalIgnoreCase)))
                return null;

            var cityIndex = Array.FindIndex(allowedCities, t => t.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0);
            int cityId = Convert.ToInt32(allowedCitiesIds.ToList()[cityIndex]);

            string url = string.Format(_appSettings.Weather.Url, cityId, _appSettings.Weather.Key);
            var w = await HttpHelper.GetPageAsync(url, "application/json");
            if (string.IsNullOrEmpty(w))
                return null;

            var arraytempMax = new double[6];
            var arraytempMin = new double[6];

            double tempMax = 0;
            double tempMin = 99999999;

            var weather = JsonConvert.DeserializeObject<WeatherApiResponse>(w);

            var currentW = weather.list.DistinctBy(x => x.dt_txt.Date)?.ToList();

            var mismoDia = weather.list[0].dt_txt.Day;

            var index = 0;
            foreach (var item in weather.list)
            {
                if (item.dt_txt.Day == mismoDia)
                {
                    if (item.main.temp_min < tempMin)
                    {
                        tempMin = item.main.temp_min;

                    }

                    if (item.main.temp_max > tempMax)
                    {
                        tempMax = item.main.temp_max;

                    }
                    arraytempMin[index] = tempMin;
                    arraytempMax[index] = tempMax;
                }
                else
                {
                    index++;
                    mismoDia = item.dt_txt.Day;
                    tempMax = 0;
                    tempMin = 99999999;
                }
            }

            var i = 0;
            foreach (var item in currentW)
            {
                item.main.temp_min = arraytempMin[i];
                item.main.temp_max = arraytempMax[i];
                i++;
            }

            return currentW.Select(x => MapToDto(x, _appSettings.Weather.ImageUrl)).ToList();
        }

        public async Task<List<string>> GetAllowedCities()
        {
            var allowedCities = _appSettings.Weather.AllowCities?.Split(';')?.ToList();
            return allowedCities;
        }

        public Task<WeatherDto> Add(WeatherDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }

        public Task Delete(WeatherDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<WeatherDto>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }
        public Task<WeatherDto> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(WeatherDto entity)
        {
            throw new NotImplementedException();
        }



        private WeatherDto MapToDto(WeatherList weather, string imageUrl)
        {
            var culture = new System.Globalization.CultureInfo("es-ES");

            return new WeatherDto()
            {
                Id = weather.weather.FirstOrDefault().id,
                Icon = weather.weather.FirstOrDefault()?.icon,
                CurrentTemperature = weather.main.temp.ToString("0.0"),
                DayAndDate = weather.dt_txt,
                Description = weather.weather.FirstOrDefault().description,
                Maximum = weather.main.temp_max.ToString("0.0"),
                Minimum = weather.main.temp_min.ToString("0.0"),
                Humidity = weather.main.humidity.ToString("0.0"),
                Pressure = weather.main.pressure.ToString("0.0"),
                WindSpeed = $"{weather.wind.speed.ToString("0.0")} km/h",
                ImageUrl = string.Format(imageUrl, weather.weather.FirstOrDefault().icon),
                ShortDatetime = $"{culture.DateTimeFormat.GetDayName(weather.dt_txt.DayOfWeek)?.ToUpper()} {weather.dt_txt.Day} {culture.DateTimeFormat.GetAbbreviatedMonthName(weather.dt_txt.Month)?.ToUpper()} {weather.dt_txt.Year}",
                DayOfWeek = culture.DateTimeFormat.GetDayName(weather.dt_txt.DayOfWeek)?.ToUpper()
            };
        }
    }
}
