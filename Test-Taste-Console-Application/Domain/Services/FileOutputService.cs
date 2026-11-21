using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class FileOutputService : IFileOutputService
    {
        private readonly IPlanetService _planetService;

        private readonly IMoonService _moonService;

        public FileOutputService(IPlanetService planetService, IMoonService moonService)
        {
            _planetService = planetService;
            _moonService = moonService;
        }

        public void OutputAllPlanetsAndTheirMoonsToFile()
        {
            //One row per planet/moon pair, so the file can be filtered and sorted in a spreadsheet.
            //A planet without moons still gets one row, with an empty moon column.
            var header = new[] { "PlanetId", "PlanetSemiMajorAxis", "TotalMoons", "MoonId" };

            var rows = _planetService.GetAllPlanets().SelectMany(planet =>
            {
                var planetColumns = new[]
                {
                    planet.Id, FormatNumber(planet.SemiMajorAxis), planet.Moons.Count.ToString(CultureInfo.InvariantCulture)
                };

                return planet.HasMoons()
                    ? planet.Moons.Select(moon => planetColumns.Append(moon.Id))
                    : new[] { planetColumns.Append(string.Empty) };
            });

            WriteFile(PathName.AllPlanetsAndTheirMoonsFile, header, rows);
        }

        public void OutputAllMoonsAndTheirMassToFile()
        {
            var header = new[] { "MoonId", "PlanetId", "MassValue", "MassExponent" };

            var rows = _moonService.GetAllMoons().Select(moon => new[]
            {
                moon.Id, moon.PlanetId, FormatNumber(moon.MassValue), FormatNumber(moon.MassExponent)
            });

            WriteFile(PathName.AllMoonsAndTheirMassFile, header, rows);
        }

        public void OutputAllPlanetsAndTheirAverageMoonTemperatureToFile()
        {
            //Temperatures are written as plain numbers in Kelvin; an empty cell means "unknown" (not 0 K).
            var header = new[]
            {
                "PlanetId", "TotalMoons", "MoonsWithTemperatureData", "AverageMoonTemperatureKelvin",
                "PlanetAverageTemperatureKelvin"
            };

            var rows = _planetService.GetAllPlanets()
                .Where(planet => planet.HasMoons())
                .Select(planet => new[]
                {
                    planet.Id,
                    planet.Moons.Count.ToString(CultureInfo.InvariantCulture),
                    planet.MoonsWithKnownTemperatureCount.ToString(CultureInfo.InvariantCulture),
                    FormatNumber(planet.AverageMoonTemperature),
                    FormatNumber(planet.AverageTemperature)
                });

            WriteFile(PathName.AllPlanetsAndTheirAverageMoonTemperatureFile, header, rows);
        }

        private static void WriteFile(string fileName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> rows)
        {
            //The output folder is created on the first run.
            if (!Directory.Exists(PathName.PathToOutputFolder))
            {
                Directory.CreateDirectory(PathName.PathToOutputFolder);
                Console.WriteLine($"{OutputString.FolderCreated}{PathName.PathToOutputFolder}");
            }

            var filePath = Path.Combine(PathName.PathToOutputFolder, fileName);
            CsvWriter.Write(filePath, header, rows);
            Console.WriteLine($"{OutputString.FileCreated}{filePath}");
        }

        //Numbers are written with the invariant culture (decimal point), so the file is the same on every machine.
        private static string FormatNumber(float value) => value.ToString(CultureInfo.InvariantCulture);

        private static string FormatNumber(double value) => value.ToString(CultureInfo.InvariantCulture);

        private static string FormatNumber(float? value) =>
            value.HasValue ? value.Value.ToString("F1", CultureInfo.InvariantCulture) : string.Empty;
    }
}
