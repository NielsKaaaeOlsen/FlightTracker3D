
using FlightTracker3D;
using FlightTracker3DApp;
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
var appSettings = config.GetSection("FlightTracker3DAppSettings").Get<FlightTracker3DAppSettings>();

if (appSettings == null)
{ Console.WriteLine("Error: FlightTracker3DAppSettings is invalid in appsettings.json."); return; }

Console.WriteLine($"MinimumLogLevel: {appSettings.MinimumLogLevel}");
Console.WriteLine($"LCD HardwareMode: {appSettings.HardwareModes.LcdHardwareMode}");
Console.WriteLine($"LED HardwareMode: {appSettings.HardwareModes.LedHardwareMode}");
Console.WriteLine($"Steppper HardwareMode: {appSettings.HardwareModes.StepperMotorHardwareMode}");

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(appSettings.MinimumLogLevel)
        .AddSimpleConsole(options => { options.TimestampFormat = "[HH:mm:ss.fff] "; /*options.IncludeScopes = true;*/ })
        ;
});

DateTime start = DateTime.Now;

ILogger logger = loggerFactory.CreateLogger("Flight Tracker 3D App");
logger.LogInformation("Flight Tracker 3D App started");


using (FlightTracker3D.FlightTracking flightTracking = new FlightTracker3D.FlightTracking(appSettings.HardwareModes, loggerFactory))
{
    await flightTracking.StartTrackingAsync();
}
