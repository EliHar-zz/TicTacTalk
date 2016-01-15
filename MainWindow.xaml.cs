//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Project Oxford: http://ProjectOxford.ai
//
// ProjectOxford SDK Github:
// https://github.com/Microsoft/ProjectOxfordSDK-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//**************************************************************************** SPEECH TO TEXT

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TTSSample;
using Microsoft.ProjectOxford.SpeechRecognition;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading;
using SpeechToTextWPFSample;

namespace MicrosoftProjectOxfordExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static string speechResult = string.Empty;
        // You can also put the primary key in app.config, instead of using UI.
        string _subscriptionKey;

        string _luisAppID = ConfigurationManager.AppSettings["luisAppID"];
        string _luisSubscriptionID = ConfigurationManager.AppSettings["luisSubscriptionID"];

        string _recoLanguage = "en-us";

        private DataRecognitionClient _dataClient;
        private MicrophoneRecognitionClient _micClient;

        /// <summary>
        /// The MAIS reco response event
        /// </summary>
        private AutoResetEvent _FinalResponseEvent;

        /// <summary>
        /// Gets or sets subscription key
        /// </summary>
        public string SubscriptionKey
        {
            get
            {
                return _subscriptionKey;
            }

            set
            {
                _subscriptionKey = value;
                OnPropertyChanged<string>();
            }
        }

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        Thread anotherOne;

        public MainWindow()
        {
            InitializeComponent();
            //Intialize();

            SubscriptionKey = "b296eb666a2f48c6bbcecb602f26e3c1";
            // Short phrase recongition using microphone

            openMic();

            ///addd the thread here bro
            Logic myClass = new Logic();
            anotherOne = new Thread(myClass.DoWork);
            anotherOne.Start();

            _FinalResponseEvent = new AutoResetEvent(false);
        }

        public void openMic()
        {
            LogRecognitionStart("microphone", _recoLanguage, SpeechRecognitionMode.ShortPhrase);

            if (_micClient == null)
            {
                _micClient = CreateMicrophoneRecoClient(SpeechRecognitionMode.ShortPhrase, _recoLanguage, SubscriptionKey);
            }
            _micClient.StartMicAndRecognition();
        }

        /// <summary>
        //  Raises the System.Windows.Window.Closed event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (null != _dataClient)
            {
                _dataClient.Dispose();
            }

            if (null != _micClient)
            {
                _micClient.Dispose();
            }

            _FinalResponseEvent.Dispose();

            base.OnClosed(e);
        }

        private void LogRecognitionStart(string recoSource, string recoLanguage, SpeechRecognitionMode recoMode)
        {
           // WriteLine("\n--- Start speech recognition using " + recoSource + " with " + recoMode + " mode in " + recoLanguage + " language ----\n\n");
        }

        MicrophoneRecognitionClient CreateMicrophoneRecoClient(SpeechRecognitionMode recoMode, string language, string subscriptionKey)
        {
            MicrophoneRecognitionClient micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                recoMode,
                language,
                subscriptionKey);

            // Event handlers for speech recognition results
            micClient.OnMicrophoneStatus += OnMicrophoneStatus;
            micClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;

            micClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;


            micClient.OnConversationError += OnConversationErrorHandler;

            return micClient;
        }

        MicrophoneRecognitionClient CreateMicrophoneRecoClientWithIntent(string recoLanguage)
        {
           // WriteLine("--- Start microphone dictation with Intent detection ----");

            MicrophoneRecognitionClientWithIntent intentMicClient =
                SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(recoLanguage,
                                                                                 SubscriptionKey,
                                                                                 _luisAppID,
                                                                                 _luisSubscriptionID);
            intentMicClient.OnIntent += OnIntentHandler;

            // Event handlers for speech recognition results
            intentMicClient.OnMicrophoneStatus += OnMicrophoneStatus;
            intentMicClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            intentMicClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            intentMicClient.OnConversationError += OnConversationErrorHandler;

            intentMicClient.StartMicAndRecognition();

            return intentMicClient;

        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.projectoxford.ai/doc/general/subscription-key-mgmt");
        }

        /// <summary>
        ///     Speech recognition with data (for example from a file or audio source).  
        ///     The data is broken up into buffers and each buffer is sent to the Speech Recognition Service.
        ///     No modification is done to the buffers, so the user can apply their
        ///     own Silence Detection if desired.
        /// </summary>
        DataRecognitionClient CreateDataRecoClient(SpeechRecognitionMode recoMode, string recoLanguage)
        {
            DataRecognitionClient dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                recoMode,
                recoLanguage,
                SubscriptionKey);

            // Event handlers for speech recognition results
            if (recoMode == SpeechRecognitionMode.ShortPhrase)
            {
                dataClient.OnResponseReceived += OnDataShortPhraseResponseReceivedHandler;
            }
            else
            {
                dataClient.OnResponseReceived += OnDataDictationResponseReceivedHandler;
            }
            dataClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            dataClient.OnConversationError += OnConversationErrorHandler;

            return dataClient;
        }

        DataRecognitionClientWithIntent CreateDataRecoClientWithIntent(string recoLanguage, string wavFileName)
        {
            DataRecognitionClientWithIntent intentDataClient =
                SpeechRecognitionServiceFactory.CreateDataClientWithIntent(recoLanguage,
                                                                           SubscriptionKey,
                                                                           _luisAppID,
                                                                           _luisSubscriptionID);
            // Event handlers for speech recognition results
            intentDataClient.OnResponseReceived += OnDataShortPhraseResponseReceivedHandler;
            intentDataClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            intentDataClient.OnConversationError += OnConversationErrorHandler;

            // Event handler for intent result
            intentDataClient.OnIntent += OnIntentHandler;

            return intentDataClient;
        }

        private void SendAudioHelper(DataRecognitionClient dataClient, string wavFileName)
        {
            using (FileStream fileStream = new FileStream(wavFileName, FileMode.Open, FileAccess.Read))
            {
                // Note for wave files, we can just send data from the file right to the server.
                // In the case you are not an audio file in wave format, and instead you have just
                // raw data (for example audio coming over bluetooth), then before sending up any 
                // audio data, you must first send up an SpeechAudioFormat descriptor to describe 
                // the layout and format of your raw audio data via DataRecognitionClient's sendAudioFormat() method.

                int bytesRead = 0;
                byte[] buffer = new byte[1024];

                try
                {
                    do
                    {
                        // Get more Audio data to send into byte buffer.
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                        // Send of audio data to service. 
                        dataClient.SendAudio(buffer, bytesRead);
                    } while (bytesRead > 0);
                }
                finally
                {
                    // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
                    dataClient.EndAudio();
                }
            }
        }

        /// <summary>
        ///     Called when a final response is received; 
        /// </summary>
        /// 


        void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
               // WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

                _FinalResponseEvent.Set();

                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.
                _micClient.EndMicAndRecognition();

                // BUGBUG: Work around for the issue when cached _micClient cannot be re-used for recognition.
                _micClient.Dispose();
                _micClient = null;
                openMic();
                WriteResponseResult(e);
            }));
        }

        /// <summary>
        ///     Called when a final response is received; 
        /// </summary>
        void OnDataShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
               // WriteLine("--- OnDataShortPhraseResponseReceivedHandler ---");
                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.

                _FinalResponseEvent.Set();

                WriteResponseResult(e);
            }));
        }

        private void WriteResponseResult(SpeechResponseEventArgs e)
        {

            if (e.PhraseResponse.Results.Length == 0)
            {
               //// WriteLine("I didn't hear you...");
                openMic();
            }
            else
            {
                // Take action upon getting input 

                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    if (e.PhraseResponse.Results[i].Confidence == Microsoft.ProjectOxford.SpeechRecognition.Confidence.High || e.PhraseResponse.Results[i].Confidence == Microsoft.ProjectOxford.SpeechRecognition.Confidence.Normal)
                    {
                        speechResult = e.PhraseResponse.Results[i].DisplayText;
                        // Console.WriteLine(speechResult);

                        String mess = checkMessage(speechResult);
                        Console.WriteLine(mess + "   $$$$$$");
                        if (mess != null)
                        {
                            //notify logicc dat we gots ze word
                            Logic.setMessage(mess);
                            //Thread.Sleep(2000);
                            break;
                        }
                        else
                        {
                            TTSSample.Program.sayThis("Please repeat your move.");
                            break;
                        }
                    }
                    else
                    {
                        // Console.WriteLine("Say again...");
                        TTSSample.Program.sayThis("Please repeat your move.");
                        openMic();
                        break;
                    }
                }
               // WriteLine();
            }
        }
        
        /// <summary>
        ///     Called when a final response is received; 
        /// </summary>
        void OnMicDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
           // WriteLine("--- OnMicDictationResponseReceivedHandler ---");
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    _FinalResponseEvent.Set();

                    // we got the final result, so it we can end the mic reco.  No need to do this
                    // for dataReco, since we already called endAudio() on it as soon as we were done
                    // sending all the data.
                    _micClient.EndMicAndRecognition();

                    // BUGBUG: Work around for the issue when cached _micClient cannot be re-used for recognition.
                    _micClient.Dispose();
                    _micClient = null;
                }));
            }
            WriteResponseResult(e);
        }

        /// <summary>
        ///     Called when a final response is received; 
        /// </summary>
        void OnDataDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
           // WriteLine("--- OnDataDictationResponseReceivedHandler ---");
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    _FinalResponseEvent.Set();

                    // we got the final result, so it we can end the mic reco.  No need to do this
                    // for dataReco, since we already called endAudio() on it as soon as we were done
                    // sending all the data.
                }));
            }
            WriteResponseResult(e);
        }


        /// <summary>
        ///     Called when a final response is received and its intent is parsed 
        /// </summary>
        void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
           // WriteLine("--- Intent received by OnIntentHandler() ---");
           // WriteLine("{0}", e.Payload);
           // WriteLine();
        }

        /// <summary>
        ///     Called when a partial response is received.
        /// </summary>
        void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
           // WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
           // WriteLine("{0}", e.PartialResult);
           // WriteLine();
        }

        /// <summary>
        ///     Called when an error is received.
        /// </summary>
        void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
           // WriteLine("--- Error received by OnConversationErrorHandler() ---");
           // WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
           // WriteLine("Error text: {0}", e.SpeechErrorText);
           // WriteLine();
        }

        /// <summary>
        ///     Called when the microphone status has changed.
        /// </summary>
        void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
               // WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
               // WriteLine("********* Microphone status: {0} *********", e.Recording);
                if (e.Recording)
                {
                   // WriteLine("Please start speaking.");
                }
               // WriteLine();
            });
        }


        /// <summary>
        /// Writes the line.
        /// </summary>
        void WriteLine()
        {
           // WriteLine(string.Empty);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);
            Trace.WriteLine(formattedStr);
            Dispatcher.Invoke(() =>
            {
                _logText.Text += (formattedStr + "\n");
                _logText.ScrollToEnd();
            });
        }

        /// <summary>
        /// Helper function for INotifyPropertyChanged interface 
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="caller">Property name</param>
        private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(caller));
            }
        }

        private string checkMessage(String mess)
        {
            mess = mess.ToLower();
            Console.WriteLine(mess);
            switch (mess)
            {
                case "a1.":
                case "a 1.":
                case "a one.":
                    markSpot("A1", Logic.currentToken);
                    return "a1";
                case "a2.":
                case "a 2.":
                case "a two.":
                    markSpot("A2", Logic.currentToken);
                    return "a2";
                case "a3.":
                case "a 3.":
                case "a three.":
                    markSpot("A3", Logic.currentToken);
                    return "a3";
                case "b1.":
                case "b 1.":
                case "b one.":
                case "be 1.":
                case "be one.":
                case "bee 1.":
                case "bee one.":
                    markSpot("B1", Logic.currentToken);
                    return "b1";
                case "b2.":
                case "b 2.":
                case "b two.":
                case "be 2.":
                case "be two.":
                case "bee 2.":
                case "bee two.":
                    markSpot("B2", Logic.currentToken);
                    return "b2";
                case "b3.":
                case "b 3.":
                case "b three.":
                case "be 3.":
                case "be three.":
                case "bee 3.":
                case "bee three.":
                    markSpot("B3", Logic.currentToken);
                    return "b3";
                case "c1.":
                case "c 1.":
                case "c one.":
                case "see 1.":
                case "see one.":
                case "sea 1.":
                case "sea one.":
                    markSpot("C1", Logic.currentToken);
                    return "c1";
                case "c2.":
                case "c 2.":
                case "c two.":
                case "see 2.":
                case "see two.":
                case "sea 2.":
                case "sea two.":
                    markSpot("C2", Logic.currentToken);
                    return "c2";
                case "c3.":
                case "c 3.":
                case "c three.":
                case "see 3.":
                case "see three.":
                case "sea 3.":
                case "sea three.":
                    markSpot("C3", Logic.currentToken);
                    return "c3";
            }
            return null;
        }

        void markSpot(String s, char token)
        {
            switch (s)
            {
                case "A1":
                    A1.Text += token;
                    break;
                case "A2":
                    A2.Text += token;
                    break;
                case "A3":
                    A3.Text += token;
                    break;
                case "B1":
                    B1.Text += token;
                    break;
                case "B2":
                    B2.Text += token;
                    break;
                case "B3":
                    B3.Text += token;
                    break;
                case "C1":
                    C1.Text += token;
                    break;
                case "C2":
                    C2.Text += token;
                    break;
                case "C3":
                    C3.Text += token;
                    break;
            }
        }
    }
}
