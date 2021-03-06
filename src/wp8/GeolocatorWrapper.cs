﻿using System.IO;
using System.IO.IsolatedStorage;
using System;
using System.Collections.Generic;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using WPCordovaClassLib.Cordova.Commands;

namespace Cordova.Extension.Commands
{
    public interface IGeolocatorWrapper
    {
        /// <summary>
        /// Raises when location is updated after Report Interval and with a minimum movement of distance filter
        /// </summary>
        event TypedEventHandler<GeolocatorWrapper, GeolocatorWrapperPositionChangedEventArgs> PositionChanged;

        void Start();
        void Stop();
        bool IsActive { get; }
        Geocoordinate GetStationaryLocation();
        void ChangeStationary(bool exitStationary);
    }

    public class GeolocatorWrapper : IGeolocatorWrapper
    {
        /// <summary>
        /// Geolocator and RunningInBackground are required properties to run in background
        /// For more information read http://msdn.microsoft.com/library/windows/apps/jj662935(v=vs.105).aspx
        /// </summary>
        private static Geolocator Geolocator { get; set; }

        /// <summary>
        /// Desired accuracy in meters
        /// </summary>
        private readonly UInt32 _desiredAccuracy;

        /// <summary>
        /// Report interval in milliseconds
        /// </summary>
        private readonly uint _reportInterval;

        /// <summary>
        /// Base distance filter (set via constructor) in meters
        /// </summary>
        private readonly double _distanceFilter;

        /// <summary>
        /// Stationary Radius in meters
        /// </summary>
        private readonly double _stationaryRadius;

        /// <summary>
        /// Automatically scaled distance filter in meters
        /// </summary>
        private double? _scaledDistanceFilter;

        /// <summary>
        /// Changing the ReportInterval fires the Geolocator.OnPositionChanged event. 
        /// Prevent a direct/second update of the DistanceFilter/ReportInterval directly after the current one 
        /// </summary>
        private bool _skipNextPosition;

        private bool _useFixedTimeInterval;
        private int _reportedPositionsCount;
        private int _reportedIntervalsPositionsCount;
        private UInt32 _intervalReportSeconds;
        private UInt32 _intervalReportMeters;
        private bool _reportTotalTime;
        private bool _reportTotalDistance;
        private bool _reportAveragePace;
        private bool _reportCurrentPace;
        private bool _reportInMiles;
        private bool _reportAverageSpeed;
        private bool _reportCurrentSpeed;
        private List<Notification> _notifications;
        private int _notificationIndex = 0;
        private int _notificationOffsetSeconds = 0;

        private readonly PositionPath _positionPath;
        private readonly StationaryManager _stationaryManager;
        public bool IsActive { get; private set; }
        public event TypedEventHandler<GeolocatorWrapper, GeolocatorWrapperPositionChangedEventArgs> PositionChanged;

        private enum StationaryUpdateResult
        {
            NotInStationary,
            InStationary,
            ExitedFromStationary
        }

        /// <param name="desiredAccuracy">In meters</param>
        /// <param name="reportInterval">In milliseconds</param>
        /// <param name="distanceFilter">In meters</param>
        /// <param name="stationaryRadius"></param>
        public GeolocatorWrapper(UInt32 desiredAccuracy, UInt32 reportInterval, double distanceFilter, 
            double stationaryRadius, bool useFixedTimeInterval, UInt32 intervalReportSeconds,
            UInt32 intervalReportMeters, bool reportTotalTime, bool reportTotalDistance, bool reportAveragePace,
            bool reportCurrentPace, bool reportAverageSpeed, bool reportCurrentSpeed, bool reportInMiles, List<Notification> notifications)
        {
            _desiredAccuracy       = desiredAccuracy;
            _reportInterval        = reportInterval;
            _distanceFilter        = distanceFilter;
            _stationaryRadius      = stationaryRadius;
            _positionPath          = new PositionPath();
            _stationaryManager     = new StationaryManager(stationaryRadius);
            _useFixedTimeInterval  = useFixedTimeInterval;
            _intervalReportSeconds = intervalReportSeconds;
            _intervalReportMeters  = intervalReportMeters;
            _reportTotalTime       = reportTotalTime;
            _reportTotalDistance   = reportTotalDistance;
            _reportAveragePace     = reportAveragePace;
            _reportCurrentPace     = reportCurrentPace;
            _reportAverageSpeed    = reportAverageSpeed;
            _reportCurrentSpeed    = reportCurrentSpeed;
            _reportInMiles         = reportInMiles;
            _notifications         = notifications;
        }

        public void Start()
        {
            if (Geolocator != null) Geolocator.PositionChanged -= OnGeolocatorPositionChanged;

            Geolocator = new Geolocator
            {
                // MovementThreshold 0 by purpose, is taken care of by this wrapper using current speed, ScaledDistanceFilter and ReportInterval
                MovementThreshold       = default(double),
                ReportInterval          = _reportInterval,
                DesiredAccuracyInMeters = _desiredAccuracy
            };

            Geolocator.PositionChanged += OnGeolocatorPositionChanged;
            IsActive = true;
        }

        public void Stop()
        {
            if (Geolocator == null) return;

            Geolocator.PositionChanged -= OnGeolocatorPositionChanged;
            Geolocator = null;
            IsActive   = false;
        }

        private void OnGeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs positionChangesEventArgs)
        {
            if (_skipNextPosition)
            {
                _skipNextPosition = false;
                return;
            }

            var newGeoCoordinate = new GeoCoordinate(positionChangesEventArgs.Position.Coordinate.Latitude, positionChangesEventArgs.Position.Coordinate.Longitude);
            var newPosition      = new Position(newGeoCoordinate, DateTime.Now, positionChangesEventArgs.Position.Coordinate.Accuracy);

            _positionPath.AddPosition(newPosition);

            var stationaryUpdateResult = StationaryUpdate(positionChangesEventArgs.Position, newPosition);
            if (stationaryUpdateResult == StationaryUpdateResult.InStationary && !_useFixedTimeInterval)
            {
                return;
            }

            var currentAvgSpeed = _positionPath.GetCurrentSpeed(TimeSpan.FromMilliseconds(_reportInterval * 5)); // avg speed of last 5 (at max) positions

            var updateScaledDistanceFilterResult = UpdateScaledDistanceFilter(currentAvgSpeed, positionChangesEventArgs.Position.Coordinate);
            if (updateScaledDistanceFilterResult.SkipPositionBecauseOfDistance && !_useFixedTimeInterval)
            {
                SkipPosition(updateScaledDistanceFilterResult.StartStationary, updateScaledDistanceFilterResult.StartStationary, updateScaledDistanceFilterResult.Distance);
                return;
            }
            
            if (updateScaledDistanceFilterResult.ScaledDistanceFilterChanged && !_useFixedTimeInterval)
            {
                var newReportInterval = CalculateNewReportInterval(currentAvgSpeed);
                UpdateReportInterval(newReportInterval);
            }

            _reportedPositionsCount++;
            _reportedIntervalsPositionsCount++;

            var geolocatorWrapperPositionChangedEventArgs = new GeolocatorWrapperPositionChangedEventArgs
            {
                GeolocatorLocationStatus = Geolocator.LocationStatus,
                Position                 = positionChangesEventArgs.Position,
                EnteredStationary        = false,
                PositionUpdateDebugData  = PostionUpdateDebugData.ForNewPosition(positionChangesEventArgs, currentAvgSpeed, updateScaledDistanceFilterResult, Geolocator.ReportInterval, stationaryUpdateResult == StationaryUpdateResult.ExitedFromStationary)
            };

            if (_notificationIndex < _notifications.Count)
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("geoLocatorWrapperOutput.txt", FileMode.Append, FileAccess.Write, IsolatedStorageFile.GetUserStoreForApplication()))
                {
                    using (StreamWriter writeFile = new StreamWriter(file))
                    {
                        writeFile.WriteLine("_reportedPositionsCount: " + _reportedPositionsCount);
                        writeFile.WriteLine("_notificationIndex: " + _notificationIndex);
                        writeFile.WriteLine("_notificationOffsetSeconds: " + _notificationOffsetSeconds);
                        writeFile.WriteLine("_notifications.Count: " + _notifications.Count);
                        writeFile.WriteLine("_notifications[_notificationIndex].intervalSeconds: " + _notifications[_notificationIndex].intervalSeconds);
                        writeFile.WriteLine("Current Time: " + (_reportInterval / 1000) * _reportedIntervalsPositionsCount);
                        writeFile.WriteLine("Next Interval Time: " + (_notifications[_notificationIndex].intervalSeconds + _notificationOffsetSeconds));
                        writeFile.Close();
                    }
                    file.Close();
                }

                if (_reportedPositionsCount > 1 && ((_reportInterval / 1000) * _reportedPositionsCount) >= (_notifications[_notificationIndex].intervalSeconds + _notificationOffsetSeconds))
                {
                    using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("geoLocatorWrapperOutput.txt", FileMode.Append, FileAccess.Write, IsolatedStorageFile.GetUserStoreForApplication()))
                    {
                        using (StreamWriter writeFile = new StreamWriter(file))
                        {
                            writeFile.WriteLine("_notifications[_notificationIndex].text: " + _notifications[_notificationIndex].text);
                            writeFile.Close();
                        }
                        file.Close();
                    }
                    _notificationOffsetSeconds += _notifications[_notificationIndex].intervalSeconds;
                    geolocatorWrapperPositionChangedEventArgs.NotiticationText = _notifications[_notificationIndex].text;
                    _notificationIndex++;
                }
                else
                {
                    geolocatorWrapperPositionChangedEventArgs.NotiticationText = "";
                }
            }
            else
            {
                geolocatorWrapperPositionChangedEventArgs.NotiticationText = "";
            }

            if (_intervalReportSeconds > 0 && ((_reportInterval / 1000) * _reportedIntervalsPositionsCount) == _intervalReportSeconds)
            {
                _reportedIntervalsPositionsCount = 0;
                geolocatorWrapperPositionChangedEventArgs.SpeachReportReady = true;
                //Get Average speed
                geolocatorWrapperPositionChangedEventArgs.AverageSpeed = 
                    _positionPath.GetCurrentSpeed(TimeSpan.FromMilliseconds(_reportInterval * _reportedPositionsCount));

                //Get Current speed
                geolocatorWrapperPositionChangedEventArgs.CurrentSpeed = 
                    _positionPath.GetCurrentSpeed(TimeSpan.FromSeconds(_intervalReportSeconds));

                if (_reportInMiles)
                {
                    //Convert Average speed to MPH
                    geolocatorWrapperPositionChangedEventArgs.AverageSpeed = 
                        geolocatorWrapperPositionChangedEventArgs.AverageSpeed * 2.23694;

                    //Convert Current speed to MPH
                    geolocatorWrapperPositionChangedEventArgs.CurrentSpeed =
                        geolocatorWrapperPositionChangedEventArgs.CurrentSpeed * 2.23694;

                    //Get Total Distance
                    geolocatorWrapperPositionChangedEventArgs.TotalDistance =
                        _positionPath.GetTotalDistance(TimeSpan.FromMilliseconds(_reportInterval * _reportedPositionsCount)) * 0.000621371;
                }

                //Get Average Pace
                geolocatorWrapperPositionChangedEventArgs.AveragePace =
                    60 / geolocatorWrapperPositionChangedEventArgs.AverageSpeed;

                //Get Current Pace
                geolocatorWrapperPositionChangedEventArgs.CurrentPace =
                    60 / geolocatorWrapperPositionChangedEventArgs.CurrentSpeed;

                //Get Total Time
                geolocatorWrapperPositionChangedEventArgs.TotalTime = 
                    TimeSpan.FromMilliseconds(_reportInterval * _reportedPositionsCount);                
            }
            PositionChanged(this, geolocatorWrapperPositionChangedEventArgs);
        }

        private void SkipPosition(bool becauseOfEnteringStationary, bool startStationary, double? distance)
        {
            var updateType = becauseOfEnteringStationary ? PositionUpdateType.EnteringStationary : PositionUpdateType.SkippedBecauseOfDistance;

            PositionChanged(this, new GeolocatorWrapperPositionChangedEventArgs
            {
                EnteredStationary = startStationary,
                PositionUpdateDebugData = PostionUpdateDebugData.ForSkip(updateType, distance, _distanceFilter, _stationaryRadius)
            });
        }

        public Geocoordinate GetStationaryLocation()
        {
            return !_stationaryManager.InStationary ? null : _stationaryManager.GetStationaryGeocoordinate();
        }

        public void ChangeStationary(bool exitStationary)
        {
            if (exitStationary)
            {
                _stationaryManager.ExitStationary();
                Start();
            }
            else
            {
                throw new NotImplementedException("Manually starting stationary not implemented yet");
            }
        }

        private StationaryUpdateResult StationaryUpdate(Geoposition geoPosition, Position newPosition)
        {
            if (!_stationaryManager.InStationary) return StationaryUpdateResult.NotInStationary;

            var newStationaryReportInterval = _stationaryManager.GetNewReportInterval(newPosition);

            if (!newStationaryReportInterval.HasValue || !_stationaryManager.InStationary) return StationaryUpdateResult.ExitedFromStationary;

            PositionChanged(this, new GeolocatorWrapperPositionChangedEventArgs
            {
                GeolocatorLocationStatus = Geolocator.LocationStatus,
                Position                 = geoPosition,
                PositionUpdateDebugData  =
                    PostionUpdateDebugData.ForStationaryUpdate((uint)newStationaryReportInterval,
                        _stationaryManager.GetDistanceToStationary(newPosition))
            });

            if (!_useFixedTimeInterval)
            {
                // stay in stationary
                UpdateReportInterval((uint)newStationaryReportInterval);
            }

            return StationaryUpdateResult.InStationary;
        }

        private UpdateScaledDistanceFilterResult UpdateScaledDistanceFilter(double? currentAvgSpeed, Geocoordinate geocoordinate)
        {
            var result       = new UpdateScaledDistanceFilterResult(_scaledDistanceFilter.HasValue ? _scaledDistanceFilter.Value : 0);
            var lastPosition = _positionPath.GetLastPosition();
            result.Distance  = lastPosition.DinstanceToPrevious;

            if (!lastPosition.Speed.HasValue || !currentAvgSpeed.HasValue)
            {
                _scaledDistanceFilter = _distanceFilter;
                return result;
            }

            if (lastPosition.DinstanceToPrevious.HasValue && (lastPosition.DinstanceToPrevious.Value < _distanceFilter || lastPosition.DinstanceToPrevious.Value < _stationaryRadius))
            {
                // too little movement, start Stationary and/or skip this position update

                if (lastPosition.DinstanceToPrevious.Value < _stationaryRadius)
                {
                    _stationaryManager.StartStationary(lastPosition, geocoordinate);
                    result.StartStationary = true;
                }
                result.SkipPositionBecauseOfDistance = true;
                return result;
            }

            result.NewScaledDistanceFilter = CalculateNewScaledDistanceFilter(currentAvgSpeed.Value);
            _scaledDistanceFilter          = result.NewScaledDistanceFilter;
            return result;
        }

        private void UpdateReportInterval(uint reportInterval)
        {
            _skipNextPosition = true;

            // Windows Phone suspends the app when all eventhandlers of all GeoLocator objects are removed (only in background mode)
            // Wire up a temporary Geolocator to prevent the app from closing
            var tempGeolocator = new Geolocator
            {
                MovementThreshold = 1,
                ReportInterval    = 1
            };
            TypedEventHandler<Geolocator, PositionChangedEventArgs> dummyHandler = (sender, positionChangesEventArgs2) => { };
            tempGeolocator.PositionChanged += dummyHandler;

            // It is not allowed to change properties of Geolocator when eventhandlers are attached 
            Geolocator.PositionChanged -= OnGeolocatorPositionChanged;
            Geolocator.ReportInterval   = reportInterval;
            Geolocator.PositionChanged += OnGeolocatorPositionChanged;

            tempGeolocator.PositionChanged -= dummyHandler;
        }

        private double CalculateNewScaledDistanceFilter(double currentSpeed)
        {
            var newDistanceFilter = _distanceFilter;
            if (currentSpeed > 100)
            {
                return newDistanceFilter;
            }

            var speedRoundedToNearesFive = RoundToNearestFactor(currentSpeed, 5);
            var squareRouteOfSpeed       = Math.Pow(speedRoundedToNearesFive, 2);
            newDistanceFilter            = squareRouteOfSpeed + newDistanceFilter;

            if (newDistanceFilter > 1000) newDistanceFilter = 1000;

            return newDistanceFilter;
        }

        /// <returns>New report interval in milliseconds</returns>
        private uint CalculateNewReportInterval(double? currentAvgSpeed)
        {
            var defaultReportInterval = _reportInterval;

            if (!currentAvgSpeed.HasValue || Math.Abs(currentAvgSpeed.Value) < 0.1) return defaultReportInterval;

            var newReportInterval = (_scaledDistanceFilter / currentAvgSpeed.Value) * 1000;
            if (newReportInterval > UInt32.MaxValue) newReportInterval = UInt32.MaxValue;

            // Limit new Report Interval to 10 * defaultReportInterval 
            if (newReportInterval > (10 * defaultReportInterval)) newReportInterval = (10 * _reportInterval);

            // Limit new Report Interval to one hour 
            if (newReportInterval > TimeSpan.FromHours(1).TotalMilliseconds) newReportInterval = TimeSpan.FromHours(1).TotalMilliseconds;

            return newReportInterval > defaultReportInterval ? Convert.ToUInt32(newReportInterval) : defaultReportInterval;
        }

        private int RoundToNearestFactor(double value, int factor)
        {
            return (int)Math.Round((value / factor), MidpointRounding.AwayFromZero) * factor;
        }
    }
}
