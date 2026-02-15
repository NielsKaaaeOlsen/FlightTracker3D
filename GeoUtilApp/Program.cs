using GeoUtil;
using System;

// Copenhagen Airport: 55.6180° N, 12.6508° E, altitude 5m
double lat1 = 55.6180, lon1 = 12.6508, alt1 = 5.0;

// Aircraft at 30 km east, 10 km north, 10,000m altitude
double lat2 = 55.7080, lon2 = 13.0508, alt2 = 10000.0;

double distance = GeoDistance.CalculateDistance(lat1, lon1, alt1, lat2, lon2, alt2);
Console.WriteLine($"Straight-line distance: {distance:F2} meters ({distance / 1000:F2} km)");

double horizontalDist = GeoDistance.CalculateHaversineDistance(lat1, lon1, lat2, lon2);
Console.WriteLine($"Horizontal (great-circle) distance: {horizontalDist:F2} meters ({horizontalDist / 1000:F2} km)");

// NEW: Calculate azimuth and elevation
double azimuth = GeoDistance.CalculateAzimuth(lat1, lon1, lat2, lon2);
double elevation = GeoDistance.CalculateElevation(lat1, lon1, alt1, lat2, lon2, alt2);
Console.WriteLine($"Azimuth: {azimuth:F2}° (from north), Elevation: {elevation:F2}°");

// Or combined:
var (az, el, di) = GeoDistance.CalculateAzimuthAndElevation(lat1, lon1, alt1, lat2, lon2, alt2);
Console.WriteLine($"Combined → Azimuth: {az:F2}°, Elevation: {el:F2}°  Distance: {di} meter");