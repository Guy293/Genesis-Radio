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
    public class MessageContent
    {
        public string Message { get; }
        public string Time { get; }
        public bool IsSelfSent { get; }
        public MessageContent() { }
        public MessageContent(string Message, bool IsSelfSent)
        {
            this.Message = Message;
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.IsSelfSent = IsSelfSent;
        }
    }
}