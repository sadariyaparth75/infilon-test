using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Test_Taste_Console_Application.Configuration;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //The API key can come from appsettings.json (git-ignored) or user secrets; later sources win,
            //and the environment variable overrides both (see ApiSettings).
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(ConfigurationFileName.AppSettings, optional: true)
                .AddUserSecrets<Program>(optional: true)
                .Build();

            ApiSettings apiSettings;
            try
            {
                apiSettings = ApiSettings.FromConfiguration(configuration);
            }
            catch (InvalidOperationException exception)
            {
                //Without a key every request returns 401, so stop early with a clear instruction.
                Console.WriteLine(exception.Message);
                Environment.ExitCode = 1;
                return;
            }

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(apiSettings);

            //The ConfigureServices function configures the services.
            ConfigureServices(serviceCollection);
            
            //The RunServiceOperations function executes the code that can create the outputs.
            RunServiceOperations(serviceCollection);
        }

        private static void RunServiceOperations(IServiceCollection serviceCollection)
        {
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            //The service provider gets the services.
            var planetService = serviceProvider.GetRequiredService<IPlanetService>();
            var moonService = serviceProvider.GetRequiredService<IMoonService>();
            var screenOutputService = serviceProvider.GetRequiredService<IOutputService>();
            var fileOutputService = serviceProvider.GetRequiredService<IFileOutputService>();

            //Step 1: the data is loaded once up front (2 API requests in total); both outputs reuse the cached data.
            //Without data there is nothing to write, so the run stops here on failure.
            try
            {
                Console.WriteLine(OutputString.LoadingData);
                var planetCount = planetService.GetAllPlanets().Count();
                var moonCount = moonService.GetAllMoons().Count();
                Console.WriteLine(OutputString.DataLoaded, planetCount, moonCount);
                ConsoleWriter.CreateEmptyLines(1);
            }
            catch (Exception exception)
            {
                ReportFailure(LoggerMessage.LoadingDataFailed, ExceptionMessage.LoadingDataFailed, exception);
                return;
            }

            //Step 2 and 3 are independent: a failing screen output doesn't prevent the files from being written.
            try
            {
                Console.WriteLine(OutputString.WritingAverageMoonTemperature);
                screenOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToConsole();

                Console.WriteLine(OutputString.WritingMoonsAndMass);
                screenOutputService.OutputAllMoonsAndTheirMassToConsole();

                Console.WriteLine(OutputString.WritingPlanetsAndMoons);
                screenOutputService.OutputAllPlanetsAndTheirMoonsToConsole();
            }
            catch (Exception exception)
            {
                ReportFailure(LoggerMessage.ScreenOutputOperationFailed, ExceptionMessage.ScreenOutputOperationFailed, exception);
            }

            try
            {
                Console.WriteLine(OutputString.WritingFiles);
                fileOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToFile();
                fileOutputService.OutputAllMoonsAndTheirMassToFile();
                fileOutputService.OutputAllPlanetsAndTheirMoonsToFile();
            }
            catch (Exception exception)
            {
                ReportFailure(LoggerMessage.FileOutputOperationFailed, ExceptionMessage.FileOutputOperationFailed, exception);
            }

            Console.WriteLine(OutputString.Done);
        }

        private static void ReportFailure(string loggerMessage, string userMessage, Exception exception)
        {
            //The users and developers can see the thrown exceptions; the log also gets the stack trace.
            Logger.Instance.Error($"{loggerMessage}{exception.Message}", exception);
            Console.WriteLine($"{userMessage}{exception.Message}");
            System.Diagnostics.Debug.WriteLine($"{userMessage}{exception.Message}");
            Environment.ExitCode = 1;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //The function configures all the services.
            //The logger config is resolved next to the executable, so logging also works with "dotnet run" from another folder.
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo(Path.Combine(AppContext.BaseDirectory, ConfigurationFileName.Logger)));
            serviceCollection.AddHttpClient<HttpClientService>();
            serviceCollection.AddSingleton<IPlanetService, PlanetService>();
            serviceCollection.AddSingleton<IOutputService, ScreenOutputService>();
            serviceCollection.AddSingleton<IFileOutputService, FileOutputService>();
            serviceCollection.AddSingleton<IMoonService, MoonService>();
        }
    }
}
