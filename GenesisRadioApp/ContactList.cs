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
    public class Contact
    {
        public string Caption { get; private set; }

        public Contact(string caption)
        {
            Caption = caption;
        }
    }

    public class ContactList
    {
        static Contact[] contacts =
        {
            new Contact("Yoav Ramzor"),
            new Contact("Bibi")
        };

        private Contact[] mContacts;

        public ContactList()
        {
            mContacts = contacts;
        }

        public Contact this[int i]
        {
            get { return mContacts[i]; }
        }

        public int Length
        {
            get { return mContacts.Length; }
        }
    }

  
}