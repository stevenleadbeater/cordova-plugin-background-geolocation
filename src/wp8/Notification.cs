using System;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using WPCordovaClassLib.Cordova.Commands;

namespace Cordova.Extension.Commands
{
    public class Notification
    {
        public int index { get; set; }
        public string text { get; set; }
        public int intervalSeconds { get; set; }
    }
}