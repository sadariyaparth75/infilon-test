using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Moon
    {
        public string Id { get; set; }
        public float MassValue { get; set; }
        public float MassExponent { get; set; }

        ///<summary>
        /// Average temperature in Kelvin, or null when the API has no data for this moon.
        /// Nullable on purpose: "unknown" must not be averaged in as 0 K.
        ///</summary>
        public float? AverageTemperature { get; set; }

        //The id of the body this moon orbits, as reported by the API.
        public string PlanetId { get; set; }

        public bool HasKnownTemperature => AverageTemperature.HasValue;

        public Moon(MoonDto moonDto)
        {
            Id = moonDto.Id;
            MassValue = moonDto.MassValue;
            MassExponent = moonDto.MassExponent;
            AverageTemperature = TemperatureUtility.FromApiValue(moonDto.AvgTemp);
            PlanetId = moonDto.AroundPlanetId;
        }
    }
}
