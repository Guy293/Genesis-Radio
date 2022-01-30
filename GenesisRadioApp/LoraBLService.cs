using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java.Util;
using System.IO;
using Xamarin.Essentials;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using System.Threading;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Util;

namespace GenesisRadioApp
{
    public class Packet
    {
        public String message;
        public int rssi;
    }

    [Service]
    public class LoraBLService : Service
    {
        internal static readonly string CHANNEL_ID = "status_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        NotificationManager notificationManager;
        Notification.Builder notificationBuilder;

        IBinder binder;
        public List<(BluetoothDevice device, int rssi)> Devices;
        BluetoothManager bluetoothManager;
        BluetoothAdapter bluetoothAdapter;
        BluetoothLeScanner bleScanner;
        BluetoothDevice device;

        public Intent intent;


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            this.intent = intent;

            var channel = new NotificationChannel(CHANNEL_ID, "Status Notification", NotificationImportance.Max);

            channel.SetShowBadge(false);

            notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            var pendingIntent = PendingIntent.GetActivity(this, NOTIFICATION_ID, intent, PendingIntentFlags.Immutable);
            notificationBuilder = new Notification.Builder(this, CHANNEL_ID)
            .SetContentTitle("Genesis Radio")
            .SetContentText("Looking for beacon stations...")
            .SetContentIntent(pendingIntent)
            .SetSmallIcon(Resource.Mipmap.ic_launcher)
            .SetOngoing(true);

            StartForeground(NOTIFICATION_ID, notificationBuilder.Build());


            //notificationBuilder.SetContentText("Found!");
            //notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());

            Devices = new List<(BluetoothDevice device, int rssi)>();


            bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(BluetoothService);
            bluetoothAdapter = bluetoothManager.Adapter;
            bleScanner = bluetoothAdapter.BluetoothLeScanner;

            Task.Run(() =>
            {
                while (true)
                {
                    Work();
                    Thread.Sleep(10000);
                }
            });

            //if (Devices.Count != 0)
            //{
            //    foreach (BluetoothDevice d in Devices) Log.Debug("BLE", "-----> " + d.Address);
            //}

            //Devices = new List<BluetoothDevice>();
            //if (Manager.Adapter.IsDiscovering)
            //{
            //    Log.Debug("BLE", "ALREADY SCANNING");
            //}
            //else
            //{
            //    Log.Debug("BLE", "START SCANNING");
            //    Manager.Adapter.BluetoothLeScanner.StartScan(new LeScanCallback(this));
            //}


            // TODO: Sticky or not? idk
            // return StartCommandResult.NotSticky;
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new LoraBLServiceBinder(this);
            return binder;
        }

        private void Work()
        {
            if (this.device == null)
            {
                Devices.Clear();

                // TODO: Test which scan mode to use
                ScanSettings scanSettings = new ScanSettings.Builder()
                    //.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower)
                    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                    .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                    .Build();

                ScanFilter scanFilter = new ScanFilter.Builder()
                    .SetServiceUuid(ParcelUuid.FromString("16f88c52-1471-4bba-95a8-17094b0520d3"))
                    .Build();

                List<ScanFilter> scanFilters = new List<ScanFilter>();
                scanFilters.Add(scanFilter);

                LeScanCallback callback = new LeScanCallback(this);

                bleScanner.StartScan(scanFilters, scanSettings, callback);
                Thread.Sleep(5000);
                bleScanner.StopScan(callback);

                // TODO: Test that this sort works correctly
                Devices.Sort((x, y) => y.rssi.CompareTo(x.rssi));

                (BluetoothDevice, int) nearestDevice = Devices[0];

                this.device = nearestDevice.Item1;

                Log.Debug("BLE", $"{nearestDevice.Item1.Name} ({nearestDevice.Item2})");

                notificationBuilder.SetContentText("Connected to a beacon");
                notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
            }
        }


        /*
        public LoraBLService()
        {
            //_socket.Connect();

            //if (_socket.IsConnected)
            //{
            //    Log.Debug("Conencted!");
            //    Stream inStream = _socket.InputStream;

            //    Java.IO.InputStreamReader inReader = new Java.IO.InputStreamReader(inStream);
            //    Java.IO.BufferedReader buffer = new Java.IO.BufferedReader(inReader);

            //    while (true)
            //    {
            //        if (buffer.Ready())
            //        {
            //            //byte i = ((byte)inReader.Read());
            //            //byte b = 
            //            String jsonString = buffer.ReadLine();
            //            Packet packet = JsonConvert.DeserializeObject<Packet>(jsonString);
            //            Log.Debug(packet.message + " | RSSI: " + packet.rssi);
            //        }
            //    }
            //}


            // _socket.InputStream.ReadAsync(buffer, 0, buffer.Length).Wait();


            /*
            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "ESP32-Lora"
                                      select bd).FirstOrDefault();

            Log.Debug(device.Name);
            
        }
        */

        /*
        public LoraBLService(MainActivity activity)
        {
            this.activity = activity;




            //// Check for bluetooth permissions
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            //{
            //    activity.RequestPermissions(new string[] { Manifest.Permission.BluetoothConnect }, REQUEST_ENABLE_BT);
            //}
            //else
            //{
            //    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            //    activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            //}

            //// Check if bluetooth is on
            //if (!this.btAdapter.IsEnabled)
            //{
            //    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            //    activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            //}


            // TODO: Get the nearest one (highest RSSI)
            // ConnectToModule();
        }
        */

        //public void ConnectToModule()
        //{
        //    foreach (BluetoothDevice d in this.btAdapter.BondedDevices)
        //    {
        //        if (d.Name.StartsWith("ESP32-Lora"))
        //        {
        //            Log.Debug("ESP32-Lora Found!");
        //            this.btDevice = d;
        //        }
        //    }

        //    if (this.btDevice == null)
        //    {
        //        throw new ModuleNotFoundException();
        //    }

        //    // TODO: The fuck is this string
        //    btSocket = btDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

        //    btSocket.Connect();
        //    Log.Debug("ESP32-Lora Connected!");

        //    Thread t = new Thread(new ThreadStart(Run));
        //    t.IsBackground = true;
        //    t.Start();
        //}

        //public void SendMessage(string message)
        //{
        //    activity.InsertMessage(new MessageContent(message, true));

        //    Stream outStream = btSocket.OutputStream;

        //    outStream.Write(Encoding.UTF8.GetBytes(message));
        //}

        //private void Run()
        //{
        //    if (btSocket.IsConnected)
        //    {
        //        Stream inStream = btSocket.InputStream;

        //        Java.IO.InputStreamReader inReader = new Java.IO.InputStreamReader(inStream);
        //        Java.IO.BufferedReader buffer = new Java.IO.BufferedReader(inReader);

        //        while (true)
        //        {
        //            if (buffer.Ready())
        //            {
        //                //byte i = ((byte)inReader.Read());
        //                //byte b = 
        //                String packet = buffer.ReadLine();

        //                activity.RunOnUiThread(() => {
        //                    activity.InsertMessage(new MessageContent(packet, false));
        //                });

        //                //Log.Debug(packet);
        //                //Packet packet = JsonConvert.DeserializeObject<Packet>(jsonString);
        //                //Log.Debug(packet.message + " | RSSI: " + packet.rssi);
        //            }
        //        }
        //    }

        //    //_socket.InputStream.ReadAsync(buffer, 0, buffer.Length).Wait();
        //}
    }

    public class LeScanCallback : ScanCallback
    {
        LoraBLService m;

        public LeScanCallback(LoraBLService m)
        {
            this.m = m;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            

            // Not using .Contains() because it is derrived from List in C#
            // and doesn't use the .Equals() override by the BluetoothDevice in Java
            foreach ((BluetoothDevice device, int rssi) in m.Devices)
                if (result.Device.Equals(device)) return;
            
            m.Devices.Add((result.Device, result.Rssi));

            //Log.Debug("BLE", "Device found: " + result.Device.Address);
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
            Log.Debug("BLE", "ERROR");
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);

            Log.Debug("BLE", "BBBBBBs");
        }
    }

    //class ModuleNotFoundException : Exception
    //{
    //    public ModuleNotFoundException() { }
    //}
}