// See https://aka.ms/new-console-template for more information
using TestStepMotorFaultDetectorApp;
using System.Threading;

Console.WriteLine("Hello, FaultMonitor !");

using var faultMonitor = new FaultMonitor();

// Subscribe to fault events from FaultMonitor
faultMonitor.FaultDetected += (s, e) =>
{
    Console.WriteLine($"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}  DRV8825 FAULT!");
    // TODO: add centralized handling here (log file, shutdown, alert, etc.)
};

Console.WriteLine("FAULT monitor started.");

while (true)
{
    using var cts = new CancellationTokenSource();
    faultMonitor.FaultDetected += (s, e) =>
    {
        Console.WriteLine($"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} DRV8825 FAULT!");
        //cts.Cancel(); // signalér hovedtråden
    };

    try
    {
        Task.Delay(Timeout.Infinite, cts.Token).Wait();
    }
    catch (OperationCanceledException) 
    { /* shutdown */ }
}
