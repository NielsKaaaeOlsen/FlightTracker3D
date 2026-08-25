using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SbsMessageReplayerApp
{

    internal class MessageReader
    {
        private readonly string _filepath;
        private StreamReader? _reader;
        private bool _first = true;
        private TimeSpan _offset = TimeSpan.Zero;

        private TimeStampedSbsMessage? _lastMessage;

        public class TimeStampedSbsMessage
        {
            public DateTime Timestamp { get; set; }
            public string SbsMessage { get; set; } = string.Empty;
        }


        public MessageReader(string filepath) 
        { 
            _filepath = filepath;
        }


        /// <summary>
        /// Initializes the StreamReader for reading SBS messages from the specified file path. If the file does not exist, an error message is displayed.
        /// </summary>
        public void InitializeReader()
        {
            if (!File.Exists(_filepath))
            {
                Console.WriteLine($"Error: File '{_filepath}' does not exist.");
                return;
            }

            Console.WriteLine($"Reading SBS messages from '{_filepath}'...");

            _reader = new StreamReader(_filepath);
        }

        public TimeStampedSbsMessage? GetNextSbsMessage()
        {
            if (_lastMessage != null)
            {
                var lastMessage = _lastMessage;
                _lastMessage = null;
                return lastMessage;
            }

            if (_reader?.EndOfStream == true)
            {
                return null;
            }
            else
            {
                string? line = _reader.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    TimeStampedSbsMessage timeStampedSbsMessage = ConvertLineToSbsMessage(line);
                    if (_first)
                    {
                        _offset = GetTimeOffsetFromFirstTimeStampedSbsMessage(timeStampedSbsMessage);
                        _first = false;
                    }
                    TimeStampedSbsMessage updatedTimeStampedSbsMessage = UpdateMessageDateTimes(timeStampedSbsMessage, _offset);
                    return updatedTimeStampedSbsMessage;
                }
                else
                    return null;
            }
        }

        public void PushBackSbsMessage(TimeStampedSbsMessage timeStampedSbsMessage)
        {
            _lastMessage = timeStampedSbsMessage;
        }

        /// <summary>
        /// Calculates the time offset from the first timestamped SBS message to the current time plus a 5-second delay.
        /// </summary>
        /// <param name="timeStampedSbsMessage"></param>
        /// <returns></returns>
        TimeSpan GetTimeOffsetFromFirstTimeStampedSbsMessage(TimeStampedSbsMessage timeStampedSbsMessage)
        {
            TimeSpan delay = TimeSpan.FromSeconds(5);
            TimeSpan offset = DateTime.UtcNow + delay - timeStampedSbsMessage.Timestamp;
            return offset;
        }

        /// <summary>
        /// Converts a line from the SBS message file into a TimeStampedSbsMessage object.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        TimeStampedSbsMessage ConvertLineToSbsMessage(string line)
        {
            string[] tokens = line.Split(";");
            if (tokens.Length != 2)
                throw new Exception($"Invalid record format: {line}. Two tokens expected.");

            string sbsMessage = tokens[1];
            if (DateTime.TryParseExact(tokens[0], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime timeStamp))
            {
                var rawTimeStampedSbsMessage = new TimeStampedSbsMessage
                {
                    Timestamp = timeStamp.Subtract(TimeSpan.FromHours(2)),  //TODO: FJERN 
                    SbsMessage = sbsMessage
                };

                return rawTimeStampedSbsMessage; 
            }
            else
                throw new Exception($"DateTime parsing failed: {tokens[0]}");
        }

        /// <summary>
        /// Updates the date and time fields in the SBS message based on the provided offset.
        /// </summary>
        /// <param name="sbsMessage"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TimeStampedSbsMessage UpdateMessageDateTimes( TimeStampedSbsMessage sbsMessage, TimeSpan offset)
        {
            DateTime timestampUpdated = sbsMessage.Timestamp.Add(offset);

            string[] lineTokens = sbsMessage.SbsMessage.Split(",");

            //                        4      6   7    8    9    10       11         12        13    14  15   16          17     18    19        20  21
            // SBS-1 format: MSG,3,,,ICAO,,date,time,date,time,callsign,altitude,groundspeed,track,lat,lon,verticalrate,squawk,alert,emergency,spi,onground

            string dateStr1 = lineTokens[6];
            string timeStr1 = lineTokens[7];
            string dateStr2 = lineTokens[8];
            string timeStr2 = lineTokens[9];

            if (TimeOnly.TryParseExact(timeStr1,"HH:mm:ss.fff",CultureInfo.InvariantCulture, DateTimeStyles.None,out TimeOnly time1)  &&
                DateOnly.TryParseExact(dateStr1, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date1) &&
                TimeOnly.TryParseExact(timeStr2, "HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly time2) &&
                DateOnly.TryParseExact(dateStr2, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date2)
                )
            {
                DateTime datetime1PlusOffset = new DateTime(date1, time1).Add(offset);
                DateTime datetime2PlusOffset = new DateTime(date2, time2).Add(offset);

                lineTokens[6] = datetime1PlusOffset.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                lineTokens[7] = datetime1PlusOffset.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

                lineTokens[8] = datetime2PlusOffset.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                lineTokens[9] = datetime2PlusOffset.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

            }
            string lineUpdated = string.Join(",", lineTokens);

            // Return a new TimeStampedSbsMessage with the updated timestamp and SBS message based on the provided offset
            return new TimeStampedSbsMessage() { Timestamp = timestampUpdated, SbsMessage = lineUpdated };
        }
    }
}
