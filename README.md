# TenForce Test and Taste – Solar System console app

A .NET console application that reads data from the [Solar System OpenData API](https://api.le-systeme-solaire.net/en/) and writes these lists to the console and to CSV files:

1. The planets that have at least one moon, with the **average temperature of their moons**.
2. All moons and their mass.
3. All planets and their moons.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (the project targets `net10.0`)
- A free API key from https://api.le-systeme-solaire.net/generatekey.html

## Setting the API key

Since version 2.0.0 the API requires an API key on every request. The key is **not** stored in the repository. The application reads the `SOLAR_SYSTEM_API_KEY` setting from three places. If it is set in more than one, the first one in this list wins:

1. The environment variable `SOLAR_SYSTEM_API_KEY`
2. .NET user secrets (key `SOLAR_SYSTEM_API_KEY`)
3. `Test-Taste-Console-Application/appsettings.json`, which is excluded from git

**Option A – appsettings.json (simplest):**

Copy the template and put your key in it:

```powershell
# PowerShell
Copy-Item Test-Taste-Console-Application/appsettings.example.json Test-Taste-Console-Application/appsettings.json
```

```bash
# bash
cp Test-Taste-Console-Application/appsettings.example.json Test-Taste-Console-Application/appsettings.json
```

```json
{
  "SOLAR_SYSTEM_API_KEY": "<YOUR_API_KEY>"
}
```

`appsettings.json` is listed in `.gitignore`, so the key is never committed. Only the template `appsettings.example.json` is in the repository.

**Option B – user secrets (outside the project folder):**

```bash
dotnet user-secrets set SOLAR_SYSTEM_API_KEY <YOUR_API_KEY> --project Test-Taste-Console-Application
```

User secrets are stored in your user profile (`%APPDATA%\Microsoft\UserSecrets\` on Windows, `~/.microsoft/usersecrets/` on Linux/macOS), outside the repository.

**Option C – environment variable (for a single session or CI):**

```powershell
# PowerShell
$env:SOLAR_SYSTEM_API_KEY = "<YOUR_API_KEY>"
```

```bash
# bash
export SOLAR_SYSTEM_API_KEY="<YOUR_API_KEY>"
```

If no key is found, the application stops with an instruction. If the API rejects the key (HTTP 401/403), that is reported as well.

## Running

```bash
dotnet run --project Test-Taste-Console-Application
```

The lists are printed to the console and written as CSV files to a `FileOutput` folder in the current directory (it is created if needed and excluded from git):

| File | Content |
|---|---|
| `AllPlanetsAndTheirAverageMoonTemperature.csv` | Planets with at least one moon, moon count, moons with temperature data, average moon temperature and planet temperature (Kelvin; an empty cell means unknown) |
| `AllMoonsAndTheirMass.csv` | Every moon, the body it orbits, and its mass |
| `AllPlanetsAndTheirMoons.csv` | One row per planet/moon pair (planets without moons have an empty moon column) |

The run is split into three steps: loading the data, writing to the screen, and writing the files. If loading fails, the run stops. A failure while writing to the screen does not prevent the files from being written. Any failure sets exit code 1.

## Design decisions

### Domain model

| Change | Why |
|---|---|
| `Moon.AverageTemperature` (`float?`, Kelvin) and `Moon.PlanetId` added | The system had no notion of temperature. The value is **nullable** because the API reports `avgTemp = 0` when the temperature is unknown. 0 K is physically impossible, so it is treated as "no data" rather than a real value. |
| `Planet.AverageMoonGravity` replaced by `Planet.AverageMoonTemperature` (`float?`) | The old property was a stub that always returned `0`. The new one averages only the moons with a known temperature (unknown values would drag the average to 0) and returns `null` when no moon has data. |
| `Planet.MoonsWithKnownTemperatureCount` and `Planet.AverageTemperature` added | An average is only meaningful if you know how many values it is based on. The planet's own temperature is shown as a reference. |
| `Planet` now receives its moons via the constructor | Moons are loaded in a separate bulk request (see below), so the planet no longer depends on the DTO carrying fully loaded moons. |

### API interface

| Change | Why |
|---|---|
| One bulk request for all moons (`filter[]=bodyType,eq,Moon`) with `avgTemp` and `aroundPlanet` | The original code sent **one request per moon** (400+ requests, repeated for every output list). The app now needs **2 requests in total**. Planet → moon references are resolved in memory by id. |
| `bodyType,eq,Moon` instead of `aroundPlanet,gt,null` | The old filter silently returned only 336 of the 479 moons. |
| Nested fields (`massValue`, `planet`) listed explicitly in `data=` | The API returns empty nested objects otherwise. |
| `avgTemp` added to the planet request | Used as reference value in the output. |
| Data is loaded once and cached in the (singleton) services | Every output list reuses the same data. |
| Central `HttpClientService.GetFromJson<T>` with error handling | Non-success responses now raise a clear error instead of silently returning an empty list, which looked like "no planets found". A rejected key (401/403) gets a dedicated message. |
| `Authorization: Bearer <key>` header from configuration | Required since API v2.0.0. The key is never stored in source code. |

### Bugs fixed

- The original project did not compile: `Program.cs` contained `$""{...}""` in the `Debug.WriteLine` call.
- The original sent no `Authorization` header, so since API v2.0.0 every request was rejected and the lists came out empty.
- The semi-major axis was always `0`. The `data=` field names are case-sensitive, and the API field is `semimajorAxis`, not `semiMajorAxis`. It is now also a `double`, because a `float` cannot hold values like 4,498,396,441 km exactly.
- The file output described in the assignment was missing. `FileOutputService` now writes the three lists as CSV.
- The average temperature list now only shows planets that have at least one moon.
- The log4net configuration is resolved next to the executable, so logging also works when the app is started from another folder.
- Retargeted from the unsupported `net5.0` to `net10.0` (LTS), with up-to-date packages. The build has 0 warnings.

## A note on the data

At the time of writing, the API returns `avgTemp = 0` (unknown) for **every** moon. The average moon temperature is therefore shown as `n/a`, with the number of moons that have data (`0`), plus an explanatory note. As soon as the dataset contains moon temperatures, the averages appear without any code change. Planets do have temperatures; these are shown in a separate column.

## Project structure

```
Test-Taste-Console-Application/
├── Configuration/        API settings (API key resolution)
├── Constants/            URIs, output strings, messages
├── Domain/
│   ├── DataTransferObjects/   Shapes of the API responses (data access)
│   ├── Objects/               Domain objects: Planet, Moon (data objects)
│   └── Services/              HTTP access, planet/moon services, screen and file output (data handling)
└── Utilities/            Console writer, CSV writer, logger, temperature helpers
```
