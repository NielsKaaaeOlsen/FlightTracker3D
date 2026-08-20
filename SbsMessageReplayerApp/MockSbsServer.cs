using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static SbsMessageReplayerApp.MessageReader;



namespace SbsMessageReplayerApp
{
    internal class MockSbsServer
    {

        public async Task StartAsync(int port)
        {
            // Initialize simulated aircraft
            MessageReader reader = new MessageReader("sbs_messages.txt");


            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server started on port {port}");

            // Accept clients
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                    _ = HandleClientAsync(client);
                }
            });

            // Update aircraft positions
            while (true)
            {
                var next = reader.GetNextRecord();
                if (next != null)
                {
                    await WaitUntilTimestamp(next);
                }
            }
        }

        public async Task WaitUntilTimestamp(TimeStampedSbsMessage record)
        {
            TimeSpan delay = record.Timestamp - DateTime.Now;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                await using var stream = client.GetStream();
                using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                while (client.Connected)
                {
                    foreach (var ac in _aircrafts)
                    {
                        // Generate SBS-1 format message
                        string sbsMessage = GenerateSbsMessage(ac);
                        await writer.WriteLineAsync(sbsMessage);
                        Console.WriteLine(sbsMessage);
                    }

                    await Task.Delay(1000);  // Send updates every second
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected: {ex.Message}");
            }
        }


    }
}
