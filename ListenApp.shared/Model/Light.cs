
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListenApp.Model
{
    /// <summary>
    /// Trip model. Stores basic details about individual trips, retreived from the TripStore.
    /// Implements INotifyPropertyChanged to allow two-way data-binding at the UI layer.
    /// </summary>
    public class Light : INotifyPropertyChanged
    {
        private string room;
        private string description;
        private string color;
        private bool state;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Trip Destination. Should not be an empty string.
        /// </summary>
        public string Room
        {
            get
            {
                return room;
            }
            set
            {
                room = value;
                NotifyPropertyChanged("Room");
            }
        }

        /// <summary>
        /// Description of a trip. Shown to users as a basic description in the UI.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Any notes about a trip, such as things to take, activities, etc.
        /// </summary>
        public string Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                NotifyPropertyChanged("Color");
            }
        }

        public bool State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                NotifyPropertyChanged("State");
            }
        }
        /// <summary>
        /// Notify any subscribers to the INotifyPropertyChanged interface that a property
        /// was updated. This allows the UI to automatically update (for instance, if Cortana
        /// triggers an update to a trip, or removal of a trip).
        /// </summary>
        /// <param name="propertyName">The case-sensitive name of the property that was updated.</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                handler(this, args);
            }
        }
    }
}
