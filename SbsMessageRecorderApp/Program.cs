using SbsMessageRecorder;

try
{
    Console.WriteLine("SDR SBS message recorder.\nBy NKO,  June 2026");

    string filePath = $"SbsMessages_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";

    var recorder = new MessageRecorder();
    await recorder.Record(60); // Record for 60 seconds

    recorder.SaveToTxtFile(filePath);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}