// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using SbsMessageReplayerApp;

Console.WriteLine("SDR SBS message replayer.\nBy NKO,  June 2026");


//-- Get configuration from appsettings.json in the app folder
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var replayerSettings = config.GetSection("SbsMessageReplayerSettings").Get<SbsMessageReplayerSettings>();

if (replayerSettings == null)
{ 
    Console.WriteLine("Error: SbsMessageReplayerSettings is invalid in appsettings.json."); 
    return; 
}

Console.WriteLine($"SbsMessageFile: {replayerSettings.SbsMessageFile}\nPort: {replayerSettings.port}");

MockSbsServer server = new MockSbsServer();
await server.StartAsync(replayerSettings.SbsMessageFile, replayerSettings.port);  //30003);

Console.WriteLine("Closing replayer");
