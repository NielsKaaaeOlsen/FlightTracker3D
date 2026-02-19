using HardwareMode;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StepperMotorController;
using System;
using System.Runtime;


// Build configuration from appsettings.json in the app folder
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Dump configuration from AxElControllerSettings
var controllerSettings = config.GetSection("AxElControllerSettings").Get<AxElControllerSettings>();

if (controllerSettings == null)
{ Console.WriteLine("Error: AxElControllerSettings is invalid in appsettings.json."); return; }

Console.WriteLine($"MinimumLogLevel: {controllerSettings.MinimumLogLevel}");
Console.WriteLine($"HardwareMode: {controllerSettings.HardwareMode}");
Console.WriteLine($"MicrosteppingMode: {controllerSettings.MicrosteppingMode}");
Console.WriteLine($"MotoComamnds:");
foreach (var command in controllerSettings.MoveToCommands)
{
    Console.WriteLine($"   MotorComamnd: az={command.Az}, el={command.El}, duration={command.Duration} sec");
}

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(controllerSettings.MinimumLogLevel)
        .AddSimpleConsole(options => { options.TimestampFormat = "[HH:mm:ss.fff] "; /*options.IncludeScopes = true;*/ })
        ;
});

DateTime start = DateTime.Now;

ILogger logger = loggerFactory.CreateLogger("Azimuth Elevation Stepper Motors");
logger.LogInformation("StepMotorApp started");


bool asyncTest = true;

if (asyncTest)
{
    try
    {
        using (var azElController = new AzElController(loggerFactory, controllerSettings.HardwareMode))
        {
            azElController.Initialize();
            azElController.SetMicrostepping(controllerSettings.MicrosteppingMode);

            foreach (var command in controllerSettings.MoveToCommands)
            {
                logger.LogInformation("Starting ASYNC move: az={az}, el={el}, duration={t} sec", command.Az, command.El, command.Duration);
                Task moveTask = azElController.MoveToAsync(command.Az, command.El, command.Duration);
                moveTask.Wait();
                logger.LogInformation("Move ASYNC command issued.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message + "\n" + ex.StackTrace);
    }
}
else
{
    try
    {
        using (var azElController = new AzElController(loggerFactory, HardwareModeEnum.Emulated))
        {
            azElController.Initialize();
            double azMoveTo = 4.5;
            double elMoveTo = 4.5;
            double durationSeconds = 4.0;

            logger.LogInformation("1: Starting move: az={azMoveTo}, el={elMoveTo}, duration={t} sec", azMoveTo, elMoveTo, durationSeconds);
            azElController.MoveToAzEl(azMoveTo, elMoveTo, durationSeconds);
            logger.LogInformation("1: Move command issued.");

            azMoveTo -= 1.8;
            elMoveTo -= 1.8;

            logger.LogInformation("2: Starting move: az={az}, el={el}, duration={t} sec", azMoveTo, elMoveTo, durationSeconds);
            azElController.MoveToAzEl(azMoveTo, elMoveTo, durationSeconds);
            logger.LogInformation("2: Move command issued.");

            //Keep target position --> no movemment
            logger.LogInformation("3: Starting move: az={az}, el={el}, duration={t} sec", azMoveTo, elMoveTo, durationSeconds);
            azElController.MoveToAzEl(azMoveTo, elMoveTo, durationSeconds);
            logger.LogInformation("3: Move command issued.");


            (double azPos, double ElPos) = azElController.GetCurrentZElPosition();
            logger.LogInformation("Final position: azPos={azPos}, elPos={elPos}", azPos, ElPos);

            DateTime end = DateTime.Now;
            logger.LogInformation("StepMotorApp ended. Total duration: {duration} sec", (end - start).TotalSeconds);

            //Move back to initial position with mazimum speed
            logger.LogInformation("Move back to initial position with mazimum speed");
            azElController.MoveToAzEl(0, 0);
            (double azPosInitial, double ElPosInitial) = azElController.GetCurrentZElPosition();
            logger.LogInformation("Final position as initial position: azPos={azPos}, elPos={elPos}", azPosInitial, ElPosInitial);

            //azElController.MoveTo(-9, 0, durationSeconds);
            //azElController.MoveTo(0, -9, durationSeconds);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message + "\n" + ex.StackTrace);
    }

    // dispose factory when done
    loggerFactory.Dispose();
}