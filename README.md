# Solar System Console App

A .NET console application that reads data from the [Solar System OpenData API](https://api.le-systeme-solaire.net/en/) and writes three lists to the console and to CSV files:

1. **Planets with moons and the average temperature of their moons** (new in this exercise)
2. All moons and their mass
3. All planets and their moons

Built for the TenForce "Test and Taste" developer exercise, which extends an existing project.

## Contents

- [Getting started](#getting-started)
- [Configuring the API key](#configuring-the-api-key)
- [Output](#output)
- [How it works](#how-it-works)
- [Design decisions](#design-decisions)
- [Changes compared to the original project](#changes-compared-to-the-original-project)
- [Known limitations](#known-limitations)
- [Project structure](#project-structure)

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A free API key from the [API key page](https://api.le-systeme-solaire.net/generatekey.html). The page is in French; use the "En" switch for English.

### Run

```bash
git clone https://github.com/sadariyaparth75/infilon-test.git
cd infilon-test

# 1. Add your API key (see the next section for other options)
cp Test-Taste-Console-Application/appsettings.example.json Test-Taste-Console-Application/appsettings.json
#    then replace <YOUR_API_KEY> in appsettings.json with your key

# 2. Run
dotnet run --project Test-Taste-Console-Application
```

In Visual Studio, open `Test-Taste-Console-Application.sln` in the repository root, add the key as described above, and press F5.

## Configuring the API key

Since version 2.0.0 the API rejects requests without an API key. The key is never stored in source code or in the repository. The application reads the setting `SOLAR_SYSTEM_API_KEY` from three places. If it is set in more than one, the first one in this list wins:

| Priority | Source | Typical use |
|---|---|---|
| 1 | Environment variable `SOLAR_SYSTEM_API_KEY` | A single session, CI |
| 2 | .NET user secrets | Developer machine, outside the project folder |
| 3 | `Test-Taste-Console-Application/appsettings.json` | Simplest local setup |

### Option A: appsettings.json

Copy the template and put your key in it.

PowerShell:

```powershell
Copy-Item Test-Taste-Console-Application/appsettings.example.json Test-Taste-Console-Application/appsettings.json
```

bash:

```bash
cp Test-Taste-Console-Application/appsettings.example.json Test-Taste-Console-Application/appsettings.json
```

```json
{
  "SOLAR_SYSTEM_API_KEY": "<YOUR_API_KEY>"
}
```

`appsettings.json` is listed in `.gitignore`, so the key is never committed. Only the template `appsettings.example.json` is part of the repository.

### Option B: user secrets

```bash
dotnet user-secrets set SOLAR_SYSTEM_API_KEY <YOUR_API_KEY> --project Test-Taste-Console-Application
```

User secrets are stored in your user profile (`%APPDATA%\Microsoft\UserSecrets\` on Windows, `~/.microsoft/usersecrets/` on Linux and macOS).

### Option C: environment variable

PowerShell:

```powershell
$env:SOLAR_SYSTEM_API_KEY = "<YOUR_API_KEY>"
```

bash:

```bash
export SOLAR_SYSTEM_API_KEY="<YOUR_API_KEY>"
```

### When the key is missing or wrong

- **No key found:** the application stops before calling the API, explains how to set the key, and exits with code 1.
- **Key rejected** (HTTP 401 for a missing key, 403 for an invalid one): the application reports that the key was rejected and exits with code 1.

## Output

### Console

The application reports each step while it runs:

```
Loading data from the Solar System OpenData API...
Data loaded: 8 planets and 479 moons.

Writing the planets with moons and their average moon temperature...
--------------------+---------------+------------------------------+------------------------------+------------------------------
Planet's Id         |Total Moons    |Moons With Temperature Data   |Average Moon Temperature      |Planet's Average Temperature
--------------------+---------------+------------------------------+------------------------------+------------------------------
Uranus              |29             |0                             |n/a                           |76.0 K / -197.1 C
Neptune             |16             |0                             |n/a                           |55.0 K / -218.1 C
Jupiter             |115            |0                             |n/a                           |165.0 K / -108.1 C
Mars                |2              |0                             |n/a                           |210.0 K / -63.1 C
Saturne             |293            |0                             |n/a                           |134.0 K / -139.1 C
Terre               |1              |0                             |n/a                           |288.0 K / 14.9 C
--------------------+---------------+------------------------------+------------------------------+------------------------------
Note: the API reports no temperature (avgTemp = 0 K) for any of these moons, so their average is shown as n/a.

Writing all moons and their mass...
Writing all planets and their moons...
Writing the lists to CSV files...
Done.
```

Planet ids are the API's own (French) ids, for example `terre` for Earth. Mercury and Venus are not in the first list because they have no moons. The `n/a` values are explained under [Known limitations](#known-limitations).

### CSV files

The same lists are written to a `FileOutput` folder in the current directory. The folder is created when needed and is excluded from git.

| File | Content |
|---|---|
| `AllPlanetsAndTheirAverageMoonTemperature.csv` | Planets with at least one moon: moon count, moons with temperature data, average moon temperature and the planet's own temperature, in Kelvin. An empty cell means unknown. |
| `AllMoonsAndTheirMass.csv` | Every moon, the body it orbits, and its mass |
| `AllPlanetsAndTheirMoons.csv` | One row per planet and moon pair. Planets without moons have one row with an empty moon column. |

The files are UTF-8 with a byte order mark, so spreadsheet tools show accented names correctly. Numbers always use a decimal point.

## How it works

```
Program.cs              configuration, dependency injection, the three steps
   |
ScreenOutputService     FileOutputService            output
   |
PlanetService           MoonService                  data handling (cached per run)
   |
HttpClientService                                    data access (API key header, errors)
   |
api.le-systeme-solaire.net
```

A run has three steps:

1. **Load the data.** Two API requests in total: one for the planets, one for all moons. If loading fails, the run stops, because there is nothing to write.
2. **Write to the console.** A failure here is reported but does not stop step 3.
3. **Write the CSV files.**

Any failure is logged with log4net and sets exit code 1.

## Design decisions

### Domain model

| Change | Why |
|---|---|
| `Moon.AverageTemperature` (`float?`, Kelvin) and `Moon.PlanetId` added | The system had no notion of temperature. The value is nullable because the API reports `avgTemp = 0` when the temperature is unknown. 0 K (absolute zero) is physically impossible, so it is treated as "no data" rather than as a real value. |
| `Planet.AverageMoonGravity` replaced by `Planet.AverageMoonTemperature` (`float?`) | The old property was a stub that always returned `0`. The new one averages only the moons with a known temperature, because unknown values would drag the average towards 0. It returns `null` when no moon has data. |
| `Planet.MoonsWithKnownTemperatureCount` and `Planet.AverageTemperature` added | An average is only meaningful if you know how many values it is based on. The planet's own temperature is shown as a reference. |
| `Planet` receives its moons through the constructor | Moons are loaded in a separate bulk request, so a planet no longer depends on its DTO carrying fully loaded moons. |

### API interface

| Change | Why |
|---|---|
| One bulk request for all moons, including `avgTemp` and `aroundPlanet` | The original code sent one request per moon (456 moons), and repeated that for each list that reads planets: about 915 requests per run. The application now needs 2. Planets and moons are matched in memory by id. |
| Filter `bodyType,eq,Moon` instead of `aroundPlanet,gt,null` | The old filter silently returned only 336 of the 479 moons. |
| Nested fields (`massValue`, `planet`) listed explicitly in `data=` | Otherwise the API returns empty nested objects. |
| `semimajorAxis` instead of `semiMajorAxis` | Field names in `data=` are case-sensitive. With the wrong spelling the value was always `0`. |
| `avgTemp` added to the planet request | Shown as a reference value next to the moon average. |
| Data loaded once per run and cached in the (singleton) services | All lists use the same data, and the API is not called again for each list. |
| One `HttpClientService.GetFromJson<T>` helper with error handling | A failed request raises a clear error instead of silently returning an empty list that looked like "no planets found". |
| `Authorization: Bearer <key>` header, key read from configuration | Required since API v2.0.0. |

## Changes compared to the original project

- **Build fixed.** `Program.cs` did not compile because of `$""{...}""` in a `Debug.WriteLine` call.
- **API key added.** The original sent no `Authorization` header, so every request was rejected and every list came out empty.
- **New list** of planets with moons and the average temperature of their moons.
- **File output added.** The assignment describes file output, and the original already contained the CSV file names, but the output itself was missing.
- **Semi-major axis fixed.** It was always `0` because of the field name casing. It is now a `double`, because a `float` cannot hold values like 4,498,396,441 km exactly.
- **All moons loaded.** 479 instead of 336.
- **Progress messages and error handling** for every step.
- **Logging configuration** is read from the application folder, so logging also works when the application is started from another folder.
- **Retargeted** from the unsupported `net5.0` to `net10.0` (LTS), with up-to-date packages. The build has no warnings.

## Known limitations

- **No moon temperatures in the dataset.** At the time of writing the API returns `avgTemp = 0` (unknown) for every moon, so the average moon temperature is `n/a` for every planet. The output says so explicitly, and shows the planet's own temperature separately instead of substituting it. When the dataset gains moon temperatures, the averages appear without any code change.
- **Unknown masses are shown as 0.** The API has no mass for 275 of the 479 moons. The moons and mass list shows those as `0`, as the original project did.
- **Key press at the end.** The application waits for a key press after `Done.` so the console window stays open. When input is redirected (for example in scripts or CI) this step fails after all output has been written.
- **Synchronous HTTP calls.** With only two requests per run, async code throughout every layer added little. It is the first thing to change if the number of calls grows.

## Project structure

```
.
├── Test-Taste-Console-Application.sln
├── docs/
│   └── presentation.html          Slides for the project walkthrough and demo
└── Test-Taste-Console-Application/
    ├── Program.cs                 Entry point: configuration, DI, the three steps
    ├── appsettings.example.json   Template for the git-ignored appsettings.json
    ├── Configuration/             API key resolution (ApiSettings)
    ├── Constants/                 URIs, output strings, messages
    ├── Domain/
    │   ├── DataTransferObjects/   Shapes of the API responses (data access)
    │   ├── Objects/               Planet and Moon (data objects)
    │   └── Services/              HTTP access, planet and moon services, console and file output (data handling)
    └── Utilities/                 Console writer, CSV writer, logger, temperature helpers
```

The presentation is a single HTML file: open `docs/presentation.html` in a browser and use the arrow keys. Press N for speaker notes.
