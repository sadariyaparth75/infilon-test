using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    public class PlanetDto
    {
        public string Id { get; set; }
        //Semi-major axis in km. A double, because a float can't hold values like 4498396441 exactly.
        //Mapped explicitly because the API spells it "semimajorAxis".
        [JsonProperty("semimajorAxis")] public double SemiMajorAxis { get; set; }

        //Average temperature in Kelvin. The API uses 0 when the temperature is unknown.
        public float AvgTemp { get; set; }
        public ICollection<MoonDto> Moons { get; set; }
    }
}