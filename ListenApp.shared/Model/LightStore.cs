
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;

namespace ListenApp.Model
{
    /// <summary>
    /// Persistance layer for Trips created with Adventure Works. Writes out and retreives trips 
    /// from a simple xml format like
    /// <root>
    ///  <Trip>
    ///   <Destination></Destination>
    ///   <Description></Description>
    ///   <StartDate></StartDate>
    ///   <EndDate></EndDate>
    ///   <Notes></Notes>
    ///  </Trip>
    ///  <Trip>
    ///   ....
    ///  </Trip>
    /// </root>
    /// </summary>
    public class LightStore
    {
        private bool loaded;

        /// <summary>
        /// Persist the loaded trips in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<Light> lights;

        public LightStore()
        {
            loaded = false;
            Lights = new ObservableCollection<Light>();
        }

        /// <summary>
        /// Persisted trips, reloaded across executions of the application
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
            }
        }

        /// <summary>
        /// Load trips from a file on first-launch of the app. If the file does not yet exist,
        /// pre-seed it with several trips, in order to give the app demonstration data.
        /// </summary>
        public async Task LoadLights()
        {
            // Ensure that we don't load trip data more than once.
            if (loaded)
            {
                return;
            }
            loaded = true;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.lights.Clear();

            var item = await folder.TryGetItemAsync("lights.xml");
            if (item == null)
            {
                // Add some 'starter' trips
                lights.Add(
                    new Light()
                    {
                        Room = "bathroom",
                        Description = "joy",
                        Color = "aaaaaa",
                        State = false
                    });
                lights.Add(
                    new Light()
                    {
                        Room = "livingroom",
                        Description = "calm",
                        Color = "eeeeee",
                        State = false
                    });
                lights.Add(
                    new Light()
                    {
                        Room = "Bedroom",
                        Description = "ice",
                        Color = "ffffff",
                        State = false
                    });
                lights.Add(
                    new Light()
                    {
                        Room = "Patio",
                        Description = "ice",
                        Color = "ffffff",
                        State = false
                    });
                lights.Add(
                    new Light()
                    {
                        Room = "Exterior",
                        Description = "ice",
                        Color = "ffffff",
                        State = false
                    });
                lights.Add(
                    new Light()
                    {
                        Room = "Balcony",
                        Description = "ice",
                        Color = "ffffff",
                        State = false
                    });
                lights.Add(
                   new Light()
                   {
                       Room = "courtyard",
                       Description = "ice",
                       Color = "ffffff",
                       State = false
                   });
                await WriteLights();
                return;
            }

            // Load trips out of a simple XML format. For the purposes of this example, we're treating
            // parse failures as "no trips exist" which will result in the file being erased.
            if (item.IsOfType(StorageItemTypes.File))
            {
                StorageFile lightsFile = item as StorageFile;

                string lightsXmlText = await FileIO.ReadTextAsync(lightsFile);

                try
                {
                    XElement xmldoc = XElement.Parse(lightsXmlText);

                    var lightElements = xmldoc.Descendants("Light");
                    foreach (var lightElement in lightElements)
                    {
                        Light light = new Light();

                        var destElement = lightElement.Descendants("Room").FirstOrDefault();
                        if (destElement != null)
                        {
                            light.Room = destElement.Value;
                        }

                        var descElement = lightElement.Descendants("Description").FirstOrDefault();
                        if (descElement != null)
                        {
                            light.Description = descElement.Value;
                        }


                        var notesElement = lightElement.Descendants("Color").FirstOrDefault();
                        if (notesElement != null)
                        {
                            light.Color = notesElement.Value;
                        }

                        var stateElement = lightElement.Descendants("State").FirstOrDefault();
                        var state = stateElement.Equals("true");
                        if (stateElement != null)
                        {
                            light.State = state;
                        }

                        Lights.Add(light);
                    }
                }
                catch (XmlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return;
                }

            }
        }

        /// <summary>
        /// Delete a trip from the persistent trip store, and save the trips file.
        /// </summary>
        /// <param name="light">The trip to delete. If the trip is not an existing trip in the store,
        /// will not have an effect.</param>
        public async Task DeleteLight(Light light)
        {
            Lights.Remove(light);
            await WriteLights();
        }

        /// <summary>
        /// Add a trip to the persistent trip store, and saves the trips data file.
        /// </summary>
        /// <param name="trip">The trip to save or update in the data file.</param>
        public async Task SaveLight(Light light)
        {
            if (!Lights.Contains(light))
            {
                Lights.Add(light);
            }
            await WriteLights();
        }


        /// <summary>
        /// Write out a new XML file, overwriting the existing one if it already exists
        /// with the currently persisted trips. See class comment for basic format.
        /// </summary>
        private async Task WriteLights()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            XElement xmldoc = new XElement("Root");

            StorageFile lightsFile;

            var item = await folder.TryGetItemAsync("lights.xml");
            if (item == null)
            {
                lightsFile = await folder.CreateFileAsync("lights.xml");
            }
            else
            {
                lightsFile = await folder.GetFileAsync("lights.xml");
            }
            foreach (var light in Lights)
            {

                xmldoc.Add(
                    new XElement("Light",
                    new XElement("Room", light.Room),
                    new XElement("Description", light.Description),
                    new XElement("Color", light.Color),
                    new XElement("State", light.State)));
            }
            await FileIO.WriteTextAsync(lightsFile, xmldoc.ToString());
        }
    }
}
