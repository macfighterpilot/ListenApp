
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ListenApp.Common;
using Windows.Media.SpeechRecognition;
using System.Linq;
using Windows.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Gpio;
using System.Threading.Tasks;

namespace ListenApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class. Contains initialization of the
    /// NavigationService, and handles Activation via methods other than normal user interaction (For example, voice commands/Cortana
    /// or URI invoking)
    /// </summary>
    sealed partial class App : Application
    {
        private GpioPin pin2, pin3, pin4, pin17, pin27, pin22, pin10, pin9;
        public Dictionary<int, GpioPin> pins = new Dictionary<int, GpioPin>();
        public Dictionary<string, int> pins_code = new Dictionary<string, int>();


        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            InitGPIO();


        }


        public static NavigationService NavigationService { get; private set; }

        private RootFrameNavigationHelper rootFrameNavigationHelper;

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                App.NavigationService = new NavigationService(rootFrame);

                // Use the RootFrameNavigationHelper to respond to keyboard and mouse shortcuts.
                this.rootFrameNavigationHelper = new RootFrameNavigationHelper(rootFrame);

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Determine if we're being activated normally, or with arguments from Cortana.
                if (string.IsNullOrEmpty(e.Arguments))
                {
                    // Launching normally.
                    rootFrame.Navigate(typeof(View.LightListView), "");
                }
                else
                {
                    // Launching with arguments. We assume, for now, that this is likely
                    // to be in the form of "room=<location>" from activation via Cortana.
                    rootFrame.Navigate(typeof(View.LightDetails), e.Arguments);
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();

            try
            {
                // Install the main VCD. Since there's no simple way to test that the VCD has been imported, or that it's your most recent
                // version, it's not unreasonable to do this upon app load.
                StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"ListenAppCommands.xml");

                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);

                // Update phrase list.
                ViewModel.ViewModelLocator locator = App.Current.Resources["ViewModelLocator"] as ViewModel.ViewModelLocator;

                if(locator != null)
                {
                    await locator.LightViewModel.UpdateRoomPhraseList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            Type navigationToPageType;
//            ViewModel.TripVoiceCommand? navigationCommand = null;
            ViewModel.LightVoiceCommand? voiceCommand = null;

            // If the app was launched via a Voice Command, this corresponds to the "show trip to <location>" command. 
            // Protocol activation occurs when a tile is clicked within Cortana (via the background task)
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                // The arguments can represent many different activation types. Cast it so we can get the
                // parameters we care about out.
                var commandArgs = args as VoiceCommandActivatedEventArgs;

                SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                // Get the name of the voice command and the text spoken. See AdventureWorksCommands.xml for
                // the <Command> tags this can be filled with.

                string voiceCommandName = speechRecognitionResult.RulePath[0];
                string textSpoken = speechRecognitionResult.Text;
                // The commandMode is either "voice" or "text", and it indictes how the voice command
                // was entered by the user.
                // Apps should respect "text" mode by providing feedback in silent form.
                string commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);
                // Access the value of the {room} phrase in the voice command
                // Access the value of the {state} phrase in the voice command
                string room = this.SemanticInterpretation("room", speechRecognitionResult);
                string state = this.SemanticInterpretation("state", speechRecognitionResult);
//                Debug.WriteLine(message: "App.xaml.cs"+ " | " + voiceCommandName + " | " + textSpoken + " | " + room + " | "+ state);

                switch (voiceCommandName)
                {
                    case "showDevicesOfRoom":

                        // Create a navigation command object to pass to the page. Any object can be passed in,
                        // here we're using a simple struct.
                        voiceCommand = new ViewModel.LightVoiceCommand(
                            voiceCommandName,
                            commandMode,
                            textSpoken,
                            room);

                        // Set the page to navigate to for this voice command.
                        navigationToPageType = typeof(View.LightDetails);
                        break;
                    case "changingStateOfLights":
                        Debug.WriteLine(room + " | " + state);

                        changeStateLightsinRoom(room, state);




                        voiceCommand = new ViewModel.LightVoiceCommand(
                            voiceCommandName,
                            commandMode,
                            textSpoken,
                            room);
                        navigationToPageType = typeof(View.LightDetails);
                        break;
                    default:
                        // If we can't determine what page to launch, go to the default entry point.
                        navigationToPageType = typeof(View.LightListView);
                        break;
                }
            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                // Extract the launch context. In this case, we're just using the destination from the phrase set (passed
                // along in the background task inside Cortana), which makes no attempt to be unique. A unique id or 
                // identifier is ideal for more complex scenarios. We let the destination page check if the 
                // destination trip still exists, and navigate back to the trip list if it doesn't.
                var commandArgs = args as ProtocolActivatedEventArgs;
                Windows.Foundation.WwwFormUrlDecoder decoder = new Windows.Foundation.WwwFormUrlDecoder(commandArgs.Uri.Query);
                var room = decoder.GetFirstValueByName("LaunchContext");
                voiceCommand = new ViewModel.LightVoiceCommand(
                                        "protocolLaunch",
                                        "text",
                                        "light",
                                        room);

                navigationToPageType = typeof(View.LightDetails);
            }
            else
            {
                // If we were launched via any other mechanism, fall back to the main page view.
                // Otherwise, we'll hang at a splash screen.
                navigationToPageType = typeof(View.LightListView);
            }

            // Re"peat the same basic initialization as OnLaunched() above, taking into account whether
            // or not the app is already active.
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                App.NavigationService = new NavigationService(rootFrame);

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // Since we're expecting to always show a details page, navigate even if 
            // a content frame is in place (unlike OnLaunched).
            // Navigate to either the main trip list page, or if a valid voice command
            // was provided, to the details page for that trip.
            rootFrame.Navigate(navigationToPageType, voiceCommand);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private string SemanticInterpretation(string interpretationKey, SpeechRecognitionResult speechRecognitionResult)
        {
            return speechRecognitionResult.SemanticInterpretation.Properties[interpretationKey].FirstOrDefault();
        }






















        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            pins_code.Add("bathroom", 2);
            pin2 = gpio.OpenPin(2);
            pin2.Write(GpioPinValue.Low);
            pin2.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(2, pin2);

            pins_code.Add("livingroom", 3);
            pin3 = gpio.OpenPin(3);
            pin3.Write(GpioPinValue.Low);
            pin3.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(3, pin3);

            pins_code.Add("bedroom", 4);
            pin4 = gpio.OpenPin(4);
            pin4.Write(GpioPinValue.Low);
            pin4.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(4, pin4);

            pins_code.Add("kitchen", 17);
            pin17 = gpio.OpenPin(17);
            pin17.Write(GpioPinValue.Low);
            pin17.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(17, pin17);

            pins_code.Add("patio", 27);
            pin27 = gpio.OpenPin(27);
            pin27.Write(GpioPinValue.Low);
            pin27.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(27, pin27);

            pins_code.Add("Exterior", 22);
            pin22 = gpio.OpenPin(22);
            pin22.Write(GpioPinValue.Low);
            pin22.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(22, pin22);

            pins_code.Add("Balcony", 10);
            pin10 = gpio.OpenPin(10);
            pin10.Write(GpioPinValue.Low);
            pin10.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(10, pin10);

            pins_code.Add("courtyard", 9);
            pin9 = gpio.OpenPin(9);
            pin9.Write(GpioPinValue.Low);
            pin9.SetDriveMode(GpioPinDriveMode.Output);
            pins.Add(9, pin9);
    }





        void changeStateLightsinRoom(string room, string state)
        {
            GpioPinValue val =  state.Equals("on") ? GpioPinValue.High : GpioPinValue.Low;
            pins[pins_code[room.ToLower()]].Write(val);

        }





        void StartScenario()
        {
            var gpio = GpioController.GetDefault();

            // Set up our GPIO pin for setting values.
            // If this next line crashes with a NullReferenceException,
            // then the problem is that there is no GPIO controller on the device.

            // Configure pin for output.
            pins[0].SetDriveMode(GpioPinDriveMode.Output);
        }

        void StopScenario()
        {
            // Release the GPIO pin.
            if (pins[0] != null)
            {
                pins[0].Dispose();
                pins[0] = null;
            }
        }

        void StartStopScenario()
        {
            if (pins[0] != null)
            {
                StopScenario();
            }
            else
            {
                StartScenario();
            }
        }

        private void SetPinHigh(int p)
        {
            // Set the pin value to High.
            pins[p].Write(GpioPinValue.High);
        }

        private void SetPinLow(int p)
        {
            // Set the pin value to Low.
            pins[p].Write(GpioPinValue.Low);
        }










}
}
