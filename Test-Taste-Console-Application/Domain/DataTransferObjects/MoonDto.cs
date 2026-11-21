using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;

namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    //The converter is needed for this DTO. Because of the converter, all the properties need to get the JsonProperty annotation. 
    [Newtonsoft.Json.JsonConverter(typeof(JsonPathConverter))]
    public class MoonDto
    {
        [JsonProperty("id")] public string Id { get; set; }

        //The property moon is used to set the Id property. 
        [JsonProperty("moon")]
        public string Moon
        {
            get => Id;
            set => Id = value;
        }

        //The path of the specific moon
        [JsonProperty("rel")] public string Rel { get; set; }

        //The id at the end of the rel URL (e.g. ".../bodies/lune" -> "lune"). Null when the rel isn't part of the response.
        public string URLId { get => Rel?.TrimEnd('/').Split('/').Last(); }

        //The path to the nested property is created by using a dot.
        [JsonProperty("mass.massValue")] public float MassValue { get; set; }
        [JsonProperty("mass.massExponent")] public float MassExponent { get; set; }

        //Average temperature in Kelvin. The API uses 0 when the temperature is unknown.
        [JsonProperty("avgTemp")] public float AvgTemp { get; set; }

        //The id of the planet (or dwarf planet / asteroid) the moon orbits.
        [JsonProperty("aroundPlanet.planet")] public string AroundPlanetId { get; set; }
    }
}
