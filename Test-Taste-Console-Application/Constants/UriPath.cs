namespace Test_Taste_Console_Application.Constants
{
    public static class UriPath
    {
        public const string BaseUri = "https://api.le-systeme-solaire.net";
        private const string BodiesUri = "/rest/bodies";

        //The planets, their own average temperature and the references (id + rel) to their moons.
        //The data= field names are case-sensitive: the API field is "semimajorAxis" (the former "semiMajorAxis" always returned nothing, so 0).
        public const string GetAllPlanetsWithMoonsQueryParameters =
            BodiesUri + "?data=id,semimajorAxis,avgTemp,moons,moon,rel&filter[]=isPlanet,neq,false";

        //All moons in ONE request, including mass, temperature and the planet they orbit.
        //Nested fields (massValue, planet) must be listed explicitly, otherwise the API returns empty objects.
        //The filter bodyType=Moon is used because "aroundPlanet,gt,null" silently misses about 140 moons.
        public const string GetAllMoonsQueryParameters = BodiesUri +
            "?data=id,mass,massValue,massExponent,avgTemp,aroundPlanet,planet&filter[]=bodyType,eq,Moon";
    }
}
