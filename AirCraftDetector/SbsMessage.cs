namespace AirCraftDetector
{
    public class SbsMessage
    {
        public string MessageType { get; set; }
        public string TransmissionType { get; set; }
        public string Icao { get; set; }
        public string Callsign { get; set; }
        public int? Altitude { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? GroundSpeed { get; set; }
        public double? Track { get; set; }
        public int? VerticalRate { get; set; }
        public bool? OnGround { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
