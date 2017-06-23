//using Microsoft.ApplicationInsights;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.UI;
using Windows.UI.Core;

namespace SensorKit
{
    public class SensorModel : ViewModel
    {

        public SensorKitConnector Connector { get; set; }
        public SensorInformation Information { get; set; }

        int BUFFER_SIZE = 50;

        string currentFilePathCSV; // path/exp_20170101120000_1_2_USERID_cm.csv
        string currentFileName; // exp_20170101120000_1_2_USERID_cm
        
        public ulong BluetoothAddress { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }

        

        string _tag;

        public string Tag {

            get {
                return _tag;
            }

            set {
                var tagkey = SensorKit.GetHashName(Name);
                if (SensorKit.Settings.Tags.ContainsKey(tagkey))
                {
                    SensorKit.Settings.Tags[tagkey] = value;
                }
                else
                {
                    SensorKit.Settings.Tags.Add(tagkey, value);
                }
                SetValue(ref _tag, value, "Tag");
            }
        }

        public HumanKinematics HumanKinematics { get; set; } = new HumanKinematics();


        public string SensorType
        {
            get
            {
                if (Name != null)
                {
                    if (Name.Contains("illumiSki"))
                        return "illumiSki";
                    else if (Name.Contains("illumiSens"))
                        return "illumiSens";
                    else if (Name.Contains("illumiBand"))
                        return "illumiBand";
                    else
                        return "SensorKit";
                }
                else
                {
                    return "SensorKit";
                }
            }
        }

        

        public string Icon
        {
            get
            {
                return $"/Assets/sensorkit/{SensorType}.png";
            }
        }

        public bool IsSubscribed
        {
            get
            {
                if (Id == "local")
                    return true;
                if (Connector != null)
                    return Connector.IsConnected;
                else
                    return false;
            }
        }

        public Color ActualColor
        {
            get
            {
                if(Connector != null)
                {
                    return Connector.ActualColor;
                }
                return Color.FromArgb(0,0,0,0);
            }
            set
            {
                if(Connector != null)
                {
                    Connector.ActualColor = value;
                }
            }

        }

        public bool IsColorWriteEnabled
        {
            get
            {
                if (Connector != null)
                {
                    return Connector.IsColorWriteEnabled;
                }
                else
                    return false;
            }
        }

        public bool IsMute
        {
            get
            {
                if (Connector != null)
                {
                    return Connector.IsMute;
                }
                return false;
            }
            set
            {
                if (Connector != null)
                {
                    Connector.IsMute = value;
                }
            }

        }

        private List<string> buffer { get; set; } = new List<string>();
        SensorKit Controller;

        public SensorModel(SensorKit controller, string name)
        {
            Controller = controller;
            Name = name;
            // initialize the tag
            if(SensorKit.Settings.Tags != null && SensorKit.Settings.Tags.ContainsKey(SensorKit.GetHashName(Name)))
                _tag = SensorKit.Settings.Tags[SensorKit.GetHashName(Name)];
            OnPropertyChanged("Tag");
        }

        bool isAppending = false;
        bool isStarting = false;
        bool isStarted = false;

        public void Start()
        {
            try
            {
                Debug.WriteLine($"***STARTING MODEL... ID={Id} TAG={Tag}");
                if (!isStarting && !isStarted)
                {
                    isStarting = true;
                    currentFileName = $"exp_{DateTime.Now:yyyyMMddhhmmss}_{SensorKit.Settings.Experiment}_{Controller.FusionData.activityTypeId}_{UserId}_{Name.ToPascalCase()}_{Tag.ToPascalCase()}";
                    currentFilePathCSV = $"{FileSystem.Current.LocalStorage.Path}/{SensorKit.folderPrefix}_{DateTime.Now:yyyyMMdd}/{currentFileName}.csv";
                    isStarted = true;
                }
                isStarting = false;
            }
            catch { }
        }

        long totalCount = 0;

        public void Append(SensorItem e)
        {
            try
            {
                if (!isAppending)
                {
                    isAppending = true;
                    var message = $"{e.timestamp:yyyy-MM-dd HH:mm:ss.fffffffzzz},{e.aX:F8},{e.aY:F8},{e.aZ:F8},{e.avX:F8},{e.avY:F8},{e.avZ:F8},{e.qW:F8},{e.qX:F8},{e.qY:F8},{e.qZ:F8},{e.lat:F8},{e.lon:F8},{e.speed:F8},{e.alt:F8},{e.incl:F8}"; //,{CommonState.LocationData.LastAltitude},{CommonState.Account.LapSummary.ActivityMetrics.Slope},{CommonState.LocationData.LastPosition.Latitude},{CommonState.LocationData.LastPosition.Longitude}";
                    buffer.Add(message);
                    totalCount++;
                    if (buffer.Count > BUFFER_SIZE)
                    {
                        Debug.WriteLine($"***SAVING... ID={Id} TAG={Tag} {totalCount}");
                        Save();
                        buffer.Clear();
                    }
                    if(Id != SensorKit.HUB_SENSOR_ID)
                    Debug.WriteLine($"***APPEND MODEL... ID={Id} TAG={Tag} {message}");
                }
            }
            catch(Exception x) {
                Debug.WriteLine(x);
            }
            isAppending = false;
        }

        public void Save()
        {
            File.AppendAllLines(currentFilePathCSV, buffer);
        }

        public void Forget()
        {
            Stop(); // stop data
                           // unsubscribe from the bluetooth
            if (Connector != null)
            {
                Connector.Unsubscribe();
                Connector = null;
            }
            // untag the sensor
            Tag = null;
            NotifyPropertyChanged("IsConnected");
        }

        public async Task Subscribe()
        {
            Connector = new SensorKitConnector(Controller, this);
            await Connector.Subscribe();
        }

        public void Stop()
        {
            try
            {
                Save();
                buffer.Clear();
                isStarted = false;
                Task.Run(async () => {
                    await SaveAsync();
                });
            }
            catch(Exception x) {
                Debug.WriteLine(x);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var zipPath = currentFilePathCSV + ".zip"; // csv.zip
                Debug.WriteLine($"***SAVING ZIP... ID={Id} TAG={Tag} {zipPath}");
                using (var fo = File.OpenRead(currentFilePathCSV)) // .csv
                using (var fs = new FileStream(zipPath, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
                    {
                        var entry = archive.CreateEntry(currentFileName + ".csv");
                        using (var es = entry.Open())
                        {
                            await fo.CopyToAsync(es);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        

    }
}




