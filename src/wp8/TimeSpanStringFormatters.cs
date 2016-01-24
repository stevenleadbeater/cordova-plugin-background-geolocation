using System;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using WPCordovaClassLib.Cordova.Commands;

namespace Cordova.Extension.Commands
{
    public static class TimeSpanStringFormatters
    {
        public static string GetSpeechFormat(this TimeSpan timeSpan)
        {
            var returnValue = "";

            if(timeSpan >= new TimeSpan(1, 0, 0))
            {
                var hours = timeSpan.ToString("%h");
                if (hours != "0")
                {
                    returnValue += string.Format(" {0} hours ", hours);
                }
            }
            if (timeSpan >= new TimeSpan(0, 1, 0))
            {
                var minutes = timeSpan.ToString("%m");
                if (minutes != "0")
                {
                    returnValue += string.Format(" {0} minutes ", minutes);
                }
            }
            if (timeSpan >= new TimeSpan(0, 0, 1))
            {
                var seconds = timeSpan.ToString("%s");
                if (seconds != "0")
                {
                    returnValue += string.Format(" {0} seconds ", seconds);
                }
            }

            return returnValue;
        }
    }
}