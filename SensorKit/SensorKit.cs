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
using System.Collections.ObjectModel;

namespace SensorKit
{

    public class SensorKit : ViewModel
    {
        const string AZURE_STORAGE_ACCOUNT_NAME = null; // "<ADD YOUR AZURE STORAGE ACCOUNT NAME>";
        const string AZURE_STORAGE_ACCOUNT_KEY = null; // "<ADD YOUR AZURE STORAGE ACCOUNT KEY>";
        const string AZURE_STORAGE_CONTAINER = null; // "<ADD YOUR AZURE STORAGE CONTAINER NAME>";

        const string DEVICE_PREFIX = "SensorKit";
        public static string folderPrefix = "sensorKit";
        public static string currentFolderPath;

        public SensorsRegistry Registry { get; set; } = new SensorsRegistry();

        public ObservableCollection<SensorModel> Data { get; set; } = new ObservableCollection<SensorModel>();
        // allocate one model for local on-phone sensors + all connected sensors
        public SensorModel LocalData { get; set; }
        BluetoothLEAdvertisementWatcher watcher;

        public static Settings Settings { get; set; } = new Settings();

        TimerHelper _timerRecording;
        TimerHelper _timerScanning;

        public static string HUB_SENSOR_ID = "local";

        bool isScanning = false;

        public SensorItem FusionData { get; set; }

        public int Count
        {
            get
            {
                if (Data != null)
                    return Data.Count;
                else
                    return 0;
            }
        }

        public int CountSubscribed
        {
            get
            {
                if (Data != null)
                    return Data.Count(s=>s.IsSubscribed);
                else
                    return 0;
            }
        }

        public bool IsStarted
        {
            get
            {
                return Settings.IsExperimentOn;
            }
        }

        public int RecordingSequence
        {
            get
            {
                return Settings.Experiment;
            }
        }

        public string UserId { get; set; }

        public SensorKit(string userId)
        {
            UserId = userId;
            // automatically add local model for the local on-phone sensors
            LocalData = new SensorModel(this, Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Replace("_", "-"))
            {
                Id = SensorKit.HUB_SENSOR_ID, // local
                UserId = userId,
                Information = Registry.Public.FirstOrDefault(d => d.Capabilities == SensorCapabilities.Hub) // local device is a hub
            };
            Data.Add(LocalData);
            Task.Run(() =>
            {
                // setup watchers on non UI thread
                watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
                // If we haven't seen an advertisement for 3 seconds, consider the device out of range
                watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(3000);
                // If we see an advertisement from a device with an RSSI less than -120, consider it out of range
                watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -120;
                // Register our callbacks
                watcher.Received += DeviceFound;
                //deviceWatcher = DeviceInformation.CreateWatcher();
                //deviceWatcher.Added += DeviceAdded;
                //deviceWatcher.Updated += DeviceUpdated;
            });
            _timerScanning = new TimerHelper();
        }

        

        private void _timerRecording_Elapsed(object sender, EventArgs e)
        {
            // automatically stop the experiment after 5 minutes
            Debug.WriteLine("MAXIMUM TIME ELAPSED: STOPPING SENSOR DATA...");
            try
            {
                Stop();
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        public void Start()
        {
            Debug.WriteLine("STARTING SENSOR TRACKING...");
            try
            {
                currentFolderPath = $"{FileSystem.Current.LocalStorage.Path}/{SensorKit.folderPrefix}_{ DateTime.Now:yyyyMMdd}";
                var dir = Directory.CreateDirectory(currentFolderPath);
                if (Settings.LastExperimentDate.Date < DateTime.Now.Date)
                {
                    Settings.Experiment = 0; // reset experiment to 0
                }
                else
                {
                    Settings.Experiment += 1; // automatically increment experiment number
                }
                foreach (var model in Data)
                {
                    model.Start();
                }
                NotifyPropertyChanged("RecordingSequence");
                Debug.WriteLine($"Started sensor tracking experiment {Settings.Experiment}.");
                Settings.IsExperimentOn = true;
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        public void Stop()
        {
            try
            {
                Debug.WriteLine("STOP SENSOR DATA...");
                foreach (var model in Data)
                {
                    model.Stop();
                }
                Debug.WriteLine($"Stopped sensor tracking experiment {Settings.Experiment}.");
                Task.Run(async () =>
                {
                    await UploadMissingSensorDataAsync();
                });
                Settings.IsExperimentOn = false;
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }


        bool isUploading = false;

        public async Task UploadMissingSensorDataAsync()
        {
            Debug.WriteLine("UPLOADING SENSOR DATA...");

            if (isUploading)
                return;

            isUploading = true;

            foreach (var model in Data)
            {

                if (Settings.IsOnline && currentFolderPath != null)
                {
                    try
                    {
                        var folder = await FileSystem.Current.LocalStorage.GetFolderAsync($"{folderPrefix}_{DateTime.Now:yyyyMMdd}");
                        if (folder != null && AZURE_STORAGE_ACCOUNT_KEY != null && AZURE_STORAGE_ACCOUNT_NAME != null && AZURE_STORAGE_CONTAINER != null)
                        {
                            var processedFolder = await folder.CreateFolderAsync("processed", PCLStorage.CreationCollisionOption.OpenIfExists);
                            var files = await folder.GetFilesAsync();
                            if (files != null && files.Count() > 0)
                            {
                                var uploader = new AzureUploader(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(AZURE_STORAGE_ACCOUNT_NAME, AZURE_STORAGE_ACCOUNT_KEY), AZURE_STORAGE_CONTAINER);
                                foreach (var file in files)
                                {
                                    if (file.Name.EndsWith(".csv.zip"))
                                    {
                                        try
                                        {
                                            await uploader.UploadFileAsync(file.Name, file.Path);
                                            await file.MoveAsync($"{processedFolder.Path}/{file.Name}", PCLStorage.NameCollisionOption.ReplaceExisting);
                                        }
                                        catch (Exception x)
                                        {
                                            Debug.WriteLine(x);
                                        }
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            isUploading = false;
        }

       
       

        public void StartScanning()
        {
            if (!isScanning)
            {
                Debug.WriteLine($"START SCANNING");
                try
                {

                    watcher.Start();
                    //deviceWatcher.Start();
                    if (_timerScanning == null)
                    {
                        // configure timer
                        _timerScanning.Elapsed += _timerScanning_Elapsed;
                        _timerScanning.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;
                    }
                    _timerScanning.Start();
                    isScanning = true;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x);
                }
            }
        }

        private void _timerScanning_Elapsed(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("AUTO STOP SCANNING");
                // stop scanning
                _timerScanning.Stop();
                StopScanning();
            }
            catch(Exception x) {
                Debug.WriteLine(x);
            }
        }

        public void StopScanning()
        {
            try
            {
                if (isScanning)
                {
                    Debug.WriteLine($"STOPPING SCANNING");
                    _timerScanning.Stop();
                    watcher.Stop();
                    //deviceWatcher.Stop();
                    isScanning = false;
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        private async void DeviceFound(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            try
            {
                var name = btAdv.Advertisement.LocalName;
                if (name != null && name.Contains("SensorKit") && !Data.Any(d => d.Name == name))
                {
                    await AddSensor(name, btAdv.BluetoothAddress);
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        bool isAddingSensor = false;

        async Task AddSensor(string name, ulong bluetoothAddress)
        {
            if (isAddingSensor)
                return;

            isAddingSensor = true;
            try
            {
                SensorModel sensor = null;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                //haven't seen this device yet, add it to known devices list
                sensor = new SensorModel(this, name)
                    {
                        BluetoothAddress = bluetoothAddress,
                        UserId = UserId
                    };
                    sensor.Information = Registry.Public.FirstOrDefault(s => s.Model == sensor.SensorType);
                    Data.Add(sensor);
                });

                Debug.WriteLine($"SENSOR ADDED {sensor.Name}");

                // check for known sensor tag
                var tagkey = SensorKit.GetHashName(name);
                if (Settings.Tags.ContainsKey(tagkey))
                {
                    var sensorTag = Settings.Tags[tagkey];
                    if (sensorTag != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            sensor.Tag = sensorTag;
                        });
                        // attempt to reconnect (non UI thread)
                        sensor.Connector = new SensorKitConnector(this, sensor);
                        await sensor.Connector.Subscribe();
                    }
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Data");
                });
            }catch(Exception x)
            {
                Debug.WriteLine(x);
            }
            isAddingSensor = false;
        }
        public static string GetHashName(string name)
        {
            if (name != null)
                return $"SensorKit{Convert.ToBase64String(Encoding.UTF8.GetBytes(name)).Replace("=", "")}";
            else
                return null;
        }

        


    }


}
