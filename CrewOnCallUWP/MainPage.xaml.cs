using System;
using System.Collections.Generic;
using System.Linq;
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

        ApplicationDataContainer localStorage = ApplicationData.Current.LocalSettings.CreateContainer("clientNames", ApplicationDataCreateDisposition.Always);

        public List<string> clientNames;

        Gig gig = new Gig
        {
            ClientName = null,
            VenueName = null,
            ClientNotes = null,
            StartDate = DateTime.Now,
            StartTime = DateTime.Now.TimeOfDay,
            EndDate = DateTime.Now.Add(TimeSpan.FromHours(3)),
            EndTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay,
            BreakOptions = new List<string> { "No", "30 min", "45 min", "60 min" },
            SkillOptions = new List<string> { "LEVEL3", "VANDVR", "MR/HR" },
            Skill = null,
            BreakLength = "No"
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
            for (int i = 0; i < ApplicationData.Current.LocalSettings.Containers["clientNames"].Values.Count; i++)
            {
                clientNames.Add(ApplicationData.Current.LocalSettings.Containers["clientNames"].Values.ElementAt(i).Value.ToString());
            }

            AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            FindAppointmentsOptions options = new FindAppointmentsOptions
            {
                MaxCount = 100
            };
            options.FetchProperties.Add(AppointmentProperties.Subject);
            options.FetchProperties.Add(AppointmentProperties.Location);
            options.FetchProperties.Add(AppointmentProperties.DetailsKind);
            options.FetchProperties.Add(AppointmentProperties.Details);
            options.FetchProperties.Add(AppointmentProperties.AllDay);
            options.FetchProperties.Add(AppointmentProperties.StartTime);
            options.FetchProperties.Add(AppointmentProperties.Duration);

            IReadOnlyList<Appointment> appointments;

            if (localSettings.Values["onTheWay"] != null)
                gig.OnTheWay = (bool)localSettings.Values["onTheWay"];
            else
                gig.OnTheWay = false;

            if (gig.OnTheWay)
            {
                if (localSettings.Values["startDate"] != null)
                    gig.StartDate = (DateTimeOffset)localSettings.Values["startDate"];
                else
                    gig.StartDate = DateTime.Now;

                if (localSettings.Values["startTime"] != null)
                    gig.StartTime = (TimeSpan)localSettings.Values["startTime"];
                else
                    gig.StartTime = DateTime.Now.TimeOfDay;

                var cal = new Appointment();
                var date = gig.StartDate.Date;
                var time = gig.StartTime;
                var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                var calTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0, timeZoneOffset);

                appointments = await store.FindAppointmentsAsync(calTime, TimeSpan.FromMinutes(1), options);
            }
            else
                appointments = await store.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromHours(2), options);

            if (appointments.Count > 0)
            {
                foreach (var appointment in appointments)

                    if (!appointment.AllDay)
                    {
                        gig.ClientName = appointment.Subject;
                        gig.VenueName = appointment.Location;
                        gig.ClientNotes = Windows.Data.Html.HtmlUtilities.ConvertToText(appointment.Details);
                        if (gig.ClientNotes.Contains("CrewOnCall::LEVEL3"))
                        {
                            gig.Skill = "LEVEL3";
                            gig.ClientNotes = gig.ClientNotes.Replace("CrewOnCall::LEVEL3", "");
                        }
                                
                        if (appointment.Details.Contains("CrewOnCall::VANDVR"))
                        {
                            gig.Skill = "VANDVR";
                            gig.ClientNotes = gig.ClientNotes.Replace("CrewOnCall::VANDVR", "");
                        }
                                
                        if (appointment.Details.Contains("CrewOnCall::MR/HR"))
                        {
                            gig.Skill = "MR/HR";
                            gig.ClientNotes = gig.ClientNotes.Replace("CrewOnCall::MR/HR", "");
                        }
                        gig.StartDate = appointment.StartTime;
                        gig.StartTime = gig.StartDate.TimeOfDay;
                        gig.EndDate = appointment.StartTime.Add(appointment.Duration);
                        gig.EndTime = gig.EndDate.TimeOfDay;
                        if (appointment.Duration > TimeSpan.FromHours(5))
                        {
                            gig.BreakLength = "30 min";
                        }
                        else
                        {
                            gig.BreakLength = "No";
                        }
                    }
                    else
                    {
                        gig.ClientName = (string)localSettings.Values["clientName"];
                        gig.VenueName = (string)localSettings.Values["venueName"];
                        gig.StartDate = DateTime.Now;
                        gig.StartTime = DateTime.Now.TimeOfDay;
                        gig.EndDate = DateTime.Now.Add(TimeSpan.FromHours(3));
                        gig.EndTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay;
                        gig.BreakLength = "No";
                    }
            }
            else
            {
                gig.ClientName = (string)localSettings.Values["clientName"];
                gig.VenueName = (string)localSettings.Values["venueName"];
                gig.StartDate = DateTime.Now;
                gig.StartTime = DateTime.Now.TimeOfDay;
                gig.EndDate = DateTime.Now.Add(TimeSpan.FromHours(3));
                gig.EndTime = DateTime.Now.Add(TimeSpan.FromHours(3)).TimeOfDay;
                gig.BreakLength = "No";
            }
        }

        private async void AddCalButton_Click(object sender, RoutedEventArgs e)
        {
            gig.ClientName = clientNameTextBox.Text;
            localSettings.Values["clientName"] = clientNameTextBox.Text;
            gig.VenueName = venueNameTextBox.Text;
            localSettings.Values["venueName"] = venueNameTextBox.Text;
            gig.ClientNotes = clientNotesTextBox.Text;
            gig.StartDate = startDatePicker.Date;
            gig.StartTime = startTimePicker.Time;

            gig.TotalHours = endTimePicker.Time - startTimePicker.Time;

            if (gig.TotalHours < TimeSpan.FromDays(0))
            {
                gig.TotalHours += TimeSpan.FromDays(1);
            }

            var cal = new Appointment();
            var date = startDatePicker.Date;
            var time = startTimePicker.Time;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(date.Add(time));
            var calTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0, timeZoneOffset);

            cal.StartTime = calTime;
            cal.Duration = gig.TotalHours;
            cal.Location = gig.VenueName;
            cal.Subject = gig.ClientName;
            cal.DetailsKind = AppointmentDetailsKind.PlainText;

            if ((skillPicker.SelectedIndex >= 0) && (skillPicker.SelectedItem != null))
            {
                cal.Details = "CrewOnCall::" + skillPicker.SelectedItem.ToString() + "\n" + gig.ClientNotes;
            }
            else
                cal.Details = "CrewOnCall::LEVEL3" + "\n" + gig.ClientNotes;


            cal.AllDay = false;
            cal.Reminder = TimeSpan.FromHours(2);

            try
            {
                await AppointmentManager.ShowAddAppointmentAsync(cal, new Rect(), Windows.UI.Popups.Placement.Default);
            }
            catch
            { }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            gig.ClientName = clientNameTextBox.Text;
            gig.VenueName = venueNameTextBox.Text;
            gig.StartDate = startDatePicker.Date;
            gig.StartTime = startTimePicker.Time;

            var date = gig.StartDate.LocalDateTime.ToString("D");
            var time = gig.StartTime.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage
            {
                Body = "Confirming " + gig.ClientName + " at " + gig.VenueName + " on " + date + " at " + time + "\nGeorge"
            };
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }


        private async void Ontheway_Click(object sender, RoutedEventArgs e)
        {
            gig.ClientName = clientNameTextBox.Text;
            localSettings.Values["onTheWay"] = true;
            localSettings.Values["startDate"] = startDatePicker.Date;
            localSettings.Values["startTime"] = startTimePicker.Time;

            var sms = new Windows.ApplicationModel.Chat.ChatMessage
            {
                Body = "I am on the way to " + gig.ClientName + "\nGeorge"
            };
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }

        private async void SendTotalHours_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["onTheWay"] = false;
            gig.ClientName = clientNameTextBox.Text;
            gig.StartDate = startDatePicker.Date;
            gig.StartTime = startTimePicker.Time;
            gig.EndTime = endTimePicker.Time;

            gig.TotalHours = endTimePicker.Time - startTimePicker.Time;

            if (gig.TotalHours < TimeSpan.FromDays(0))
            {
                gig.TotalHours += TimeSpan.FromDays(1);
            }
            gig.EndDate = gig.StartDate.Add(gig.TotalHours);

            if ((breakLengthPicker.SelectedIndex >= 0) && (breakLengthPicker.SelectedItem != null))
            {
                gig.BreakLength = breakLengthPicker.SelectedItem.ToString();
            }

            switch (gig.BreakLength)
            {
                case "30 min":
                    gig.TotalHours -= TimeSpan.FromMinutes(30);
                    break;
                case "45 min":
                    gig.TotalHours -= TimeSpan.FromMinutes(45);
                    break;
                case "60 min":
                    gig.TotalHours -= TimeSpan.FromMinutes(60);
                    break;
                default:
                    break;
            }

            if (gig.TotalHours < TimeSpan.FromDays(0))
            {
                gig.TotalHours = TimeSpan.FromDays(0);
            }

            if ((startDatePicker.Date.DayOfWeek == DayOfWeek.Sunday) & (gig.TotalHours < TimeSpan.FromHours(4)))
            {
                gig.TotalHours = TimeSpan.FromHours(4);
                gig.TotalTime = "4 hr call";
            }

            else
                if (gig.TotalHours < TimeSpan.FromHours(3))
            {
                gig.TotalHours = TimeSpan.FromHours(3);
                gig.TotalTime = "3 hr call";
            }

            else
                gig.TotalTime = gig.TotalHours.ToString(@"hh\:mm");

            var sms = new Windows.ApplicationModel.Chat.ChatMessage
            {
                Body = "Hours for " + gig.ClientName + " = " + gig.TotalTime + ".\n(" + gig.StartTime.ToString(@"hh\:mm") + " - " + gig.EndTime.ToString(@"hh\:mm") + ")\n" + gig.BreakLength + " break.\nGeorge"
            };
            sms.Recipients.Add("+61427015243");

            await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }

        private void ClearClientNotes_Click(object sender, RoutedEventArgs e)
        {
            gig.ClientNotes = null;
            gig.Skill = null;
        }

        private void ClientName_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void VenueName_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void ClientNotes_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void StartDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        { }

        private void SkillPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { }

        private void StartTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        { }

        private void BreakLength_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { }

        private void EndTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
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
}
