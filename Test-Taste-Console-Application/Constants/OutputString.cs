namespace Test_Taste_Console_Application.Constants
{
    public class OutputString
    {
        public const string PlanetNumber = "Planet's Number";
        public const string PlanetSemiMajorAxis = "Planet's Semi-Major Axis";
        public const string PlanetId = "Planet's Id";
        public const string TotalMoons = "Total Moons";
        public const string MoonNumber = "Moon's Number";
        public const string MoonId = "Moon's Id";
        public const string MoonMassExponent = "Moon's Mass Exponent";
        public const string MoonMassValue = "Moon's Mass Value";
        public const string MoonsWithTemperatureData = "Moons With Temperature Data";
        public const string PlanetMoonAverageTemperature = "Average Moon Temperature";
        public const string PlanetAverageTemperature = "Planet's Average Temperature";
        public const string NotAvailable = "n/a";
        public const string NoMoonsFound = "No moons found.";
        public const string NoPlanetsFound = "No planets found.";
        public const string NoPlanetsWithMoonsFound = "No planets with moons found.";
        public const string NoMoonTemperatureData =
            "Note: the API reports no temperature (avgTemp = 0 K) for any of these moons, so their average is shown as n/a.";

        //Progress messages that keep the user informed about the current step.
        public const string LoadingData = "Loading data from the Solar System OpenData API...";
        public const string DataLoaded = "Data loaded: {0} planets and {1} moons.";
        public const string WritingAverageMoonTemperature = "Writing the planets with moons and their average moon temperature...";
        public const string WritingMoonsAndMass = "Writing all moons and their mass...";
        public const string WritingPlanetsAndMoons = "Writing all planets and their moons...";
        public const string WritingFiles = "Writing the lists to CSV files...";
        public const string Done = "Done.";
        public const string FileCreated = "The file is created in: ";
        public const string FolderCreated = "The folder is created in: ";
    }
}
