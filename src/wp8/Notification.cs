using System;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using WPCordovaClassLib.Cordova.Commands;

namespace Cordova.Extension.Commands
{
    public class Notificaiton
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public int IntervalSeconds { get; set; }
    }
}