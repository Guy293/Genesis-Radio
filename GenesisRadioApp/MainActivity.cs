using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Content;
using Android.Bluetooth;
using System.Collections.Generic;

namespace GenesisRadioApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        LoraBLService module;
        LoraBLSerivceConnection loraBLServiceConnection;
        ListView messageListView;
        List<MessageContent> messageList = new List<MessageContent>();
        MessageViewAdapter messageListAdapter;

        NewMessageBroadcastReceiver newMessageBroadcastReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            messageListView = FindViewById<ListView>(Resource.Id.messages_list);

            messageListAdapter = new MessageViewAdapter(this, messageList);
            messageListView.Adapter = messageListAdapter;


            //// Check for bluetooth permissions
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            //{
            //    RequestPermissions(new string[] {

            //        Manifest.Permission.Bluetooth,
            //        Manifest.Permission.BluetoothAdmin,
            //        Manifest.Permission.BluetoothConnect,
            //        Manifest.Permission.BluetoothScan,
            //        Manifest.Permission.AccessBackgroundLocation,
            //        Manifest.Permission.AccessFineLocation,
            //    }, REQUEST_ENABLE_BT);
            //}
            //else
            //{
            //    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            //    StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            //}

            RequestPermissions(new String[] {
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessBackgroundLocation,
            }, 1);

            this.loraBLServiceConnection = new LoraBLSerivceConnection(this);

            Intent serviceIntent = new Intent(this, typeof(LoraBLService));
            BindService(serviceIntent, this.loraBLServiceConnection, Bind.AutoCreate);
            StartForegroundService(serviceIntent);

            // module = new LoraBLService(this);


            //EditText input = FindViewById<EditText>(Resource.Id.input);

            //input.KeyPress += (object sender, View.KeyEventArgs e) =>
            //{
            //    e.Handled = false;
            //    if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            //    {
            //        module.SendMessage(input.Text);
            //        input.Text = "";
            //        e.Handled = true;
            //    }
            //};


            //ContactList mContactList = new ContactList();

            //RecyclerView mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            //ContactListAdapter mAdapter = new ContactListAdapter(mContactList);
            //mRecyclerView.SetAdapter(mAdapter);

            //RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(this);
            //mRecyclerView.SetLayoutManager(mLayoutManager);

            this.newMessageBroadcastReceiver = new NewMessageBroadcastReceiver(this);

            LocalBroadcastManager.GetInstance(this)
                .RegisterReceiver(this.newMessageBroadcastReceiver, new IntentFilter("new-message"));
        }



        protected override void OnResume()
        {
            base.OnResume();

            this.newMessageBroadcastReceiver = new NewMessageBroadcastReceiver(this);

            LocalBroadcastManager.GetInstance(this)
                .RegisterReceiver(this.newMessageBroadcastReceiver, new IntentFilter("new-message"));
        }
        protected override void OnPause()
        {
            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(this.newMessageBroadcastReceiver);
            base.OnPause();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	    

        public void InsertMessage(MessageContent message)
        {
            messageList.Add(message);

            messageListAdapter.NotifyDataSetChanged();

        }

        //public class ContactListAdapter : RecyclerView.Adapter
        //{
        //    public ContactList mContactList;

        //    public ContactListAdapter(ContactList contactList)
        //    {
        //        mContactList = contactList;
        //    }

        //    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        //    {
        //        View itemView = LayoutInflater.From(parent.Context).
        //                    Inflate(Resource.Layout.ContactViewHolder, parent, false);
        //        ContactViewHolder vh = new ContactViewHolder(itemView);
        //        return vh;
        //    }

        //    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        //    {
        //        ContactViewHolder vh = holder as ContactViewHolder;
        //        vh.Caption.Text = mContactList[position].Caption;
        //    }

        //    public override int ItemCount
        //    {
        //        get { return mContactList.Length; }
        //    }
        //}

        //public class ContactViewHolder : RecyclerView.ViewHolder
        //{
        //    public TextView Caption { get; private set; }

        //    public ContactViewHolder(View itemView) : base(itemView)
        //    {
        //        // Locate and cache view references:
        //        Caption = itemView.FindViewById<TextView>(Resource.Id.textView);
        //    }
        //}
    }

    class NewMessageBroadcastReceiver : BroadcastReceiver
    {
        MainActivity mainActivity;

        public NewMessageBroadcastReceiver(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string message = intent.GetStringExtra("message");

            this.mainActivity.InsertMessage(new MessageContent(message, false));
        }
    }
}
