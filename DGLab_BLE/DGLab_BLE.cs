using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Threading;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;


public class DGLab_BLE {
    public DeviceInformation device;
    public GattDeviceService battrySV;
    public GattDeviceService controlSV;
    public GattCharacteristic battry;
    public GattCharacteristic level; //04 A 0-5 | B 6-11
    public GattCharacteristic waveA; //06 
    public GattCharacteristic waveB; //05 

    public DGLab_BLE() {
        try {
            var battrySVSelector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid("955A180A-0FE2-F5AA-A094-84B8D4F3E8AD"));
            var controlSVSelector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid("955A180B-0FE2-F5AA-A094-84B8D4F3E8AD"));
            battrySV = AsyncResult(GattDeviceService.FromIdAsync(AsyncResult(DeviceInformation
                .FindAllAsync(battrySVSelector)).First().Id));
            controlSV = AsyncResult(GattDeviceService.FromIdAsync(AsyncResult(DeviceInformation
                .FindAllAsync(controlSVSelector)).First().Id));

            battry = AsyncResult(battrySV.GetCharacteristicsForUuidAsync(new Guid("955A1500-0FE2-F5AA-A094-84B8D4F3E8AD"))).Characteristics.FirstOrDefault();
            level = AsyncResult(controlSV.GetCharacteristicsForUuidAsync(new Guid("955A1504-0FE2-F5AA-A094-84B8D4F3E8AD"))).Characteristics.FirstOrDefault();
            waveA = AsyncResult(controlSV.GetCharacteristicsForUuidAsync(new Guid("955A1506-0FE2-F5AA-A094-84B8D4F3E8AD"))).Characteristics.FirstOrDefault();
            waveB = AsyncResult(controlSV.GetCharacteristicsForUuidAsync(new Guid("955A1505-0FE2-F5AA-A094-84B8D4F3E8AD"))).Characteristics.FirstOrDefault();
        } catch {
            Console.WriteLine("BLE Connection faild.");
        }
    }


    ~DGLab_BLE() {
        battrySV.Dispose();
        controlSV.Dispose();
    }

    public enum Channel { 
        A,
        B
    }

    public byte[] ReadBytes(GattCharacteristic ch) {
        var buffer = AsyncResult(ch.ReadValueAsync()).Value;
        byte[] current = new byte[buffer.Length];
        DataReader.FromBuffer(buffer).ReadBytes(current);
        return current;
    }

    public void WriteBytes(GattCharacteristic ch, byte[] value) {
        IBuffer buffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(value);
        _ = ch.WriteValueAsync(buffer);
    }

    public (int,int) GetLevel() {
        var bytes = ReadBytes(level);
        var levelA = bytes[2] & 0x3F;
        var levelB = (bytes[2] >> 6) | ((bytes[1] & 0x0F) << 2);
        return (levelA, levelB);
    }

    public void SetLevel(Channel ch,byte value) {
        var bytes = ReadBytes(level);
        if(ch == Channel.A) {
            bytes[2] = (byte)((bytes[2] & 0xC0) | (value & 0x3F));
        } else {
            bytes[2] = (byte)((bytes[2] & 0x3F) | (value << 6));
            bytes[1] = (byte)(value >> 2);
        }
        WriteBytes(level, bytes);
    }

    public void SetWave(Channel ch, byte[] value) {
        IBuffer buffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(value);
        if (ch == Channel.A) {
            _ = waveA.WriteValueAsync(buffer);
        } else {
            _ = waveB.WriteValueAsync(buffer);
        }
    }


    private static T AsyncResult<T>(IAsyncOperation<T> async) {
        while (true) {
            switch (async.Status) {
                case AsyncStatus.Started:
                    Thread.Sleep(10);
                    continue;
                case AsyncStatus.Completed:
                    return async.GetResults();
                case AsyncStatus.Error:
                    throw async.ErrorCode;
                case AsyncStatus.Canceled:
                    throw new TaskCanceledException();
            }
        }
    }
}