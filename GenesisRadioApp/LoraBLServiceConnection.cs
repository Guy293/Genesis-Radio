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

        MainActivity mainActivity;
        public LoraBLSerivceConnection(MainActivity activity)
        {
            IsConnected = false;
            Binder = null;
            mainActivity = activity;
        }

        public bool IsConnected { get; private set; }
        public LoraBLServiceBinder Binder { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as LoraBLServiceBinder;
            IsConnected = this.Binder != null;

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
            IsConnected = false;
            Binder = null;
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