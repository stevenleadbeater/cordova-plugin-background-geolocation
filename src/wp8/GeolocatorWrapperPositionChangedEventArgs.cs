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
        public double? TotalDistance { get; set; }
        public double? CurrentPace { get; set; }
        public double? AveragePace { get; set; }
        public double? CurrentSpeed { get; set; }
        public double? AverageSpeed { get; set; }
        public string NotiticationText { get; set; }
    }
}