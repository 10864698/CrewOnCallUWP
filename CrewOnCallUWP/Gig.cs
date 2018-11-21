using System;
using System.Collections.Generic;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CrewOnCallUWP
{
    public class Gig : INotifyPropertyChanged
    {
        private string client_name;
        private string venue_name;
        private string client_notes;
        private DateTimeOffset start_date;
        private DateTimeOffset end_date;
        private TimeSpan start_time;
        private TimeSpan end_time;
        private string skill;
        private string break_length;
        private string total_time;
        private TimeSpan total_hours;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool OnTheWay { get; set; }

        
        public string ClientName
        {
            get { return client_name; }
            set
            {
                client_name = value;
                OnPropertyChanged("ClientName");
            }
        }
        public string VenueName
        {
            get { return venue_name; }
            set
            {
                venue_name = value;
                OnPropertyChanged("VenueName");
            }
        }
        public string ClientNotes
        {
            get { return client_notes; }
            set
            {
                client_notes = value;
                OnPropertyChanged("ClientNotes");
            }
        }
        public DateTimeOffset StartDate
        {
            get { return start_date; }
            set
            {
                start_date = value;
                OnPropertyChanged("StartDate");
            }
        }
        public DateTimeOffset EndDate
        {
            get { return end_date; }
            set
            {
                end_date = value;
                OnPropertyChanged("EndDate");
            }
        }
        public TimeSpan StartTime
        {
            get { return start_time; }
            set
            {
                start_time = value;
                OnPropertyChanged("StartTime");
            }
        }
        public TimeSpan EndTime
        {
            get { return end_time; }
            set
            {
                end_time = value;
                OnPropertyChanged("EndTime");
            }
        }
        public List<string> SkillOptions { get; set; }
        public string Skill
        {
            get { return skill; }
            set
            {
                skill = value;
                OnPropertyChanged("Skill");
            }
        }
        public List<string> BreakOptions { get; set; }
        public string BreakLength
        {
            get { return break_length; }
            set
            {
                break_length = value;
                OnPropertyChanged("BreakLength");
            }
        }
        public string TotalTime
        {
            get { return total_time; }
            set
            {
                total_time = value;
                OnPropertyChanged("TotalTime");
            }
        }
        public TimeSpan TotalHours
        {
            get { return total_hours; }
            set
            {
                total_hours = value;
                OnPropertyChanged("TotalHours");
            }
        }

    }
}
