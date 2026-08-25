using SbsMessageRecorder;

try
{
    // The application will record messages for 30 minutes and save them to a text file with a timestamped filename.
    // The filename will be in the format "SbsMessages_YYYYMMDD_HHMMSS.txt".
    // The recorded messages will be saved in the text file in the format: "Timestamp;Message".
    // The application will also print the recorded messages to the console as they are received.
    // The application will handle any exceptions that occur during the recording process and print the error message to the console.
    // The application will also print a message to the console indicating that it is starting the recording process.
    // The application will also print a message to the console indicating that it has finished recording and saved the messages to the file.

    // Important:
    // Launch the application from a PC with a network connection to the SBS server. The SBS server must be running and accessible at the specified IP address and port.
    // The application will connect to the SBS server and start recording messages for the specified duration. The recorded messages will be saved to a text file in the current directory.
    // Deployment of the application to Raspberry Pi is not necessary :-) .

    Console.WriteLine("SDR SBS message recorder.\nBy NKO,  June 2026");

    string filePath = $"SbsMessages_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";

    var recorder = new MessageRecorder();
    await recorder.Record(1800); // Record for 30 minutes

    recorder.SaveToTxtFile(filePath);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}