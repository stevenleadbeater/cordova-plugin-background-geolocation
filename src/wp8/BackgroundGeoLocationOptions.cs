using System;

namespace Cordova.Extension.Commands
{
    public class BackgroundGeoLocationOptions
    {
        public double StationaryRadius;
        public double DistanceFilterInMeters;
        public UInt32 LocationTimeoutInSeconds;
        public UInt32 DesiredAccuracyInMeters;
        public bool Debug;
        public bool StopOnTerminate;
        public bool ParsingSucceeded { get; set; }
        public bool UseFixedTimeInterval { get; set; }
        public UInt32 IntervalReportSeconds { get; set; }
        public UInt32 IntervalReportMeters { get; set; }
        public bool ReportInMiles { get; set; }
        public bool ReportTotalTime { get; set; }
        public bool ReportTotalDistance { get; set; }
        public bool ReportAveragePace { get; set; }
        public bool ReportCurrentPace { get; set; }        
        public bool ReportAverageSpeed { get; set; }
        public bool ReportCurrentSpeed { get; set; }
    }
}
