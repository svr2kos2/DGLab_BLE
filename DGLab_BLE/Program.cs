using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Runtime;
using System.Timers;
using System.Threading;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.IO;
using Windows.Storage.Streams;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using DGLab;

namespace BLE_HeartBeat {
    class Program {
        static float lBreast = 0;
        static float rBreast = 0;

        static float touchSelf = 0;
        static float touchOthers = 0;

        static IPEndPoint localIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9001);
        static IPEndPoint remotelpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);
        static UdpClient udp = new UdpClient(localIpep);

        static void Main(string[] args) {
            DGLab_BLE dgLab = new DGLab_BLE();
            udp.BeginReceive(RecvData, null);

            dgLab.SetLevel(DGLab_BLE.Channel.A, 0);
            dgLab.SetLevel(DGLab_BLE.Channel.B, 0);
            
            for (; ; ) {
                Console.WriteLine(DateTime.Now + " " + touchSelf + " " + lBreast + " " + rBreast);
                var breast = Math.Max(lBreast, rBreast);
                if (breast > 0.01f) {
                    var t = breast / 0.1666f;
                    dgLab.SetLevel(DGLab_BLE.Channel.B, (int)(100 + t * 400));
                    dgLab.SetWave(DGLab_BLE.Channel.B, new byte[] { 0x61, 0x81, 0x02 });
                }

                var touch = Math.Max(touchSelf, touchOthers);
                if (touch > 0.1f) {
                    dgLab.SetLevel(DGLab_BLE.Channel.A, (int)(100 + touch * 400));
                    dgLab.SetWave(DGLab_BLE.Channel.A, new byte[] { 0x61, 0x81, 0x02 });
                }
                
                Thread.Sleep(100);
            }
        }

        static void RecvData(IAsyncResult ar) {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var receivedData = udp.EndReceive(ar, ref remoteEndPoint).ToList();
            List<byte> addressBytes = new List<byte>();
            int i = 0;
            for (; i < receivedData.Count; ++i) {
                if (receivedData[i] == ',')
                    break;
                addressBytes.Add(receivedData[i]);
            }
            var address = Encoding.UTF8.GetString(addressBytes.ToArray());
            i += 2;
            var type = receivedData[i];
            i += 2;
            var valueBytes = receivedData.GetRange(i, receivedData.Count - i);
            valueBytes.Reverse();
            float value = 0;
            if (valueBytes != null && valueBytes.Count >= 4)
                value = BitConverter.ToSingle(valueBytes.ToArray(), 0);
            if (address.Contains("LBreast_Angle"))
                lBreast = value;
            if (address.Contains("RBreast_Angle"))
                rBreast = value;
            if (address.Contains("Oriface_Hole/TouchSelf"))
                touchSelf = value;
            if (address.Contains("Oriface_Hole/TouchOthers"))
                touchOthers = value;
            udp.BeginReceive(RecvData, null);
        }
    }
}
