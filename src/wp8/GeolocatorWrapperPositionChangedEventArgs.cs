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
        public string CurrentPace { get; set; }
        public string AveragePace { get; set; }
        public string CurrentSpeed { get; set; }
        public string AverageSpeed { get; set; }
    }
}