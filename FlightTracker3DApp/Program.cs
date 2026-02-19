
using FlightTracker3D;
using HardwareMode;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StepperMotorController;

Console.WriteLine("===========================================");
Console.WriteLine("   FlightTracker3D Application Starting   ");
Console.WriteLine("===========================================");



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


HardwareModes hardwareModes = new HardwareModes(
    lcdHardwareMode: HardwareModeEnum.Emulated, 
    ledHardwareMode: HardwareModeEnum.Emulated,
    stepperMotorHardwareMode: HardwareModeEnum.Emulated);


using (FlightTracker3D.FlightTracking flightTracking = new FlightTracker3D.FlightTracking(hardwareModes, loggerFactory))
{
    await flightTracking.StartTrackingAsync();
}
