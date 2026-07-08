using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SbsMessageRecorder
{
    internal class MessageRecorder
    {
        public class SbsMessageRecord
        {
            public DateTime Timestamp { get; set; }
            public string Line { get; set; } = string.Empty;
        }


        // List of raw line messages received from the SBS server, along with the timestamp of when they were received
        private List<SbsMessageRecord> _recordedMessages = new List<SbsMessageRecord>();


        public async Task Record(int durationSeconds)
        {
            // Implementation for recording messages for the specified duration

            _recordedMessages.Clear();

            DateTime recordUntil = DateTime.UtcNow + TimeSpan.FromSeconds(durationSeconds);

            // Listening to the SBS server and recording messages until we have received 100 messages
            using var client = new TcpClient();
            await client.ConnectAsync("192.168.1.92", 30003);

            using var reader = new StreamReader(client.GetStream());
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (DateTime.UtcNow > recordUntil)
                        break;
                    Console.WriteLine($"line= {line}");
                    _recordedMessages.Add(new SbsMessageRecord { Timestamp = DateTime.Now, Line = line });
                }
            }
        }

        // Save the recorded messages to a file in JSON format.
        public void SaveToJsonFile(string filePath)
        {
            // Save the recorded messages to a file in JSON format
            var json = System.Text.Json.JsonSerializer.Serialize(_recordedMessages, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }

        // Save the recorded messages to a file in text format. 
        public void SaveToTxtFile(string filePath)
        {
            // Save the recorded messages to a file in text format
            var txtLines = _recordedMessages.Select(m => $"{m.Timestamp:O};{m.Line}");  //Don't use comma as separator, because the line may contain commas. Use semicolon instead.
            System.IO.File.WriteAllLines(filePath, txtLines);
        }


    }
}
