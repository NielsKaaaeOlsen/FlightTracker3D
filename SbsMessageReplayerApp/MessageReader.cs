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
        private StreamReader _reader;
        private bool _first = true;
        private TimeSpan _offset = TimeSpan.Zero;

        public class TimeStampedSbsMessage
        {
            public DateTime Timestamp { get; set; }
            public string SbsMessage { get; set; } = string.Empty;
        }


        public MessageReader(string filepath) 
        { 
            _filepath = filepath;
        }


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

        public TimeStampedSbsMessage? GetNextRecord()
        {
            if (_reader.EndOfStream)
            {
                return null;
            }
            else
            {
                string? record = _reader.ReadLine();

                if (!string.IsNullOrWhiteSpace(record))
                {
                    if (_first)
                    {
                        _offset = GetTimeOffsetFromRecord(record);
                        _first = false;
                    }
                    TimeStampedSbsMessage messageRecord = ConvertRecordToSbsMessage(record, _offset);
                    return messageRecord;
                }
                else
                    return null;
            }
        }

        TimeSpan GetTimeOffsetFromRecord(string record)
        {
            //
            TimeSpan delay = TimeSpan.FromSeconds(5);

            string[] tokens = record.Split(";");
            if (tokens.Length != 2)
                throw new Exception($"Invalid record format: {record}. Two tokens expected.");
            if (DateTime.TryParseExact(tokens[0], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime timeStamp))
            {
                TimeSpan offset = DateTime.Now + delay - timeStamp;
                return offset;
            }
            else
                throw new Exception($"DateTime parsing failed: {tokens[0]}");
        }

        TimeStampedSbsMessage ConvertRecordToSbsMessage(string record, TimeSpan offset)
        {
            string[] tokens = record.Split(";");
            if (tokens.Length != 2)
                throw new Exception($"Invalid record format: {record}. Two tokens expected.");

            string line = tokens[1];
            if (DateTime.TryParseExact(tokens[0], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime timeStamp))
            {
                var rawRecord = new TimeStampedSbsMessage
                {
                    Timestamp = timeStamp,
                    SbsMessage = line
                };

                return UpdateMessageDateTimes(rawRecord, offset);
            }
            else
                throw new Exception($"DateTime parsing failed: {tokens[0]}");
        }

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
                DateTime datetime1Offset = new DateTime(date1, time1).Add(offset);
                DateTime datetime2Offset = new DateTime(date2, time2).Add(offset);

                lineTokens[6] = datetime1Offset.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                lineTokens[7] = datetime1Offset.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

                lineTokens[8] = datetime2Offset.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                lineTokens[9] = datetime2Offset.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

            }
            string lineUpdated = string.Join(",", lineTokens);

            TimeStampedSbsMessage newRecord = new TimeStampedSbsMessage() { Timestamp = timestampUpdated, SbsMessage = lineUpdated };
            return newRecord;
        }
    }
}
