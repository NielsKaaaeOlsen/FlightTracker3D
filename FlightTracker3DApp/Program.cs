
using FlightTracker3D;
using FlightTracker3DApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

Console.WriteLine("===========================================");
Console.WriteLine("   FlightTracker3D Application Starting   ");
Console.WriteLine("===========================================");

//-- Build configuration from FlightTracker3DAppSettings.json in the app folder
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("FlightTracker3DAppSettings.json", optional: false, reloadOnChange: true)
    .Build();

//-- Dump configuration from AxElControllerSettings
var appSettings = config.GetSection("FlightTracker3DAppSettings").Get<FlightTracker3DAppSettings>();

if (appSettings == null)
{ Console.WriteLine("Error: FlightTracker3DAppSettings is invalid in appsettings.json."); return; }

Console.WriteLine($"MinimumLogLevel: {appSettings.MinimumLogLevel}");
Console.WriteLine($"LCD HardwareMode: {appSettings.HardwareModes.LcdHardwareMode}");
Console.WriteLine($"LED HardwareMode: {appSettings.HardwareModes.LedHardwareMode}");
Console.WriteLine($"Steppper HardwareMode: {appSettings.HardwareModes.StepperMotorHardwareMode}");

//-- Create logger factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(appSettings.MinimumLogLevel)
        .AddSimpleConsole(options => { options.TimestampFormat = "[HH:mm:ss.fff] "; /*options.IncludeScopes = true;*/ })
        ;
});

ILogger logger = loggerFactory.CreateLogger("Flight Tracker 3D App");
logger.LogInformation("Flight Tracker 3D App started");

//-- Create and start flight tracking
using (FlightTracking flightTracking = new FlightTracking(appSettings.HardwareModes, loggerFactory))
{
    await flightTracking.StartTrackingAsync();
}
