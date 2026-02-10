using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AirCraftDetector
{
    public class SbsTcpClient
    {
        public async Task ConnectAsync(string host, int port, Action<string> onLine)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port);

            using var reader = new StreamReader(client.GetStream());

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    onLine(line);
            }
        }
    }
}
