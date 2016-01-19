using System;
using Windows.Devices.Geolocation;

namespace Cordova.Extension.Commands
{
    public class GeolocatorWrapperPositionChangedEventArgs
    {
        public Geoposition Position { get; set; }
        public bool EnteredStationary { get; set; }
        public PositionStatus GeolocatorLocationStatus { get; set; }
        public PostionUpdateDebugData PositionUpdateDebugData { get; set; }
        public bool SpeachReportReady { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int TotalDistance { get; set; }
        public decimal CurrentPace { get; set; }
        public decimal AveragePace { get; set; }
        public decimal CurrentSpeed { get; set; }
        public decimal AverageSpeed { get; set; }
    }
}