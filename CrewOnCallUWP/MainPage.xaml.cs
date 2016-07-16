using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CrewOnCallUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        Gig gig = new Gig
        {
            clientName = "New Client",
            venueName = "New Venue",
            startDate = DateTime.Now,
            startTime = DateTime.Now.TimeOfDay,
            endDate = DateTime.Now.Add(TimeSpan.FromHours(3)),
            endTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay
    };

    public MainPage()
        {
            Initialize();
            InitializeComponent();
            DataContext = gig;
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public async void Initialize()
        {
            AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            FindAppointmentsOptions options = new FindAppointmentsOptions();
            options.MaxCount = 100;
            options.FetchProperties.Add(AppointmentProperties.Subject);
            options.FetchProperties.Add(AppointmentProperties.Location);
            options.FetchProperties.Add(AppointmentProperties.AllDay);
            options.FetchProperties.Add(AppointmentProperties.StartTime);
            options.FetchProperties.Add(AppointmentProperties.Duration);

            IReadOnlyList<Appointment> appointments;

            if (localSettings.Values["onTheWay"] != null)
                gig.onTheWay = (bool)localSettings.Values["onTheWay"];
            else
                gig.onTheWay = false;

            if (gig.onTheWay)
            {
                if (localSettings.Values["startDate"] != null)
                    gig.startDate = (DateTimeOffset)localSettings.Values["startDate"];
                else
                    gig.startDate = DateTime.Now;

                if (localSettings.Values["startTime"] != null)
                    gig.startTime = (TimeSpan)localSettings.Values["startTime"];
                else
                    gig.startTime = DateTime.Now.TimeOfDay;

                appointments = await store.FindAppointmentsAsync(gig.startDate, gig.startTime, options);
            }
            else
                appointments = await store.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromHours(2), options);

            if (appointments.Count > 0)
            {
                var i = 0;

                while ((appointments[i].AllDay) && (i < appointments.Count))
                    i++;
                if (!appointments[i].AllDay)
                {
                    gig.clientName = appointments[i].Subject;
                    gig.venueName = appointments[i].Location;
                    gig.startDate = appointments[i].StartTime;
                    gig.startTime = gig.startDate.TimeOfDay;
                    gig.endDate = appointments[i].StartTime.Add(appointments[i].Duration);
                    gig.endTime = gig.endDate.TimeOfDay;
                }
                else
                {
                    gig.clientName = "New Client";
                    gig.venueName = "New Venue";
                    gig.startDate = DateTime.Now;
                    gig.startTime = DateTime.Now.TimeOfDay;
                    gig.endDate = DateTime.Now.Add(TimeSpan.FromHours(3));
                    gig.endTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay;
                }
            }
            else
            {
                gig.clientName = "New Client";
                gig.venueName = "New Venue";
                gig.startDate = DateTime.Now;
                gig.startTime = DateTime.Now.TimeOfDay;
                gig.endDate = DateTime.Now.Add(TimeSpan.FromHours(3));
                gig.endTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay;
            }
        }

        private async void addCalButton_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;
            gig.venueName = venueNameTextBox.Text;
            gig.clientNotes = clientNotesTextBox.Text;
            gig.startDate = startDatePicker.Date;
            gig.startTime = startTimePicker.Time;

            gig.totalHours = endTimePicker.Time - startTimePicker.Time;

            if (gig.totalHours < TimeSpan.FromDays(0))
            {
                gig.totalHours += TimeSpan.FromDays(1);
            }

            var cal = new Appointment();
            var date = startDatePicker.Date;
            var time = startTimePicker.Time;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var calTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0, timeZoneOffset);

            cal.StartTime = calTime;
            cal.Duration = gig.totalHours;
            cal.Location = gig.venueName;
            cal.Subject = gig.clientName;
            cal.Details = "CrewOnCall::" + ((ComboBoxItem)skillPicker.SelectedItem).Content.ToString() + "\n" + gig.clientNotes;
            cal.AllDay = false;
            cal.Reminder = TimeSpan.FromHours(2);

            await AppointmentManager.ShowAddAppointmentAsync(cal, new Rect(), Windows.UI.Popups.Placement.Default);
        }

        private async void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;
            gig.venueName = venueNameTextBox.Text;
            gig.startDate = startDatePicker.Date;
            gig.startTime = startTimePicker.Time;

            var date = gig.startDate.LocalDateTime.ToString("D");
            var time = gig.startTime.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "Confirming " + gig.clientName + " at " + gig.venueName + " on " + date + " at " + time + "\nGeorge";
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }


        private async void ontheway_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;
            localSettings.Values["onTheWay"] = true;
            localSettings.Values["startDate"] = startDatePicker.Date;
            localSettings.Values["startTime"] = startTimePicker.Time;

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "I am on the way to " + gig.clientName + "\nGeorge";
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }

        private async void sendtotalHours_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["onTheWay"] = false;
            gig.clientName = clientNameTextBox.Text;
            gig.startDate = startDatePicker.Date;
            gig.startTime = startTimePicker.Time;
            gig.endTime = endTimePicker.Time;

            gig.totalHours = endTimePicker.Time - startTimePicker.Time;

            if (gig.totalHours < TimeSpan.FromDays(0))
            {
                gig.totalHours += TimeSpan.FromDays(1);
            }
            gig.endDate = gig.startDate.Add(gig.totalHours);

            gig.breakLength = ((ComboBoxItem)breakLengthPicker.SelectedItem).Content.ToString();
            switch (gig.breakLength)
            {
                case "30 min":
                    gig.totalHours -= TimeSpan.FromMinutes(30);
                    break;
                case "45 min":
                    gig.totalHours -= TimeSpan.FromMinutes(45);
                    break;
                case "60 min":
                    gig.totalHours -= TimeSpan.FromMinutes(60);
                    break;
            }

            if (gig.totalHours < TimeSpan.FromDays(0))
            {
                gig.totalHours = TimeSpan.FromDays(0);
            }

            if ((startDatePicker.Date.DayOfWeek == 0) & (gig.totalHours < TimeSpan.FromHours(4)))
            {
                gig.totalHours = TimeSpan.FromHours(4);
                gig.totalTime = "4 hr call";
            }

            else
                if (gig.totalHours < TimeSpan.FromHours(3))
            {
                gig.totalHours = TimeSpan.FromHours(3);
                gig.totalTime = "3 hr call";
            }

            else
                gig.totalTime = gig.totalHours.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "Hours for " + gig.clientName + " = " + gig.totalTime + ".\n(" + gig.startTime.ToString(@"hh\:mm") + " - " + gig.endTime.ToString(@"hh\:mm") + ")\n" + gig.breakLength + " break.\nGeorge";
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
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

    public class Gig : INotifyPropertyChanged
    {
        private string client_name;
        private string venue_name;
        private string client_notes;
        private DateTimeOffset start_date;
        private DateTimeOffset end_date;
        private TimeSpan start_time;
        private TimeSpan end_time;
        private string break_length;
        private string total_time;
        private TimeSpan total_hours;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public bool onTheWay { get; set; }
        public string clientName
        {
            get { return client_name; }
            set
            {
                client_name = value;
                OnPropertyChanged("clientName");
            }
        }
        public string venueName
        {
            get { return venue_name; }
            set
            {
                venue_name = value;
                OnPropertyChanged("venueName");
            }
        }
        public string clientNotes
        {
            get { return client_notes; }
            set
            {
                client_notes = value;
                OnPropertyChanged("clientNotes");
            }
        }
        public DateTimeOffset startDate
        {
            get { return start_date; }
            set
            {
                start_date = value;
                OnPropertyChanged("startDate");
            }
        }
        public DateTimeOffset endDate
        {
            get { return end_date; }
            set
            {
                end_date = value;
                OnPropertyChanged("endDate");
            }
        }
        public TimeSpan startTime
        {
            get { return start_time; }
            set
            {
                start_time = value;
                OnPropertyChanged("startTime");
            }
        }
        public TimeSpan endTime
        {
            get { return end_time; }
            set
            {
                end_time = value;
                OnPropertyChanged("endTime");
            }
        }
        public string breakLength
        {
            get { return break_length; }
            set
            {
                break_length = value;
                OnPropertyChanged("breakLength");
            }
        }
        public string totalTime
        {
            get { return total_time; }
            set
            {
                total_time = value;
                OnPropertyChanged("totalTime");
            }
        }
        public TimeSpan totalHours
        {
            get { return total_hours; }
            set
            {
                total_hours = value;
                OnPropertyChanged("totalHours");
            }
        }

    }
}
