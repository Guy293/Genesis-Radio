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
using Android.Bluetooth;
using Java.Util;
using System.IO;
using Xamarin.Essentials;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using System.Threading;

namespace GenesisRadioApp
{
    public class Packet
    {
        public String message;
        public int rssi;
    }

    class LoraBLService
    {
        private const int REQUEST_ENABLE_BT = 2;

        BluetoothAdapter btAdapter;
        BluetoothDevice btDevice;
        BluetoothSocket btSocket;

        MainActivity activity;

        public List<MessageContent> messageList { get; }

        /*
        public LoraBLService()
        {
            //_socket.Connect();

            //if (_socket.IsConnected)
            //{
            //    Console.WriteLine("Conencted!");
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
            //            Console.WriteLine(packet.message + " | RSSI: " + packet.rssi);
            //        }
            //    }
            //}


            // _socket.InputStream.ReadAsync(buffer, 0, buffer.Length).Wait();


            /*
            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "ESP32-Lora"
                                      select bd).FirstOrDefault();

            Console.WriteLine(device.Name);
            
        }
        */

        public LoraBLService(MainActivity activity)
        {
            this.activity = activity;

            this.btAdapter = BluetoothAdapter.DefaultAdapter;

            if (this.btAdapter == null)
            {
                // Bluetooth isn't supported
                return;
            }

            // Check for bluetooth permissions
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                activity.RequestPermissions(new string[] { Manifest.Permission.BluetoothConnect }, REQUEST_ENABLE_BT);
            }
            else
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }

            // Check if bluetooth is on
            if (!this.btAdapter.IsEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }


            // TODO: Get the nearest one (highest RSSI)
            ConnectToModule();
        }

        public void OnActivityResuls(int requestCode, int resultCode, Intent data)
        {
            Console.WriteLine(resultCode);
        }

        public void ConnectToModule()
        {
            foreach (BluetoothDevice d in this.btAdapter.BondedDevices)
            {
                if (d.Name == "ESP32-Lora")
                {
                    Console.WriteLine("ESP32-Lora Found!");
                    this.btDevice = d;
                }
            }

            if (this.btDevice == null)
            {
                throw new ModuleNotFoundException();
            }

            // TODO: The fuck is this string
            btSocket = btDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

            btSocket.Connect();
            Console.WriteLine("ESP32-Lora Connected!");

            Thread t = new Thread(new ThreadStart(Run));
            t.IsBackground = true;
            t.Start();
        }

        public void SendMessage(string message)
        {
            Stream outStream = btSocket.OutputStream;

            outStream.Write(Encoding.UTF8.GetBytes(message));
        }

        private void Run()
        {
            if (btSocket.IsConnected)
            {
                Stream inStream = btSocket.InputStream;

                Java.IO.InputStreamReader inReader = new Java.IO.InputStreamReader(inStream);
                Java.IO.BufferedReader buffer = new Java.IO.BufferedReader(inReader);

                while (true)
                {
                    if (buffer.Ready())
                    {
                        //byte i = ((byte)inReader.Read());
                        //byte b = 
                        String packet = buffer.ReadLine();

                        activity.RunOnUiThread(() => {
                            activity.InsertMessage(new MessageContent(packet));
                        });

                        //Console.WriteLine(packet);
                        //Packet packet = JsonConvert.DeserializeObject<Packet>(jsonString);
                        //Console.WriteLine(packet.message + " | RSSI: " + packet.rssi);
                    }
                }
            }

            //_socket.InputStream.ReadAsync(buffer, 0, buffer.Length).Wait();
        }
    }

    class ModuleNotFoundException : Exception
    {
        public ModuleNotFoundException() { }
    }
}