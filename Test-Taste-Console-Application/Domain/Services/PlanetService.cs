using System;
using System.Collections.Generic;
using System.Linq;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;
        private readonly IMoonService _moonService;

        //The planets are loaded once per run; every output list reuses the same data.
        private IReadOnlyCollection<Planet> _cachedPlanets;

        public PlanetService(HttpClientService httpClientService, IMoonService moonService)
        {
            _httpClientService = httpClientService;
            _moonService = moonService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            if (_cachedPlanets != null)
            {
                return _cachedPlanets;
            }

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = _httpClientService.GetFromJson<JsonResult<PlanetDto>>(UriPath.GetAllPlanetsWithMoonsQueryParameters);
            var planetDtos = results?.Bodies ?? Enumerable.Empty<PlanetDto>();

            //All moons come from ONE bulk request. Looking them up by id replaces the old loop that sent
            //one request per moon (hundreds of requests, executed again for every output list).
            var moonsById = _moonService.GetAllMoons()
                .GroupBy(moon => moon.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            //Every planet is added, also planets without moons; the output services decide what to show.
            _cachedPlanets = planetDtos
                .Where(planetDto => planetDto != null)
                .Select(planetDto => new Planet(planetDto, ResolveMoons(planetDto, moonsById)))
                .ToList()
                .AsReadOnly();

            return _cachedPlanets;
        }

        ///<summary>
        /// Turns the moon references of a planet (name + rel URL) into full moon objects.
        ///</summary>
        private static IEnumerable<Moon> ResolveMoons(PlanetDto planetDto, IDictionary<string, Moon> moonsById)
        {
            var moons = new List<Moon>();

            foreach (var moonReference in planetDto.Moons ?? Enumerable.Empty<MoonDto>())
            {
                if (moonReference == null)
                {
                    continue;
                }

                var moonId = moonReference.URLId ?? moonReference.Id;
                if (moonId != null && moonsById.TryGetValue(moonId, out var moon))
                {
                    moons.Add(moon);
                    continue;
                }

                //The moon is kept so the count matches what the API reports for the planet, but it has no mass or temperature data.
                Logger.Instance.Warn($"{LoggerMessage.MoonNotFound}{moonId}");
                moonReference.Id = moonId;
                moons.Add(new Moon(moonReference));
            }

            return moons;
        }
    }
}
