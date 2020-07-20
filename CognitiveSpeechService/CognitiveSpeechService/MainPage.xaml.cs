using CognitiveSpeechService.Services;
using Microsoft.CognitiveServices.Speech;
using System;
using Xamarin.Forms;

namespace CognitiveSpeechService
{
    public partial class MainPage : ContentPage
    {
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;

        public MainPage()
        {
            InitializeComponent();
            micService = DependencyService.Resolve<IMicrophoneService>();
        }

        async void TranscribeClicked(object sender, EventArgs e)
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);
                config.SpeechRecognitionLanguage = "es-ES";
                recognizer = new SpeechRecognizer(config);
                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
            }

            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = false;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InsertDateTimeRecord();
                });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = true;
            }
            UpdateDisplayState();
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    transcribedText.Text += $"{newText}\n";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"=================\n{DateTime.Now.ToString()}\n=================";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isTranscribing)
                {
                    transcribeButton.Text = "Parar";
                    transcribeButton.BackgroundColor = Color.Red;
                    transcribingIndicator.IsRunning = true;
                }
                else
                {
                    transcribeButton.Text = "Grabar";
                    transcribeButton.BackgroundColor = Color.Green;
                    transcribingIndicator.IsRunning = false;
                }
            });
        }
    }
}
