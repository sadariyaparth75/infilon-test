using System.Globalization;
using Test_Taste_Console_Application.Constants;

namespace Test_Taste_Console_Application.Utilities
{
    public static class TemperatureUtility
    {
        private const float KelvinToCelsiusOffset = 273.15f;

        ///<summary>
        /// Converts the API's avgTemp value to a nullable temperature. The API reports 0 K when it has no data;
        /// 0 K (absolute zero) is physically impossible, so any value that isn't above 0 is treated as unknown.
        ///</summary>
        public static float? FromApiValue(float kelvin)
        {
            return kelvin > 0 ? kelvin : (float?)null;
        }

        ///<summary>
        /// Formats a temperature as "288.0 K / 14.9 C", or "n/a" when it is unknown.
        ///</summary>
        public static string Format(float? kelvin)
        {
            if (!kelvin.HasValue)
            {
                return OutputString.NotAvailable;
            }

            var celsius = kelvin.Value - KelvinToCelsiusOffset;
            return string.Format(CultureInfo.InvariantCulture, "{0:F1} K / {1:F1} C", kelvin.Value, celsius);
        }
    }
}
