// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Teams.Integration.Common;
using TeamsVoiceRecordChatBot.Application.CognitiveModels;

namespace TeamsVoiceRecordChatBot.Application.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(FlightBookingRecognizer luisRecognizer, MediaRecordingDialog recordingDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(recordingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_luisRecognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

            //    return await stepContext.NextAsync(null, cancellationToken);
            //}

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? BuildIntroMessage();
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the MediaRecordDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(MediaRecordingDialog), new RecordDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            //var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            //switch (luisResult.TopIntent().intent)
            //{
            //    case FlightBooking.Intent.BookFlight:
            //        await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

            //        // Initialize BookingDetails with any entities we may have found in the response.
            //        var bookingDetails = new BookingDetails()
            //        {
            //            // Get destination and origin from the composite entities arrays.
            //            Destination = luisResult.ToEntities.Airport,
            //            Origin = luisResult.FromEntities.Airport,
            //            TravelDate = luisResult.TravelDate,
            //        };

            //        // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //        return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

            //    case FlightBooking.Intent.GetWeather:
            //        // We haven't implemented the GetWeatherDialog so we just display a TODO message.
            //        var getWeatherMessageText = "TODO: get weather flow here";
            //        var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
            //        await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
            //        break;

            //    default:
            //        // Catch all for unhandled intents
            //        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
            //        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            //        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            //        break;
            //}

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
        {
            var unsupportedCities = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
            {
                unsupportedCities.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
            {
                unsupportedCities.Add(toEntities.To);
            }

            if (unsupportedCities.Any())
            {
                var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is RecordDetails result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.RecordDate.ToShortDateString());
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you booked to {result.RecordStoragePath} from {result.Chanel.ChanelTitle} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private string BuildIntroMessage()
        {
            var message = $"{BotConversationHintsConstants.IntroMessageHint}" +
                          $"\n{BotConversationHintsConstants.BotPurposeMessageHint}" +
                          $"\n{BotConversationHintsConstants.CommandListMessageHint}" +
                          $"**{DialogMatchesAndCommands.StartRecord}** - starts media recording)\n\n" +
                          $"**{DialogMatchesAndCommands.StopRecord}** - stops media recording\n\n" +
                          $"**{DialogMatchesAndCommands.ShowRecord}** - shows last record\n\n" +
                          $"**{DialogMatchesAndCommands.FindRecord}** - find record (e.g. **{DialogMatchesAndCommands.FindRecord} 01/15/2019**) to find records for 15th January 2019\n\n" +
                          $"**{DialogMatchesAndCommands.Schedule}** - schedule recording (e.g. **{DialogMatchesAndCommands.Schedule} 04/20/2020**) to schedule recording for the 20th April\n\n";

            return message;
        }
    }
}
