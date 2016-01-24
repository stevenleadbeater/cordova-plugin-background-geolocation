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
                returnValue += string.Format(" {0} hours ", timeSpan.ToString("%h"));
            }
            if (timeSpan >= new TimeSpan(0, 1, 0))
            {
                returnValue += string.Format(" {0} minutes ", timeSpan.ToString("%m"));
            }
            if (timeSpan >= new TimeSpan(0, 0, 1))
            {
                returnValue += string.Format(" {0} seconds ", timeSpan.ToString("%s"));
            }

            return returnValue;
        }
    }
}