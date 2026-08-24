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

        public async Task StartAsync(string filepath, int port)
        {

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
                    _ = HandleClientAsync(filepath,client);
                }
            });

        }

        private async Task HandleClientAsync(string filepath, TcpClient client)
        {
            try
            {
                // Initialize reader for SBS messages
                MessageReader reader = new MessageReader(filepath);

                await using var stream = client.GetStream();
                using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                while (client.Connected)
                {
                    TimeStampedSbsMessage? nextMessage; 
                    while ((nextMessage = reader.GetNextSbsMessage()) != null) 
                    { 
                        if (nextMessage.Timestamp > DateTime.Now) 
                        { 
                            reader.PushBackSbsMessage(nextMessage); 
                            break; 
                        } 
                        await writer.WriteLineAsync(nextMessage.SbsMessage); 
                        Console.WriteLine(nextMessage.SbsMessage);
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
