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
using Android.Support.V4.Content;
using System.Collections.ObjectModel;

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
        static readonly string TAG = typeof(LoraBLService).FullName;

        readonly string CHANNEL_ID = "status_notification_channel";
        public readonly int NOTIFICATION_ID = 100;

        bool isRunning = false;

        public NotificationManager notificationManager;
        public Notification.Builder notificationBuilder;

        IBinder binder;
        readonly string bluetoothServiceUUID = "16f88c52-1471-4bba-95a8-17094b0520d3";
        readonly string newMessageCharacteristicUUID = "af77d21b-1a5c-4910-b4b4-c98220ac0e79";
        readonly string sendMessageCharacteristicUUID = "8ef6e254-8921-4ef2-9726-368055789ba4";
        BluetoothGattCharacteristic newMessageCharacteristic;
        BluetoothGattCharacteristic sendMessageCharacteristic;
        public List<(BluetoothDevice Device, int Rssi)> Devices;
        BluetoothManager bluetoothManager;
        BluetoothAdapter bluetoothAdapter;
        BluetoothLeScanner bleScanner;
        public BluetoothDevice device;
        public BluetoothGatt bluetoothGatt;

        public Database database;

        public Intent intent;


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (isRunning)
            {
                return StartCommandResult.Sticky;
            }

            this.intent = intent;

            var channel = new NotificationChannel(CHANNEL_ID, "Status Notification", NotificationImportance.Max);

            channel.SetShowBadge(false);

            notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            var pendingIntent = PendingIntent.GetActivity(this, NOTIFICATION_ID, intent, PendingIntentFlags.Immutable);
            notificationBuilder = new Notification.Builder(this, CHANNEL_ID)
            .SetContentTitle(Resources.GetString(Resource.String.app_name))
            .SetContentText(Resources.GetString(Resource.String.notification_looking_for_beacon))
            .SetContentIntent(pendingIntent)
            .SetSmallIcon(Resource.Mipmap.ic_launcher)
            .SetOngoing(true);

            StartForeground(NOTIFICATION_ID, notificationBuilder.Build());

            database = new Database();

            Devices = new List<(BluetoothDevice Device, int Rssi)>();

            bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(BluetoothService);
            bluetoothAdapter = bluetoothManager.Adapter;

            if (bluetoothAdapter.BluetoothLeScanner == null)
            {
                bluetoothAdapter.Enable();
            }

            bleScanner = bluetoothAdapter.BluetoothLeScanner;

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Work();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TAG, ex.ToString());
                    }

                    Thread.Sleep(3000);
                }
            });

            isRunning = true;

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
                if (CheckSelfPermission(Manifest.Permission.BluetoothScan) == Permission.Denied ||
                    CheckSelfPermission(Manifest.Permission.BluetoothConnect) == Permission.Denied ||
                    CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Denied ||
                    CheckSelfPermission(Manifest.Permission.AccessBackgroundLocation) == Permission.Denied)
                {
                    return;
                }

                Devices.Clear();

                // TODO: Test which scan mode to use
                ScanSettings scanSettings = new ScanSettings.Builder()
                    //.SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower)
                    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                    .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                    .Build();

                ScanFilter scanFilter = new ScanFilter.Builder()
                    .SetServiceUuid(ParcelUuid.FromString(bluetoothServiceUUID))
                    .Build();

                List<ScanFilter> scanFilters = new List<ScanFilter>();
                scanFilters.Add(scanFilter);

                LeScanCallback callback = new LeScanCallback(this);

                bleScanner.StartScan(scanFilters, scanSettings, callback);
                Thread.Sleep(3000);
                bleScanner.StopScan(callback);

                if (Devices.Count == 0)
                {
                    Log.Debug(TAG, "No devices found");
                    return;
                }

                Devices.Sort((x, y) => y.Rssi.CompareTo(x.Rssi));

                BluetoothDevice nearestDevice = Devices[0].Device;
                int nearestDeviceRssi = Devices[0].Rssi;

                Log.Debug(TAG, $"Trying to connect to {nearestDevice.Name} ({nearestDeviceRssi} dBm)");

                nearestDevice.ConnectGatt(this, false, new LeGattCallback(this));

                while (bluetoothGatt == null) { }

                bluetoothGatt.DiscoverServices();


                while (bluetoothGatt.Services.Count == 0) { }

                BluetoothGattService bluetoothGattService = bluetoothGatt.GetService(UUID.FromString(bluetoothServiceUUID));

                newMessageCharacteristic = bluetoothGattService.GetCharacteristic(UUID.FromString(newMessageCharacteristicUUID));
                bluetoothGatt.SetCharacteristicNotification(newMessageCharacteristic, true);

                sendMessageCharacteristic = bluetoothGattService.GetCharacteristic(UUID.FromString(sendMessageCharacteristicUUID));
            }
        }

        public void SendMessage(string message)
        {
            sendMessageCharacteristic.SetValue(message);
            bluetoothGatt.WriteCharacteristic(sendMessageCharacteristic);
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
        static readonly string TAG = typeof(LeScanCallback).FullName;

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
            foreach ((BluetoothDevice Device, int Rssi) in m.Devices)
                if (result.Device.Equals(Device)) return;
            
            m.Devices.Add((result.Device, result.Rssi));

            //Log.Debug(TAG, "Device found: " + result.Device.Address);
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
            Log.Error(TAG, "Scan failed. Error code: " + errorCode.ToString());
        }
    }

    public class LeGattCallback : BluetoothGattCallback
    {
        static readonly string TAG = typeof(LeGattCallback).FullName;

        LoraBLService m;
        public LeGattCallback(LoraBLService m)
        {
            this.m = m;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            if (newState == ProfileState.Connected)
            {
                m.bluetoothGatt = gatt;
                m.device = gatt.Device;

                m.notificationBuilder.SetContentText(m.ApplicationContext.Resources.GetString(Resource.String.notification_connected_to_beacon));
                m.notificationManager.Notify(m.NOTIFICATION_ID, m.notificationBuilder.Build());

                Log.Debug(TAG, "Device connected");
            }
            else if (newState == ProfileState.Disconnected)
            {
                m.device = null;

                m.notificationBuilder.SetContentText(m.ApplicationContext.Resources.GetString(Resource.String.notification_looking_for_beacon));
                m.notificationManager.Notify(m.NOTIFICATION_ID, m.notificationBuilder.Build());

                Log.Debug(TAG, "Device disconnected");
            }
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            string messageString = characteristic.GetStringValue(0);

            Message message = new Message(messageString, false);
            m.database.SaveMessage(message);


            // Send broadcast to main activity
            // TODO: Use a proper action name (com...)
            Intent intent = new Intent("new-message");
            //intent.PutExtra("message", messageString);
            LocalBroadcastManager.GetInstance(this.m.ApplicationContext).SendBroadcast(intent);

            Log.Debug(TAG, "Received notification from device");
        }
    }

    //class ModuleNotFoundException : Exception
    //{
    //    public ModuleNotFoundException() { }
    //}
}