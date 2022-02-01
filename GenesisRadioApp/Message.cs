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
using SQLite;

namespace GenesisRadioApp
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
        public bool IsSelfSent { get; set; }
        public Message() { }
        public Message(string Content, bool IsSelfSent)
        {
            this.Content = Content;
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.IsSelfSent = IsSelfSent;
        }

        public override bool Equals(object obj)
            => obj is Message other && Id == other.Id;
    }
}