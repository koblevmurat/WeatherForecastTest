using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace weather
{
    public class WeatherForecast
    {
        public List<dailyInfo> daily { get; set; }
    }

    public class dailyInfo
    {

        public double dt { get; set; }
        public double pressure { get; set; }

        public dailyTemp temp { get; set; }
    }

    public class dailyTemp
    {
        public double morn { get; set; }
        public double eve { get; set; }

    }

    struct cityInfo
    {
        public string name;
        public string lat, lon;
    }
    class Program
    {
        static string myWeatherForecastTemplate = "https://api.openweathermap.org/data/2.5/onecall?lat={0}8&lon={1}&units=metric&exclude=minutely,hourly,alerts&appid=6f4ea9a763d0a92a58fa7e89b14f40c2";

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        private static String getReq(string req)
        {
            StringBuilder sb_respo = new StringBuilder();
            WebRequest request = WebRequest.Create(req);
            WebResponse response = request.GetResponse();
            Stream ReceiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            while (count > 0)
            {
                String str = new String(read, 0, count);
                sb_respo.Append(str);
                count = readStream.Read(read, 0, 256);
            }
            readStream.Close();
            response.Close();
            return sb_respo.ToString();
        }
        static void Main(string[] args)
        {
            cityInfo myCity = new cityInfo();
            myCity.name = "Черкесск";
            myCity.lat = "42.0578";
            myCity.lon = "44.2233";            
            var jsonString = getReq(String.Format(myWeatherForecastTemplate, myCity.lat, myCity.lon));
            try
            {
                WeatherForecast weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(jsonString);
                double maxPressure = 0;
                double maxTempDiff = 0;
                DateTime maxPressureDay = new DateTime();
                DateTime maxTempDiffDay = new DateTime();
                foreach (var dailyForecast in weatherForecast.daily)
                {
                    if (dailyForecast.pressure > maxPressure)
                    {
                        maxPressure = dailyForecast.pressure;
                        //тут можно было бы запонить индекс, чтобы каждый раз не преобразовывать, но уже ладно.. 
                        maxPressureDay = UnixTimeStampToDateTime(dailyForecast.dt);
                        
                    }
                    if (System.Math.Abs(dailyForecast.temp.eve - dailyForecast.temp.morn) > maxTempDiff)
                    {
                        maxTempDiff = Math.Round( System.Math.Abs(dailyForecast.temp.eve - dailyForecast.temp.morn), 2 );
                        maxTempDiffDay = UnixTimeStampToDateTime(dailyForecast.dt);
                    }
                }
                Console.WriteLine(String.Format("max pressure: {0} at {1}", maxPressure, maxPressureDay.Date));
                Console.WriteLine(String.Format("max temperature difference: {0} at {1}", maxTempDiff, maxTempDiffDay.Date));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
