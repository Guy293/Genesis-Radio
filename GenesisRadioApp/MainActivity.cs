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
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using static Xamarin.Essentials.Permissions;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace GenesisRadioApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const int BLUETOOTH_PERMISSION_REQUEST = 1;
        const int FINE_LOCATION_PERMISSION_REQUEST = 2;
        const int BACKGROUND_LOCATION_PERMISSION_REQUEST = 3;

        LoraBLSerivceConnection loraBLServiceConnection;
        ListView messageListView;
        public List<Message> messageList = new List<Message>();
        public MessageViewAdapter messageListAdapter;

        NewMessageBroadcastReceiver newMessageBroadcastReceiver;

        public Database database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            database = new Database();

            messageListView = FindViewById<ListView>(Resource.Id.messages_list);

            messageList = database.GetMessages();

            messageListAdapter = new MessageViewAdapter(this, messageList);
            messageListView.Adapter = messageListAdapter;


            // Asks all the permissions needed, good enough for now
            // TODO: Make sure the user actually accpets the permissions
            // https://stackoverflow.com/questions/46070814/how-to-nag-ask-user-to-enable-permission-until-user-gives-it/46071096
            if (CheckSelfPermission(Manifest.Permission.BluetoothScan) == Permission.Denied ||
                CheckSelfPermission(Manifest.Permission.BluetoothConnect) == Permission.Denied ||
                CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Denied ||
                CheckSelfPermission(Manifest.Permission.AccessBackgroundLocation) == Permission.Denied)
            {
                RequestPermissions(new string[] {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect
                }, BLUETOOTH_PERMISSION_REQUEST);
            }


            this.loraBLServiceConnection = new LoraBLSerivceConnection();

            Intent serviceIntent = new Intent(this, typeof(LoraBLService));
            BindService(serviceIntent, this.loraBLServiceConnection, Bind.Important);
            StartForegroundService(serviceIntent);


            EditText input = FindViewById<EditText>(Resource.Id.input);

            input.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    InsertMessage(new Message(input.Text, true));
                    this.loraBLServiceConnection.Service.SendMessage(input.Text);
                    input.Text = "";
                    e.Handled = true;
                }
            };


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

        public void InsertMessage(Message message) {
        
            database.SaveMessage(message);

            UpdateMessageList();
        }

        public void UpdateMessageList()
        {
            List<Message> newMessageList = database.GetMessages();

            foreach (Message message in newMessageList)
            {
                if (!messageList.Contains(message))
                {
                    messageList.Add(message);
                }
            }

            messageListAdapter.NotifyDataSetChanged();
        }

        //protected override void OnResume()
        //{
        //    base.OnResume();

        //    this.newMessageBroadcastReceiver = new NewMessageBroadcastReceiver(this);

        //    LocalBroadcastManager.GetInstance(this)
        //        .RegisterReceiver(this.newMessageBroadcastReceiver, new IntentFilter("new-message"));
        //}
        //protected override void OnPause()
        //{
        //    LocalBroadcastManager.GetInstance(this).UnregisterReceiver(this.newMessageBroadcastReceiver);
        //    base.OnPause();
        //}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == BLUETOOTH_PERMISSION_REQUEST)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation }, FINE_LOCATION_PERMISSION_REQUEST);
            }

            if (requestCode == FINE_LOCATION_PERMISSION_REQUEST)
            {
                new Android.App.AlertDialog.Builder(this)
                    .SetTitle("Background Location Permission Needed")
                    .SetMessage("Please Set Location Permission To Allow All The Time")
                    .SetPositiveButton(
                        "OK",
                        (senderAlert, args) =>
                        {
                            RequestPermissions(new string[] { Manifest.Permission.AccessBackgroundLocation }, BACKGROUND_LOCATION_PERMISSION_REQUEST);
                        })
                    .Create()
                    .Show();
            }
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

            //string message = intent.GetStringExtra("message");

            //this.mainActivity.InsertMessage(new MessageContent(message, false));

            mainActivity.UpdateMessageList();
        }
    }
}
