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

namespace BLE_HeartBeat
{
    class Program
    {

        static void Main(string[] args)
        {
            DGLab_BLE dgLab = new DGLab_BLE();
            var level = dgLab.GetLevel();
            var b = dgLab.ReadBytes(dgLab.level);
            Console.WriteLine(BitConverter.ToString(b));
            Console.WriteLine(level.Item1.ToString("X2"));
            Console.WriteLine(level.Item2.ToString("X2"));


            dgLab.SetLevel(DGLab_BLE.Channel.A, 0); //max 63
            dgLab.SetLevel(DGLab_BLE.Channel.B, 0); //max 63
            for (; ; ) {
                var cmd = Console.ReadLine();
                if(int.TryParse(cmd,out var res)) {
                    dgLab.SetLevel(DGLab_BLE.Channel.A, (byte)res);
                } else {
                    var bt = dgLab.ReadBytes(dgLab.level);
                    Console.WriteLine(BitConverter.ToString(bt));
                    dgLab.SetWave(DGLab_BLE.Channel.A, new byte[] { 0x0A, 0x08, 0x05 });
                }
            }


            dgLab.SetLevel(DGLab_BLE.Channel.A,8); //max 63
            dgLab.SetLevel(DGLab_BLE.Channel.B,7); //max 63
            dgLab.SetWave(DGLab_BLE.Channel.B, new byte[] { 0x0A, 0x08, 0x05 });
        }


    }
}
