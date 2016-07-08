﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CrewOnCallUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        //string appointmentID;
        string clientName;
        string venueName;
        string clientNotes;
        string startDate;
        string startTime;
        string endTime;
        string breakLength;
        string totalTime;
        TimeSpan totalHours;
        public MainPage()
        {
            InitializeComponent();
            Initialize();

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public async void Initialize()
        {
            var client = new Client();
            var venue = new Venue();
            var start = new Start();

            if (RetrieveSettings("onTheWay") != "yes")
            {
                AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

                FindAppointmentsOptions options = new FindAppointmentsOptions();
                options.MaxCount = 100;
                options.FetchProperties.Add(AppointmentProperties.Subject);
                options.FetchProperties.Add(AppointmentProperties.Location);
                options.FetchProperties.Add(AppointmentProperties.AllDay);
                options.FetchProperties.Add(AppointmentProperties.StartTime);
                options.FetchProperties.Add(AppointmentProperties.Duration);
                IReadOnlyList<Appointment> appointments = await store.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromHours(2), options);

                if (appointments.Count > 0)
                {
                    var i = 0;

                    while ((appointments[i].AllDay) && (i < appointments.Count))
                        i++;
                    if (!appointments[i].AllDay)
                    {
                        SaveSettings("clientName", appointments[i].Subject);
                        SaveSettings("venueName", appointments[i].Location);
                        SaveSettings("startTime", appointments[i].StartTime.ToString());
                        //SaveSettings("localID", appointments[i].LocalId);
                    }
                    else
                    {
                        SaveSettings("clientName", "Client");
                        SaveSettings("venueName", "Venue");
                        SaveSettings("startTime", DateTime.Now.ToString());
                        //SaveSettings("localID", null);
                    }
                }
                else
                {
                    SaveSettings("clientName", "Client");
                    SaveSettings("venueName", "Venue");
                    SaveSettings("startTime", DateTime.Now.ToString());
                    //SaveSettings("localID", null);
                }
            }

            client.ClientName = RetrieveSettings("clientName");
            clientNameTextBox.DataContext = client;

            venue.VenueName = RetrieveSettings("venueName");
            venueNameTextBox.DataContext = venue;

            ////start.StartTime = DateTime.Parse(RetrieveSettings("startTime"));
            //start.StartTime = DateTime.Now.AddDays(7);
            //var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            //startDatePicker.DataContext = new DateTimeOffset(start.StartTime, timeZoneOffset);

            //var dt = start.StartTime;
            //var ts = dt - dt.Date;
            //startTimePicker.DataContext = ts;
            //start.StartTime = DateTime.Now.AddHours(6);
            //startTimePicker.DataContext = start.StartTime - start.StartTime.Date;

        }

        public class Client
        {
            private string _name;

            public Client()
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if ((localSettings.Values["clientName"] == null) || !localSettings.Values.ContainsKey("clientName"))
                    localSettings.Values["clientName"] = "Client";

                _name = RetrieveSettings("clientName");
            }

            public string ClientName
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }
        }

        public class Venue
        {
            private string _venue;

            public Venue()
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if ((localSettings.Values["venueName"] == null) || !localSettings.Values.ContainsKey("venueName"))
                    localSettings.Values["venueName"] = "Venue";

                _venue = RetrieveSettings("venueName");
            }

            public string VenueName
            {
                get
                { return _venue; }
                set
                { _venue = value; }
            }
        }

        public class Start
        {

            private DateTime _start;

            public Start()
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if ((localSettings.Values["startTime"] == null) || !localSettings.Values.ContainsKey("startTime"))
                    localSettings.Values["startTime"] = DateTime.Now.ToString();

                _start = DateTime.Parse(RetrieveSettings("startTime"));
            }

            public DateTime StartTime
            {
                get { return _start; }
                set { _start = value; }
            }

        }

        private async void addCalButton_Click(object sender, RoutedEventArgs e)
        {
            clientName = clientNameTextBox.Text;
            venueName = venueNameTextBox.Text;
            clientNotes = clientNotesTextBox.Text;
            startDate = startDatePicker.Date.ToString("D");
            startTime = startTimePicker.Time.ToString(@"hh\:mm");
            totalHours = endTimePicker.Time - startTimePicker.Time;

            if (totalHours < TimeSpan.FromDays(0))
            {
                totalHours += TimeSpan.FromDays(1);
            }

            var cal = new Windows.ApplicationModel.Appointments.Appointment();

            var date = (DateTimeOffset)startDatePicker.Date;
            var time = (TimeSpan)startTimePicker.Time;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var calTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0, timeZoneOffset);

            cal.StartTime = calTime;
            cal.Duration = totalHours;
            cal.Location = venueName;
            cal.Subject = clientName;
            cal.Details = "CrewOnCall::" + ((ComboBoxItem)skillPicker.SelectedItem).Content.ToString() + "\n" + clientNotes;
            cal.AllDay = false;
            cal.Reminder = TimeSpan.FromHours(2);

            String ID = await Windows.ApplicationModel.Appointments.AppointmentManager.ShowAddAppointmentAsync(cal, new Rect(), Windows.UI.Popups.Placement.Default);
        }

        private async void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            clientName = clientNameTextBox.Text;
            venueName = venueNameTextBox.Text;
            startDate = startDatePicker.Date.ToString("D");
            startTime = startTimePicker.Time.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "Confirming " + clientName + " at " + venueName + " on " + startDate + " at " + startTime + "\nGeorge";
            sms.Recipients.Add("+61490139009");
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }


        private async void ontheway_Click(object sender, RoutedEventArgs e)
        {
            clientName = clientNameTextBox.Text;

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "I am on the way to " + clientName + "\nGeorge";
            sms.Recipients.Add("+61490139009");
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);

            SaveSettings("clientName", clientName);
            SaveSettings("onTheWay", "yes");
        }

        private async void sendtotalHours_Click(object sender, RoutedEventArgs e)
        {
            clientName = clientNameTextBox.Text;

            if (VerifyTimeIsAvailable(startTimePicker.Time) == true)
            {
                startTime = startTimePicker.Time.ToString(@"hh\:mm");
            }
            else
            {
                startTime = DateTime.Now.Subtract(TimeSpan.FromHours(3)).ToString(@"hh\:mm");
            }

            if (VerifyTimeIsAvailable(endTimePicker.Time) == true)
            {
                endTime = endTimePicker.Time.ToString(@"hh\:mm");
            }
            else
            {
                endTime = DateTime.Now.ToString(@"hh\:mm");
            }

            totalHours = endTimePicker.Time - startTimePicker.Time;

            if (totalHours < TimeSpan.FromDays(0))
            {
                totalHours += TimeSpan.FromDays(1);
            }

            breakLength = ((ComboBoxItem)breakLengthPicker.SelectedItem).Content.ToString();
            switch (breakLength)
            {
                case "30 min":
                    totalHours -= TimeSpan.FromMinutes(30);
                    break;
                case "45 min":
                    totalHours -= TimeSpan.FromMinutes(45);
                    break;
                case "60 min":
                    totalHours -= TimeSpan.FromMinutes(60);
                    break;
            }

            if (totalHours < TimeSpan.FromDays(0))
            {
                totalHours = TimeSpan.FromDays(0);
            }

            if ((startDatePicker.Date.DayOfWeek == 0) & (totalHours < TimeSpan.FromHours(4)))
            {
                totalHours = TimeSpan.FromHours(4);
                totalTime = "4 hr call";
            }

            else
                if (totalHours < TimeSpan.FromHours(3))
            {
                totalHours = TimeSpan.FromHours(3);
                totalTime = "3 hr call";
            }

            else totalTime = totalHours.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "Hours for " + clientName + " = " + totalTime + ".\n(" + startTime + " - " + endTime + ")\n" + breakLength + " break.\nGeorge";
            sms.Recipients.Add("+61490139009");
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);

            SaveSettings("clientName", "Client");
            SaveSettings("venueName", "Venue");
            SaveSettings("onTheWay", "no");
        }

        private void clientName_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void venueName_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void clientNotes_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void startDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        { }

        private void skillPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { }

        private void startTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        { }

        private void breakLength_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { }

        private void endTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        { }

        public bool SaveSettings(string key, string value)
        {
            try
            {
                //var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[key] = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string RetrieveSettings(string key)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if (localSettings.Values[key] != null)
                    return localSettings.Values[key].ToString();
                return key;
            }
            catch
            {
                return key;
            }
        }

        private bool VerifyTimeIsAvailable(TimeSpan timeSpan)
        {
            //throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
