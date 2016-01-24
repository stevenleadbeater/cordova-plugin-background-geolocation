using System;
using Windows.Devices.Geolocation;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;
using System.Diagnostics;
using Windows.Phone.Speech.Synthesis;

namespace Cordova.Extension.Commands
{
    public class BackgroundGeoLocation : BaseCommand, IBackgroundGeoLocation
    {
        private string ConfigureCallbackToken { get; set; }
        private string OnStationaryCallbackToken { get; set; }
        private BackgroundGeoLocationOptions BackgroundGeoLocationOptions { get; set; }

        public static IGeolocatorWrapper Geolocator { get; set; }

        /// <summary>
        /// RunningInBackground is a required property to run in background (also an active Geolocator instance is required)
        /// For more information read http://msdn.microsoft.com/library/windows/apps/jj662935(v=vs.105).aspx
        /// </summary>
        public static bool RunningInBackground { get; set; }

        /// <summary>
        /// When start() is fired immediate after configure() in javascript, configure may not be finished yet, IsConfigured and IsConfiguring are used to keep track of this
        /// </summary>
        private bool IsConfigured { get; set; }
        private bool IsConfiguring { get; set; }
        private bool _reportInMiles;

        private readonly IDebugNotifier _debugNotifier;

        public BackgroundGeoLocation()
        {
            IsConfiguring = false;
            IsConfigured = false;
            _debugNotifier = DebugNotifier.GetDebugNotifier();
        }

        public void configure(string args)
        {
            IsConfiguring = true;
            ConfigureCallbackToken = CurrentCommandCallbackId;
            RunningInBackground = false;

            BackgroundGeoLocationOptions = ParseBackgroundGeoLocationOptions(args);

            IsConfigured = BackgroundGeoLocationOptions.ParsingSucceeded;
            IsConfiguring = false;
        }

        private BackgroundGeoLocationOptions ParseBackgroundGeoLocationOptions(string configureArgs)
        {
            var parsingSucceeded = true;

            var options = JsonHelper.Deserialize<string[]>(configureArgs);

            double stationaryRadius, distanceFilter;
            UInt32 locationTimeout, desiredAccuracy;
            bool debug;
            bool useFixedTimeInterval;
            UInt32 intervalReportSeconds;
            UInt32 intervalReportMeters;
            bool reportTotalTime;
            bool reportTotalDistance;
            bool reportAveragePace;
            bool reportCurrentPace;
            bool reportAverageSpeed;
            bool reportCurrentSpeed;
            bool reportInMiles;

            if (!double.TryParse(options[0], out stationaryRadius))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for stationaryRadius: {0}", options[0])));
                parsingSucceeded = false;
            }
            if (!double.TryParse(options[1], out distanceFilter))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for distanceFilter: {0}", options[1])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[2], out locationTimeout))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for locationTimeout: {0}", options[2])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[3], out desiredAccuracy))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for desiredAccuracy: {0}", options[3])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[4], out debug))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for debug: {0}", options[4])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[15], out useFixedTimeInterval))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for useFixedTimeInterval: {0}", options[15])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[16], out intervalReportSeconds))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for intervalReportSeconds: {0}", options[16])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[17], out intervalReportMeters))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for intervalReportMeters: {0}", options[17])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[18], out reportTotalTime))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportTotalTime: {0}", options[18])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[19], out reportTotalDistance))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportTotalDistance: {0}", options[19])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[20], out reportAveragePace))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportAveragePace: {0}", options[20])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[21], out reportCurrentPace))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportCurrentPace: {0}", options[21])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[22], out reportAverageSpeed))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportAverageSpeed: {0}", options[22])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[23], out reportCurrentSpeed))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportCurrentSpeed: {0}", options[23])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[24], out reportInMiles))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportInMiles: {0}", options[24])));
                parsingSucceeded = false;
            }

            _reportInMiles = reportInMiles;

            return new BackgroundGeoLocationOptions
            {
                StationaryRadius = stationaryRadius,
                DistanceFilterInMeters = distanceFilter,
                LocationTimeoutInSeconds = locationTimeout,
                DesiredAccuracyInMeters = desiredAccuracy,
                Debug = debug,
                ParsingSucceeded = parsingSucceeded,
                UseFixedTimeInterval = useFixedTimeInterval,
                IntervalReportSeconds = intervalReportSeconds,
                IntervalReportMeters = intervalReportMeters,
                ReportInMiles = reportInMiles,
                ReportTotalTime = reportTotalTime,
                ReportTotalDistance = reportTotalDistance,
                ReportAveragePace = reportAveragePace,
                ReportCurrentPace = reportCurrentPace,
                ReportAverageSpeed = reportAverageSpeed,
                ReportCurrentSpeed = reportCurrentSpeed
            };
        }

        private readonly Object _startLock = new Object();

        public void start(string args)
        {
            lock (_startLock)
            {
                while (!IsConfigured && IsConfiguring)
                {
                    // Wait for configure() to complete...
                }

                if (!IsConfigured || !BackgroundGeoLocationOptions.ParsingSucceeded)
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.INVALID_ACTION, "Cannot start: Run configure() with proper values!"));
                    stop(args);
                    return;
                }

                if (Geolocator != null && Geolocator.IsActive)
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.INVALID_ACTION, "Already started!"));
                    return;
                }

                Geolocator = new GeolocatorWrapper(BackgroundGeoLocationOptions.DesiredAccuracyInMeters, BackgroundGeoLocationOptions.LocationTimeoutInSeconds * 1000, 
                    BackgroundGeoLocationOptions.DistanceFilterInMeters, BackgroundGeoLocationOptions.StationaryRadius, BackgroundGeoLocationOptions.UseFixedTimeInterval, 
                    BackgroundGeoLocationOptions.IntervalReportSeconds, BackgroundGeoLocationOptions.IntervalReportMeters, BackgroundGeoLocationOptions.ReportTotalTime, 
                    BackgroundGeoLocationOptions.ReportTotalDistance, BackgroundGeoLocationOptions.ReportAveragePace, BackgroundGeoLocationOptions.ReportCurrentPace, 
                    BackgroundGeoLocationOptions.ReportAverageSpeed, BackgroundGeoLocationOptions.ReportCurrentSpeed, BackgroundGeoLocationOptions.ReportInMiles);
                Geolocator.PositionChanged += OnGeolocatorOnPositionChanged;
                Geolocator.Start();

                RunningInBackground = true;
            }
        }

        private void OnGeolocatorOnPositionChanged(GeolocatorWrapper sender, GeolocatorWrapperPositionChangedEventArgs eventArgs)
        {
            if (eventArgs.GeolocatorLocationStatus == PositionStatus.Disabled || eventArgs.GeolocatorLocationStatus == PositionStatus.NotAvailable)
            {
                DispatchMessage(PluginResult.Status.ERROR, string.Format("Cannot start: LocationStatus/PositionStatus: {0}! {1}", eventArgs.GeolocatorLocationStatus, IsConfigured), true, ConfigureCallbackToken);
                return;
            }

            HandlePositionUpdateDebugData(eventArgs.PositionUpdateDebugData);

            if (eventArgs.Position != null)
                DispatchMessage(PluginResult.Status.OK, eventArgs.Position.Coordinate.ToJson(), true, ConfigureCallbackToken);
            else if (eventArgs.EnteredStationary)
                DispatchMessage(PluginResult.Status.OK, string.Format("{0:0.}", BackgroundGeoLocationOptions.StationaryRadius), true, OnStationaryCallbackToken);
            else
                DispatchMessage(PluginResult.Status.ERROR, "Null position received", true, ConfigureCallbackToken);

            //DispatchMessage(PluginResult.Status.OK, eventArgs.ToJson(), true, ConfigureCallbackToken);

            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SpeakTextAsync(string.Format("Time {0}, Distance {1}, Current Pace {2}, Average Pace {3}", eventArgs.TotalTime.ToString("mm:ss"),
                        eventArgs.TotalDistance, eventArgs.CurrentPace, eventArgs.AveragePace));

            //if (eventArgs.SpeachReportReady)
            //{
                

            //    if (_reportInMiles)
            //    {
                    
            //    }
            //}
        }

        private void HandlePositionUpdateDebugData(PostionUpdateDebugData postionUpdateDebugData)
        {
            var debugMessage = postionUpdateDebugData.GetDebugNotifyMessage();
            Debug.WriteLine(debugMessage);

            if (!BackgroundGeoLocationOptions.Debug) return;

            switch (postionUpdateDebugData.PositionUpdateType)
            {
                case PositionUpdateType.SkippedBecauseOfDistance:
                    _debugNotifier.Notify(debugMessage, new Tone(250, Frequency.Low));
                    break;
                case PositionUpdateType.NewPosition:
                    _debugNotifier.Notify(debugMessage, new Tone(750, Frequency.High));
                    break;
                case PositionUpdateType.EnteringStationary:
                    _debugNotifier.Notify(debugMessage, new Tone(250, Frequency.High), new Tone(250, Frequency.High));
                    break;
                case PositionUpdateType.StationaryUpdate:
                    _debugNotifier.Notify(debugMessage, new Tone(750, Frequency.Low), new Tone(750, Frequency.Low));
                    break;
                case PositionUpdateType.ExitStationary:
                    _debugNotifier.Notify(debugMessage, new Tone(250, Frequency.High), new Tone(250, Frequency.High), new Tone(250, Frequency.High));
                    break;
            }
        }

        public void stop(string args)
        {
            RunningInBackground = false;
            if (Geolocator != null) Geolocator.Stop();
        }

        public void finish(string args)
        {
            DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
        }

        public void onPaceChange(bool isMoving)
        {
            if (isMoving)
            {
                Geolocator.ChangeStationary(isMoving);
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
            }
            else
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.INVALID_ACTION, "Manualy start stationary not available"));
            }
        }

        public void setConfig(string setConfigArgs)
        {
            if (Geolocator == null) return;

            if (Geolocator.IsActive)
            {
                Geolocator.PositionChanged -= OnGeolocatorOnPositionChanged;
                Geolocator.Stop();
            }

            if (string.IsNullOrWhiteSpace(setConfigArgs))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.INVALID_ACTION, "Cannot set config because of an empty input"));
                return;
            }
            var parsingSucceeded = true;

            var options = JsonHelper.Deserialize<string[]>(setConfigArgs);

            double stationaryRadius, distanceFilter;
            UInt32 locationTimeout, desiredAccuracy;
            bool useFixedTimeInterval;
            UInt32 intervalReportSeconds;
            UInt32 intervalReportMeters;
            bool reportTotalTime;
            bool reportTotalDistance;
            bool reportAveragePace;
            bool reportCurrentPace;
            bool reportAverageSpeed;
            bool reportCurrentSpeed;
            bool reportInMiles;

            if (!double.TryParse(options[0], out stationaryRadius))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for stationaryRadius: {0}", options[2])));
                parsingSucceeded = false;
            }
            if (!double.TryParse(options[1], out distanceFilter))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for distanceFilter: {0}", options[3])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[2], out locationTimeout))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for locationTimeout: {0}", options[4])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[3], out desiredAccuracy))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for desiredAccuracy: {0}", options[5])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[15], out useFixedTimeInterval))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for useFixedTimeInterval: {0}", options[15])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[16], out intervalReportSeconds))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for intervalReportSeconds: {0}", options[16])));
                parsingSucceeded = false;
            }
            if (!UInt32.TryParse(options[17], out intervalReportMeters))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for intervalReportMeters: {0}", options[17])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[18], out reportTotalTime))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportTotalTime: {0}", options[18])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[19], out reportTotalDistance))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportTotalDistance: {0}", options[19])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[20], out reportAveragePace))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportAveragePace: {0}", options[20])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[21], out reportCurrentPace))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportCurrentPace: {0}", options[21])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[22], out reportAverageSpeed))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportAverageSpeed: {0}", options[22])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[23], out reportCurrentSpeed))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportCurrentSpeed: {0}", options[23])));
                parsingSucceeded = false;
            }
            if (!bool.TryParse(options[24], out reportInMiles))
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, string.Format("Invalid value for reportInMiles: {0}", options[24])));
                parsingSucceeded = false;
            }
            if (!parsingSucceeded) return;

            BackgroundGeoLocationOptions.StationaryRadius = stationaryRadius;
            BackgroundGeoLocationOptions.DistanceFilterInMeters = distanceFilter;
            BackgroundGeoLocationOptions.LocationTimeoutInSeconds = locationTimeout * 1000;
            BackgroundGeoLocationOptions.DesiredAccuracyInMeters = desiredAccuracy;
            BackgroundGeoLocationOptions.UseFixedTimeInterval = useFixedTimeInterval;
            BackgroundGeoLocationOptions.IntervalReportSeconds = intervalReportSeconds;
            BackgroundGeoLocationOptions.IntervalReportMeters = intervalReportMeters;
            BackgroundGeoLocationOptions.ReportInMiles = reportInMiles;
            BackgroundGeoLocationOptions.ReportTotalTime = reportTotalTime;
            BackgroundGeoLocationOptions.ReportTotalDistance = reportTotalDistance;
            BackgroundGeoLocationOptions.ReportAveragePace = reportAveragePace;
            BackgroundGeoLocationOptions.ReportCurrentPace = reportCurrentPace;
            BackgroundGeoLocationOptions.ReportAverageSpeed = reportAverageSpeed;
            BackgroundGeoLocationOptions.ReportCurrentSpeed = reportCurrentSpeed;

            Geolocator = new GeolocatorWrapper(desiredAccuracy, locationTimeout * 1000, distanceFilter, stationaryRadius, useFixedTimeInterval, intervalReportSeconds,
                intervalReportMeters, reportTotalTime, reportTotalDistance, reportAveragePace, reportCurrentPace, reportAverageSpeed, reportCurrentSpeed, reportInMiles);
            Geolocator.PositionChanged += OnGeolocatorOnPositionChanged;
            Geolocator.Start();

            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }

        public void getStationaryLocation(string args)
        {
            var stationaryGeolocation = Geolocator.GetStationaryLocation();
            DispatchMessage(PluginResult.Status.OK, stationaryGeolocation.ToJson(), true, ConfigureCallbackToken);
        }

        public void addStationaryRegionListener(string args)
        {
            OnStationaryCallbackToken = CurrentCommandCallbackId;
        }

        private void DispatchMessage(PluginResult.Status status, string message, bool keepCallback, string callBackId)
        {
            var pluginResult = new PluginResult(status, message) { KeepCallback = keepCallback };
            DispatchCommandResult(pluginResult, callBackId);
        }
    }
}
