using System;
using System.Collections.Generic;

namespace Rino.Dtos
{
    public class WeatherDto
    {
        public int Id { get; set; }
        public DateTime DayAndDate { get; set; }
        public string ShortDatetime { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Maximum { get; set; }
        public string Minimum { get; set; }
        public string CurrentTemperature { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
        public string WindSpeed { get; set; }
        public string ImageUrl { get; set; }
        public string DayOfWeek { get; set; }
    }



    #region WeatherApi

    public class WeatherApiResponse
    {
        public string cod { get; set; }
        public int cnt { get; set; }
        public List<WeatherList> list { get; set; }
        public City city { get; set; }
    }


    public class WeatherList
    {
        // public int dt { get; set; }
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        //public Clouds clouds { get; set; }
         public Wind wind { get; set; }
        //public Sys sys { get; set; }
        public DateTime dt_txt { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
    }

    public class City
    {
        public int id { get; set; }
        public string name { get; set; }
        //  public Coord coord { get; set; }
        public string country { get; set; }
        public int timezone { get; set; }
    }
    public class Main
    {
        public double temp { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double pressure { get; set; }
        //public double sea_level { get; set; }
        //public double grnd_level { get; set; }
        public int humidity { get; set; }
        //public double temp_kf { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    #endregion


}
