//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using ListenApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListenApp.ViewModel
{
    /// <summary>
    /// ViewModelLocator ensures that viewmodels can be instantiated with a common reference to the TripStore. 
    /// This allows for easier decoupling of the store implementation and the view models, and allows for 
    /// less viewmodel specific code in the views.
    /// </summary>
    public class ViewModelLocator
    {
        private Dictionary<string, ViewModelBase> modelSet = new Dictionary<string, ViewModelBase>();

        /// <summary>
        /// Set up all of the known view models, and instantiate the trip repository.
        /// </summary>
        public ViewModelLocator()
        {
            LightStore store = new LightStore();
            InitializeStore(store);
            modelSet.Add("LightListViewModel", new LightListViewModel(store));
            modelSet.Add("LightViewModel", new LightViewModel(store));
            /*
            TripStore store = new TripStore();
            InitializeStore(store);
            modelSet.Add("TripListViewModel", new TripListViewModel(store));
            modelSet.Add("TripViewModel", new TripViewModel(store));
            */
        }

        private async void InitializeStore(LightStore store)
        {
 //           await store.LoadTrips();
            await store.LoadLights();
        }




        /// <summary>
        /// TripList (main page) view model. The TripListView is databound to this property.
        /// </summary>
        public LightListViewModel LightListViewModel
        {
            get
            {
                return (LightListViewModel)modelSet["LightListViewModel"];
            }
        }

        /// <summary>
        /// Trip (detail page) view model. TripDetails page is databound to this property.
        /// </summary>
        public LightViewModel LightViewModel
        {
            get
            {
                return (LightViewModel)modelSet["LightViewModel"];
            }
        }
        /// <summary>
        /// TripList (main page) view model. The TripListView is databound to this property.
        /// </summary>
        /*
        public TripListViewModel TripListViewModel
        {
            get
            {
                return (TripListViewModel)modelSet["TripListViewModel"];
            }
        }

        /// <summary>
        /// Trip (detail page) view model. TripDetails page is databound to this property.
        /// </summary>
        public TripViewModel TripViewModel
        {
            get
            {
                return (TripViewModel)modelSet["TripViewModel"];
            }
        }
        */
    }
}
