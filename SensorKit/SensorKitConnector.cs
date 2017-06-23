using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using PCLStorage;
using System.IO;
using System.Numerics;

namespace SensorKit
{
    public class SensorKitConnector : ViewModel
    {

        // Sensor Kit services
        public static Guid SENSORKIT_GATT_SERVICE = new Guid("1cccbdda-ca25-4c59-b68a-4fb2d90c0d0c");
        // COMMAND
        public static Guid SENSORKIT_GATT_WRITE_COMMAND = new Guid("1C680CDC-C0CB-4DF8-BA2B-B800BA0C0D0C");
        public static Guid SENSORKIT_GATT_NOTIFY_COMMAND = new Guid("1C7359C1-67EE-4C93-A401-14A6420C0D0C");
        // LED CHARACTERISTICS
        public static Guid SENSORKIT_GATT_READ_LED_COLOR = new Guid("1C69D4A1-BD32-4059-9E6C-E6DBDF0C0D0C");
        // SENSOR CHARACTERISTICS
        public static Guid SENSORKIT_GATT_NOTIFY_SENSOR = new Guid("1C7059C1-67EE-4C93-A401-14A6420C0D0C");
        public static Guid SENSORKIT_GATT_READ_SENSOR = new Guid("1C71D4A1-BD32-4059-9E6C-E6DBDF0C0D0C");


        private GattCharacteristic _writeCommandCharacteristic;
        private GattCharacteristic _readColorCharacteristic;
        private GattCharacteristic _notifyCommandCharacteristic;

        private GattCharacteristic _notifySensorCharacteristic;
        private GattCharacteristic _readSensorCharacteristic;

        string COMMAND_NIL = "00";
        string COMMAND_MUTE = "01";
        string COMMAND_COLOR = "02";
        string COMMAND_UNMUTE = "03";
        string COMMAND_START_EXPERIMENT = "04";
        string COMMAND_STOP_EXPERIMENT = "05";

        bool _isConnected = false;
        public bool IsConnected {
            get {
                return _isConnected;
            }
            set
            {
                SetValue(ref _isConnected, value, "IsConnected");
            }
        }
        public bool IsRegisteredForNotifications { get; set; }

        public bool IsColorWriteEnabled
        {
            get
            {
                return (_writeCommandCharacteristic != null) ? true : false;
            }
        }

        
        bool isUpdating = false;

        SensorModel Data;
        SensorKit Controller;

        public SensorKitConnector(SensorKit controller, SensorModel data)
        {
            Controller = controller;
            Data = data;
        }

        

        bool _isMute = false;
       

        public bool IsMute
        {
            get { return _isMute; }
            set
            {
                if (!isUpdating && value != _isMute)
                {
                    isUpdating = true;
                    SetValue(ref _isMute, value, "IsMute");
                    Task.Run(async () =>
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await SetMute(_isMute);
                        });
                    });
                }
                isUpdating = false;
            }

        }

        private Color _currentColor = Color.FromArgb(0, 0, 148, 255); // 0094FF
        public Color ActualColor
        {
            get { return _currentColor; }
            set
            {
                if (!isUpdating && value != _currentColor)
                {
                    isUpdating = true;
                    SetValue(ref _currentColor, value, "ActualColor");
                    Task.Run(async () =>
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await SetColor(_currentColor);
                        });
                    });
                }
                isUpdating = false;
            }

        }

        public async Task Subscribe()
        {
           
            try
            {
                Debug.WriteLine($"***SUBSCRIBING {Data.Name}...");

                if (Data.BluetoothAddress != 0)
                {

                    var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(Data.BluetoothAddress);
                    var devAccessStatus = await bleDevice.RequestAccessAsync();
                    var serviceResult = await bleDevice.GetGattServicesForUuidAsync(SENSORKIT_GATT_SERVICE);

                    var deviceSvc = serviceResult.Services.FirstOrDefault();
                    if (deviceSvc != null)
                    {
                        var characteristics = await deviceSvc.GetCharacteristicsAsync();

                        _writeCommandCharacteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == SENSORKIT_GATT_WRITE_COMMAND);
                        _readColorCharacteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == SENSORKIT_GATT_READ_LED_COLOR);
                        _notifySensorCharacteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == SENSORKIT_GATT_NOTIFY_SENSOR);
                        _readSensorCharacteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == SENSORKIT_GATT_READ_SENSOR);
                        _notifyCommandCharacteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == SENSORKIT_GATT_NOTIFY_COMMAND);

                        if (_notifySensorCharacteristic != null)
                        {
                            _notifySensorCharacteristic.ValueChanged += SensorNotifyCharacteristic_ValueChanged;
                            var registerForNotificationsResult = await _notifySensorCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            if (registerForNotificationsResult == GattCommunicationStatus.Success)
                            {
                                IsRegisteredForNotifications = true;
                            }
                        }

                        if (_notifyCommandCharacteristic != null)
                        {
                            _notifyCommandCharacteristic.ValueChanged += _notifyCommandCharacteristic_ValueChanged;
                            var registerForNotificationsResult = await _notifyCommandCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        }

                        //var color = await GetColor();
                        //if (color != null)
                        //    _currentColor = color.Value;

                        IsConnected = true;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            Data.NotifyPropertyChanged("IsSubscribed");
                        });

                        //var data = await GetSensorData();

                    }
                    else
                    {
                        // Custom GATT Service Not Found on the Bluefruit";
                    }

                }
            }catch(Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        public void Unsubscribe()
        {
            if(_notifySensorCharacteristic != null)
            {
                _notifySensorCharacteristic.ValueChanged -= SensorNotifyCharacteristic_ValueChanged;
            }
            if (_notifyCommandCharacteristic != null)
            {
                _notifyCommandCharacteristic.ValueChanged -= _notifyCommandCharacteristic_ValueChanged;
            }
        }

        DateTime _lastButtonReceived = DateTime.MinValue;

        private async void _notifyCommandCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (DateTime.Now - _lastButtonReceived < TimeSpan.FromSeconds(5))
                return; // ignore

            _lastButtonReceived = DateTime.Now;
            try{
                if(args?.CharacteristicValue?.Length > 0){
                    byte[] bytes = new byte[args.CharacteristicValue.Length];
                    DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bytes);
                    var str = BitConverter.ToString(bytes);
                    Debug.WriteLine($"*** BUTTON *** {str}");
                    if (SensorKit.Settings.IsExperimentOn)
                    {
                       Controller.Stop();
                    }
                    else
                    {
                        if (SensorKit.Settings.IsTrackingOn)
                        {
                            Controller.Start();
                        }
                        else
                        {
                            Debug.WriteLine($"Please, enable tracking first");
                        }
                    }
                }
            }catch(Exception x){
                Debug.WriteLine(x);
            }
            
        }

        private async void SensorNotifyCharacteristic_ValueChanged(GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            try
            {
                byte[] bytes = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bytes);
                var reading = BytesToSensorData(bytes);
                if (reading!= null && Data != null && SensorKit.Settings.IsExperimentOn && SensorKit.Settings.Experiment > 0)
                {
                        Data.Append(reading);
                }
                
            }catch(Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        public async Task SetMute(bool mute)
        {
            await WriteCommand((mute) ? $"{COMMAND_MUTE}000000" : $"{COMMAND_UNMUTE}000000");
        }

        public async Task SetColor(Color color)
        {
            await WriteCommand($"{COMMAND_COLOR}{color.R:X2}{color.G:X2}{color.B:X2}");
        }

        public async Task WriteCommand(string cmd)
        {
            try{
            if (_writeCommandCharacteristic != null)
            {
                var writer = new DataWriter();
                writer.WriteString(cmd);
                Debug.WriteLine($"Writing command to Writable GATT characteristic ... {cmd}");
                var result = await _writeCommandCharacteristic.WriteValueAsync(writer.DetachBuffer());
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Success");
                }
            }
            }catch(Exception x){
                Debug.WriteLine(x);
            }
        }

        public async Task<Color?> GetColor()
        {
            try{
            if (_readColorCharacteristic != null)
            {
                var result = await _readColorCharacteristic.ReadValueAsync();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    if (result.Value != null && result.Value.Length > 0)
                    {
                        byte[] bArray = new byte[result.Value.Length];
                        DataReader.FromBuffer(result.Value).ReadBytes(bArray);
                        var color = Color.FromArgb(0, bArray[0], bArray[1], bArray[2]);
                        return color;
                    }
                }
                //There was a problem reading the GATT characteristic";
            }
            }catch(Exception x){
                Debug.WriteLine(x);
            }
            return null;
        }

        public async Task<SensorItem> GetSensorData()
        {
            try{
            if (_readSensorCharacteristic != null)
            {
                var result = await _readSensorCharacteristic.ReadValueAsync();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    if (result.Value != null && result.Value.Length > 0)
                    {
                        byte[] bArray = new byte[result.Value.Length];
                        DataReader.FromBuffer(result.Value).ReadBytes(bArray);
                        return BytesToSensorData(bArray);
                    }
                }
                //There was a problem reading the GATT characteristic";
            }
            }catch(Exception x){
                Debug.WriteLine(x);
            }
            return null;
        }

        private SensorItem BytesToSensorData(byte[] bArray)
        {
            SensorItem reading = null;

            try{

                if (bArray != null && bArray.Length >= 12)
                {
                    //foreach (var b in bArray)
                    //{
                    //    Debug.Write($"{b:X2}-");
                    //}
                    //Debug.WriteLine("");
                    // convert values to float

                    bool isRecognized = false;

                    var dataType = BitConverter.ToChar(bArray, 0);

                    Quaternion q = new Quaternion();
                    double ax = 0.0;
                    double ay = 0.0;
                    double az = 0.0;

                    double lat = 0.0;
                    double lon = 0.0;
                    double speed = 0.0;
                    double altitude = 0.0;
                    double incline = 0.0;

                    if (dataType == 'Q' || (char)bArray[0] == 'Q')
                    {
                        q = new Quaternion
                        {

                            X = BitConverter.ToSingle(bArray, 1),
                            Y = BitConverter.ToSingle(bArray, 5),
                            Z = BitConverter.ToSingle(bArray, 9),
                            W = BitConverter.ToSingle(bArray, 13)
                        };
                        isRecognized = true;
                    
                    }else if(dataType == 'A' || (char)bArray[0] == 'A')
                    {
                        ax = BitConverter.ToSingle(bArray, 1);
                        ay = BitConverter.ToSingle(bArray, 5);
                        az = BitConverter.ToSingle(bArray, 9);
                        isRecognized = true;
                    }

                    if (isRecognized)
                    {
                        if (Controller.FusionData != null)
                        {
                            lat = Controller.FusionData.lat;
                            lon = Controller.FusionData.lon;
                            speed = Controller.FusionData.speed;
                            altitude = Controller.FusionData.alt;
                            incline = Controller.FusionData.incl;
                        }

                        reading = new SensorItem
                        {
                            timestamp = DateTimeOffset.Now,
                            aX = az,
                            aY = ay,
                            aZ = az,
                            qX = q.X,
                            qY = q.Y,
                            qZ = q.Z,
                            qW = q.W,
                            lat = lat,
                            lon = lon,
                            speed = speed,
                            alt = altitude,
                            incl = incline
                        };

                        Debug.WriteLine($"*** {Data.Name} Q X:{reading.qX} Y:{reading.qY} Z:{reading.qZ} W:{reading.qW} A: X:{reading.aX} Y:{reading.aY} Z:{reading.aZ}");
                        return reading;
                    }
                
                }
            }catch(Exception x){
                Debug.WriteLine(x);
            }

            return null;
        }


    }
}
