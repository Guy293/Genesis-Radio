using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenesisRadioApp
{
    public class LoraBLSerivceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string TAG = typeof(LoraBLSerivceConnection).FullName;

        public LoraBLServiceBinder Binder { get; private set; }
        public LoraBLService Service { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder serviceBinder)
        {
            Binder = serviceBinder as LoraBLServiceBinder;
            Service = Binder.GetBackgroundService();

            Log.Debug(TAG, $"OnServiceConnected {name.ClassName}");
            //string message = "onServiceConnected - ";

            //if (IsConnected)
            //{
            //    message = message + " bound to service " + name.ClassName;
            //    mainActivity.UpdateUiForBoundService();
            //}
            //else
            //{
            //    message = message + " not bound to service " + name.ClassName;
            //    mainActivity.UpdateUiForUnboundService();
            //}

            //Log.Info(TAG, message);
            //mainActivity.timestampMessageTextView.Text = message;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
            Binder = null;
            Service = null;
            //mainActivity.UpdateUiForUnboundService();
        }

        //public string GetFormattedTimestamp()
        //{
        //    if (!IsConnected)
        //    {
        //        return null;
        //    }

        //    return Binder?.GetFormattedTimestamp();
        //}
    }
}