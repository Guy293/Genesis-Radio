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
using Java.Lang;

namespace GenesisRadioApp
{
    internal class MessageViewAdapter : BaseAdapter
    {
        private Activity activity;
        private List<MessageContent> messageList;

        public MessageViewAdapter(Activity activity, List<MessageContent> lstMessage)
        {
            this.activity = activity;
            this.messageList = lstMessage;
        }

        public override int Count
        {
            get { return messageList.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)activity.BaseContext.GetSystemService(Context.LayoutInflaterService);
            View ItemView = inflater.Inflate(Resource.Layout.message_layout, null);
            TextView message_content, message_time;
            message_content = ItemView.FindViewById<TextView>(Resource.Id.message_text);
            message_time = ItemView.FindViewById<TextView>(Resource.Id.message_time);

            message_content.Text = messageList[position].Message;
            message_time.Text = messageList[position].Time;

            return ItemView;
        }
    }
}