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

namespace GenesisRadioApp
{
    public class LoraBLServiceBinder : Binder
    {
        public LoraBLService Service { get; private set; }

        public LoraBLServiceBinder(LoraBLService service)
        {
            this.Service = service;
        }

        public LoraBLService GetBackgroundService()
        {
            return this.Service;
        }
    }
}