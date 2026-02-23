
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
Console.WriteLine($"AirCraftListener HardwareMode: {appSettings.HardwareModes.AirCraftListenerMode}");

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

try
{
    //-- Create cancellation token source for graceful shutdown
    using var cts = new CancellationTokenSource();

    //-- Start keyboard monitoring task
    var keyboardTask = Task.Run(() =>
    {
        Console.WriteLine("Press Ctrl-X to shutdown gracefully...");
        while (!cts.Token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.X && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    Console.WriteLine("\n[Ctrl-X detected] Initiating graceful shutdown...");
                    cts.Cancel();
                    break;
                }
            }
            Thread.Sleep(100);
        }
    });

    //-- Create and start flight tracking
    using (FlightTracking flightTracking = new FlightTracking(appSettings.HardwareModes, loggerFactory))
    {
        await flightTracking.StartTrackingAsync(cts.Token);
    }

    await keyboardTask;
}
catch (OperationCanceledException)
{
    logger.LogInformation("Flight Tracker 3D App shut down gracefully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred in the Flight Tracker 3D App.");
}
