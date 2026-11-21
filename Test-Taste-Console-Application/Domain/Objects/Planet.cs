using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Planet
    {
        public string Id { get; set; }
        //Semi-major axis of the orbit in km.
        public double SemiMajorAxis { get; set; }
        public ICollection<Moon> Moons { get; set; }

        ///<summary>
        /// The planet's own average temperature in Kelvin, or null when unknown.
        ///</summary>
        public float? AverageTemperature { get; set; }

        ///<summary>
        /// How many of the planet's moons have a known temperature, so the average can be put in context.
        ///</summary>
        public int MoonsWithKnownTemperatureCount => Moons?.Count(moon => moon.HasKnownTemperature) ?? 0;

        ///<summary>
        /// The average temperature (Kelvin) of the planet's moons. Moons without temperature data are left out,
        /// because counting them as 0 K would pull the average down. Null when no moon has temperature data.
        /// This replaces the former AverageMoonGravity stub, which always returned 0.
        ///</summary>
        public float? AverageMoonTemperature
        {
            get
            {
                var knownTemperatures = Moons?
                    .Where(moon => moon.HasKnownTemperature)
                    .Select(moon => moon.AverageTemperature.Value)
                    .ToList();

                if (knownTemperatures == null || knownTemperatures.Count == 0)
                {
                    return null;
                }

                return knownTemperatures.Average();
            }
        }

        ///<summary>
        /// Creates a planet from its DTO and the moons that belong to it. The moons are passed in separately
        /// because they are loaded in one bulk request instead of being nested in the planet response.
        ///</summary>
        public Planet(PlanetDto planetDto, IEnumerable<Moon> moons)
        {
            if (planetDto == null)
            {
                throw new ArgumentNullException(nameof(planetDto));
            }

            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            AverageTemperature = TemperatureUtility.FromApiValue(planetDto.AvgTemp);
            Moons = new Collection<Moon>((moons ?? Enumerable.Empty<Moon>()).ToList());
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
