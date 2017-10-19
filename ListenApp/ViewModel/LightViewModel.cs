using ListenApp.Common;
using ListenApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel.VoiceCommands;
using System.Globalization;
using Windows.UI.Xaml;
using System.Diagnostics;


namespace ListenApp.ViewModel
{
    public class LightViewModel : ViewModelBase
    {
        private ICommand saveLightsCommand;
        private ICommand deleteLightsCommand;
        private Light light;
        private LightStore store;

        private bool showRoomValidation = false;
        private string roomValidationError;

        private bool showDelete = false;

        /// <summary>
        /// The Lights this view model represents.
        /// </summary>

        public Light Light
        {
            get
            {
                return light;
            }
            set
            {
                light = value;
                NotifyPropertyChanged("Light");

            }
        }
        /// <summary>
        /// We require that a destination be set to a non-empty string. If the user
        /// attempts to save without one, this will be set to an explanatory validation
        /// prompt to be rendered in the UI.
        /// </summary>
        public String RoomValidationError
        {
            get
            {
                return roomValidationError;
            }
            private set
            {
                roomValidationError = value;
                NotifyPropertyChanged("RoomValidationError");
            }
        }

        /// <summary>
        /// Controls whether the TextBlock that contains the destination validation string
        /// is visible. Combined with the VisibilityConverter, can show or collapse elements.
        /// </summary>
        public bool ShowRoomValidation
        {
            get
            {
                return showRoomValidation;
            }
            private set
            {
                showRoomValidation = value;
                NotifyPropertyChanged("ShowRoomValidation");
            }
        }


        /// <summary>
        /// Controls whether the Button that deletes trips is shown. If creating a new trip,
        /// this is false. Otherwise, true.
        /// </summary>
        public bool ShowDelete
        {
            get
            {
                return showDelete;
            }
            private set
            {
                showDelete = value;
                NotifyPropertyChanged("ShowDelete");
            }
        }

        /// <summary>
        /// Bound to the save button, provides a command to invoke when pressed.
        /// </summary>
        public ICommand SaveLightCommand
        {
            get
            {
                return saveLightsCommand;
            }
        }

        /// <summary>
        /// Load a trip fomr the store, and set up the view per a normal ShowTrip command.
        /// </summary>
        /// <param name="room"></param>
        internal async void LoadLightFromStore(string room)
        {
            Light t = store.Lights.Where(p => p.Room == room).FirstOrDefault();
            if (t != null)
            {
                this.ShowLight(t);
            }
            else
            {
                // Redirect back to the main page, and pass along an error message to show
                await Window.Current.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    App.NavigationService.Navigate<View.LightListView>(
                    string.Format(
                        "Sorry, couldn't find a light in Room {0}",
                        room));
                });
            }
        }

        /// <summary>
        /// Bound to the Delete button, provides a command to invoke when pressed.
        /// </summary>
        public ICommand DeleteLightCommand
        {
            get
            {
                return deleteLightsCommand;
            }
        }

        /// <summary>
        /// Whenever a trip is modified, we trigger an update of the voice command Phrase list. This allows
        /// voice commands such as "Adventure Works Show trip to {destination} to be up to date with saved
        /// Trips.
        /// </summary>
        public async Task UpdateRoomPhraseList()
        {
            try
            {
                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a trip, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                VoiceCommandDefinition commandDefinitions;

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "en-us";
                }

                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("ListenAppCommandSet_" + countryCode, out commandDefinitions))
                {
                    List<string> rooms = new List<string>();
                    foreach (Model.Light t in store.Lights)
                    {
                        rooms.Add(t.Room);
                    }
                    await commandDefinitions.SetPhraseListAsync("room", rooms);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Updating Phrase list for VCDs: " + ex.ToString());
            }
        }

        /// <summary>
        /// removes a trip from the store, and returns to the trip list.
        /// </summary>
        private async void DeleteLight()
        {
            await store.DeleteLight(Light);
            await UpdateRoomPhraseList();
            App.NavigationService.GoBack();
        }

        /// <summary>
        /// Performs validation on the destination to ensure it's not empty, then 
        /// saves a trip to the store. If the destination isn't valid, shows a validation
        /// error.
        /// </summary>
        private async void SaveLight()
        {
            ShowRoomValidation = false;
            bool valid = true;

            if (String.IsNullOrEmpty(Light.Room))
            {
                valid = false;
                ShowRoomValidation = true;
                RoomValidationError = "Room cannot be blank";
            }
            else
            {
                Light.Room = Light.Room.Trim();
            }

            if (valid)
            {
                await store.SaveLight(this.Light);
                await UpdateRoomPhraseList();
                App.NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Construct Trip ViewModel, providing the store to persist trips. 
        /// Creates the RelayCommands to be bound to various buttons in the UI.
        /// </summary>
        /// <param name="store">The persistent store</param>
        public LightViewModel(LightStore store)
        {
            light = new Light();
            saveLightsCommand = new RelayCommand(new Action(SaveLight));
            deleteLightsCommand = new RelayCommand(new Action(DeleteLight));
            this.store = store;
        }

        /// <summary>
        /// Sets up the view model to show an existing trip.
        /// </summary>
        /// <param name="light"></param>
        internal void ShowLight(Light light)
        {
            showDelete = true;
            Light = light;
        }

        /// <summary>
        /// Sets up the view model to create a new trip.
        /// </summary>
        internal void NewLight()
        {
            showDelete = false;
            Light = new Light();
        }


    }
}
