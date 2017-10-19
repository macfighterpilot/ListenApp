
using ListenApp.Common;
using ListenApp.Model;
using ListenApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ListenApp.ViewModel
{
    public class LightListViewModel : ViewModelBase
    {
        private ICommand addLightCommand;
        private ObservableCollection<Light> lights;
        private Light selectedLight;
        private LightStore store;

        /// <summary>
        /// The list of trips to display on the UI.
        /// </summary>
        public ObservableCollection<Light> Lights
        {
            get
            {
                return lights;
            }
            private set
            {
                lights = value;
                NotifyPropertyChanged("Lights");
            }
        }

        /// <summary>
        /// The implementation of the command to execute when the Add button is pressed.
        /// </summary>
        private void AddLight()
        {
            App.NavigationService.Navigate<LightDetails>();
        }


        /// <summary>
        /// Construct the trip view, passing in the persistent trip store. Sets up
        /// a command to handle invoking the Add button.
        /// </summary>
        /// <param name="store"></param>
        public LightListViewModel(LightStore store)
        {
            this.store = store;
            Lights = store.Lights;

            addLightCommand = new RelayCommand(new Action(AddLight));
        }





        /// <summary>
        /// A two-way binding that keeps reference to the currently selected trip on
        /// the UI.
        /// </summary>
        public Light SelectedLight
        {
            get
            {
                return selectedLight;
            }
            set
            {
                selectedLight = value;
                NotifyPropertyChanged("SelectedLight");
            }
        }

        /// <summary>
        /// The command to invoke when the Add button is pressed.
        /// </summary>
        public ICommand AddLightCommand
        {
            get
            {
                return addLightCommand;
            }
        }

        /// <summary>
        /// Reload the trip store from data files.
        /// </summary>
        internal async Task LoadLights()
        {
            await store.LoadLights();
        }

        /// <summary>
        /// Handles a user selecting a trip on the UI by navigating to the TripDetails view
        /// for that trip.
        /// </summary>
        internal void SelectionChanged()
        {
            if (SelectedLight != null)
            {
                App.NavigationService.Navigate<LightDetails>(SelectedLight);
            }
        }









    }
}
