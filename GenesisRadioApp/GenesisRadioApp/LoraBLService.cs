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
using System.Linq;
using Java.Util;
using System.IO;
using Newtonsoft.Json;


namespace GenesisRadioApp
{
    public class Packet
    {
        public String message;
        public int rssi;
    }

    class LoraBLService
    {
        BluetoothAdapter btAdapter;
        BluetoothDevice btDevice;
        BluetoothSocket btSocket;

        public LoraBLService()
        {
            _socket.Connect();

            if (_socket.IsConnected)
            {
                Console.WriteLine("Conencted!");
                Stream inStream = _socket.InputStream;

                Java.IO.InputStreamReader inReader = new Java.IO.InputStreamReader(inStream);
                Java.IO.BufferedReader buffer = new Java.IO.BufferedReader(inReader);

                while (true)
                {
                    if (buffer.Ready())
                    {
                        //byte i = ((byte)inReader.Read());
                        //byte b = 
                        String jsonString = buffer.ReadLine();
                        Packet packet = JsonConvert.DeserializeObject<Packet>(jsonString);
                        Console.WriteLine(packet.message + " | RSSI: " + packet.rssi);
                    }
                }
            }


            // _socket.InputStream.ReadAsync(buffer, 0, buffer.Length).Wait();


            /*
            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "ESP32-Lora"
                                      select bd).FirstOrDefault();

            Console.WriteLine(device.Name);
            */
        }

        public void Start()
        {
            foreach (BluetoothDevice d in this.btAdapter.BondedDevices)
            {
                if (d.Name == "ESP32-Lora")
                {
                    this.btDevice = d;
                }
            }

            if (this.btDevice == null)
            {
                throw new ModuleNotFoundException();
            }

            this.btSocket = btDevice.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

            btSocket.Connect();

        }
    }

    class ModuleNotFoundException : Exception
    {
        public ModuleNotFoundException() { }
    }
}