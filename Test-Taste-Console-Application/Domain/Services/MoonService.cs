using System.Collections.Generic;
using System.Linq;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class MoonService : IMoonService
    {
        private readonly HttpClientService _httpClientService;

        //The moons are loaded once per run and reused by the output lists and by the planet service.
        private IReadOnlyCollection<Moon> _cachedMoons;

        public MoonService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Moon> GetAllMoons()
        {
            if (_cachedMoons != null)
            {
                return _cachedMoons;
            }

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = _httpClientService.GetFromJson<JsonResult<MoonDto>>(UriPath.GetAllMoonsQueryParameters);

            //The JSON converter can return a null object; entries without an id can't be linked to a planet and are skipped.
            _cachedMoons = (results?.Bodies ?? Enumerable.Empty<MoonDto>())
                .Where(moonDto => moonDto != null && !string.IsNullOrWhiteSpace(moonDto.Id))
                .Select(moonDto => new Moon(moonDto))
                .ToList()
                .AsReadOnly();

            return _cachedMoons;
        }
    }
}
