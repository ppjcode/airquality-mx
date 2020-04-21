using System.Linq;
using Latincoder.AirQuality.Model.DTO;

namespace Latincoder.AirQuality.Services
{
    internal enum Quality : int {
            Good = 50,
            Moderate = 100,
            UnhealthySensitive = 150,
            Unhealthy = 200,
            Hazardous = int.MaxValue
    }

    /// <summary>
    /// This object function is to format notifications to supported delivery
    /// channels.
    /// Current scope:
    /// utility static methods for supported channels
    ///
    /// Next steps:
    /// Create configurable object for which given a set of conditions
    /// it may format notifications as needed
    /// </summary>
    public class NotificationFormatter
    {
        private const int TwitterCharLimit = 280;

        private NotificationFormatter() {}

        // TODO: needs strong/strict validation
        public static string GetTwitterMessageSpanish(CityFeed feed) {
            var charactersLeft = TwitterCharLimit;
            var attributions = from attr in feed.MaxAqiStation.Attributions
                               let attributionMsg = attr.GetNameOrDefault()
                               select attributionMsg;
            var attrText = string.Concat("Fuente:\n",
                attributions.Aggregate((prev, current) => string.Concat(prev, "\n", current)));
            // how many characters do we have left?
            charactersLeft -= attrText.Length;
            if (charactersLeft < 200) {
                var custom = getQuality(feed.MaxAQI) == Quality.Good ? "Es momento de respirar" : "Mantenga sus precauciones";
                return $"{custom} en {feed.CityName} calidad del aire es {getScaleSpanish(feed.MaxAQI)}(Indice Calidad:{feed.MaxAQI}) reportado por la estacion {feed.MaxAqiStation.Name})\n{attrText}";
            }

            if (charactersLeft < 150) {
                return $"En {feed.CityName} calidad del aire es {getScaleSpanish(feed.MaxAQI)}(Indice Calidad:{feed.MaxAQI}) (estacion {feed.MaxAqiStation.Name})\n{attrText}";
            }

            // attempt to send this message
            return $"En {feed.CityName} calidad del aire es {feed.MaxAQI} (estacion {feed.MaxAqiStation.Name})\n{attrText}";
        }

        public static string GetSimpleMessage(CityFeed feed) {
            var attributionText = (from attr in feed.MaxAqiStation.Attributions
                    let attributionMsg = attr.ToString()
                    select attributionMsg)
                    .Aggregate((prev, current) => string.Concat(prev, "\n", current));
            return $"En {feed.CityName} calidad del aire es {getScaleSpanish(feed.MaxAQI)}(Indice Calidad:{feed.MaxAQI}) reportado por la estacion {feed.MaxAqiStation.Name})\n{attributionText}";
        }

        // Helper methods

        // TODO: find more elegant implementation
        // NOTE: enums in C# seems limited compared to Enums in Java, find a better solution
        private static Quality getQuality(int aqi) {
            if (aqi <= (int)Quality.Good) return Quality.Good;
            if (aqi <= (int)Quality.Moderate) return Quality.Moderate;
            if (aqi <= (int)Quality.UnhealthySensitive) return Quality.UnhealthySensitive;
            if (aqi <= (int)Quality.Unhealthy) return Quality.Unhealthy;
            return Quality.Hazardous;
        }
        private static string getScaleSpanish(int aqi) {
            var quality = getQuality(aqi);
            switch(quality) {
                case Quality.Good:
                return "BUENA";
                case Quality.Moderate:
                return "MODERADA";
                case Quality.Unhealthy:
                case Quality.UnhealthySensitive:
                return "NO SALUDABLE";
                default:
                return "PELIGROSA";
            }
        }
    }
}
