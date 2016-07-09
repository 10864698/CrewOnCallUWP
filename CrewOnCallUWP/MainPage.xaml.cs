using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
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
        Gig gig = new Gig
        {
            clientName = "New Client",
            venueName = "New Venue",
            testDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
            //startDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("D"),
            startTime = DateTime.Now.ToString(@"hh\:mm"),
            endTime = DateTime.Now.Subtract(TimeSpan.FromHours(3)).ToString(@"hh\:mm")
        };

        public MainPage()
        {
            InitializeComponent();
            DataContext = gig;

            //Initialize();

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
            IReadOnlyList<Appointment> appointments = await store.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromHours(2), options);

            if (appointments.Count > 0)
            {
                var i = 0;

                while ((appointments[i].AllDay) && (i < appointments.Count))
                    i++;
                if (!appointments[i].AllDay)
                {
                    gig.clientName = appointments[i].Subject;
                    gig.venueName = appointments[i].Location;
                    gig.startTime = appointments[i].StartTime.ToString();
                }
                else
                {
                    gig.clientName = "Client TEST appointment count > 0";
                    gig.venueName = "Venue";
                    gig.startTime = DateTime.Now.ToString();
                }
            }
            else
            {
                gig.clientName = "Client TEST appointment count =< 0";
                gig.venueName = "Venue";
                gig.startTime = DateTime.Now.ToString();
            }
        }

        private async void addCalButton_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;
            gig.venueName = venueNameTextBox.Text;
            gig.clientNotes = clientNotesTextBox.Text;
            gig.startDate = startDatePicker.Date.ToString("D");
            gig.startTime = startTimePicker.Time.ToString(@"hh\:mm");
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
            gig.startDate = startDatePicker.Date.ToString("D");
            gig.startTime = startTimePicker.Time.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "Confirming " + gig.clientName + " at " + gig.venueName + " on " + gig.startDate + " at " + gig.startTime + "\nGeorge";
            sms.Recipients.Add("+61490139009");
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }


        private async void ontheway_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;

            var sms = new Windows.ApplicationModel.Chat.ChatMessage();
            sms.Body = "I am on the way to " + gig.clientName + "\nGeorge";
            sms.Recipients.Add("+61490139009");
            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);

        }

        private async void sendtotalHours_Click(object sender, RoutedEventArgs e)
        {
            gig.clientName = clientNameTextBox.Text;

            if (VerifyTimeIsAvailable(startTimePicker.Time) == true)
            {
                gig.startTime = startTimePicker.Time.ToString(@"hh\:mm");
            }
            else
            {
                gig.startTime = DateTime.Now.Subtract(TimeSpan.FromHours(3)).ToString(@"hh\:mm");
            }

            if (VerifyTimeIsAvailable(endTimePicker.Time) == true)
            {
                gig.endTime = endTimePicker.Time.ToString(@"hh\:mm");
            }
            else
            {
                gig.endTime = DateTime.Now.ToString(@"hh\:mm");
            }

            gig.totalHours = endTimePicker.Time - startTimePicker.Time;

            if (gig.totalHours < TimeSpan.FromDays(0))
            {
                gig.totalHours += TimeSpan.FromDays(1);
            }

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
            sms.Body = "Hours for " + gig.clientName + " = " + gig.totalTime + ".\n(" + gig.startTime + " - " + gig.endTime + ")\n" + gig.breakLength + " break.\nGeorge";
            sms.Recipients.Add("+61490139009");
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

    public class Gig
    {
        public string clientName { get; set; }
        public string venueName { get; set; }
        public string clientNotes { get; set; }
        public string startDate { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string breakLength { get; set; }
        public string totalTime { get; set; }
        public TimeSpan totalHours { get; set; }

        public DateTimeOffset testDate { get; set; }

    }
}
