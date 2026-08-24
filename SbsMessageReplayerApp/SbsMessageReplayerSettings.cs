using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SbsMessageReplayerApp
{
    public class SbsMessageReplayerSettings
    {
        public required string SbsMessageFile { get; set; } = string.Empty;
        public required int port { get; set; }
    }
}
