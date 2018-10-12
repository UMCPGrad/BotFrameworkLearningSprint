﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JayResortBot.Dialogs.Activity;
using JayResortBot.Dialogs.SpaServices;
using JayResortBot.Repository;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace JayResortBot
{
    public class MainDialog : RouterDialog
    {
        private BotServices _services;
        private UserState _userState;
        private ConversationState _conversationState;
        private MainResponses _responder = new MainResponses();

        public MainDialog(BotServices services, ConversationState conversationState, UserState userState)
            : base(nameof(MainDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _conversationState = conversationState;
            _userState = userState;

            AddDialog(new OnboardingDialog(_services, _userState.CreateProperty<OnboardingState>(nameof(OnboardingState))));
            // AddDialog(new EscalateDialog(_services));

            AddDialog(new ActivityDialog(_services, _userState.CreateProperty<ActivityState>(nameof(ActivityState)), _userState.CreateProperty<OnboardingState>(nameof(OnboardingState))));

            AddDialog(new SpaServicesDialog(_services, _userState.CreateProperty<SpaServicesState>(nameof(SpaServicesState)), _userState.CreateProperty<OnboardingState>(nameof(OnboardingState))));
        }

        protected override async Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var onboardingAccessor = _userState.CreateProperty<OnboardingState>(nameof(OnboardingState));
            var onboardingState = await onboardingAccessor.GetAsync(innerDc.Context, () => new OnboardingState());

            // var activityAccessor = _userState.CreateProperty<ActivityState>(nameof(ActivityState));
            // var activityState = await activityAccessor.GetAsync(innerDc.Context, () => new ActivityState());

            var view = new MainResponses();
            await view.ReplyWith(innerDc.Context, MainResponses.Intro);

            if (string.IsNullOrEmpty(onboardingState.CustomerName))
            {
                // This is the first time the user is interacting with the bot, so gather onboarding information.
                // await innerDc.BeginDialogAsync(nameof(ActivityDialog));
                await innerDc.BeginDialogAsync(nameof(OnboardingDialog));
            }
        }

        protected override async Task RouteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check dispatch result
            var dispatchResult = await _services.DispatchRecognizer.RecognizeAsync<Dispatch>(dc.Context, CancellationToken.None);
            var intent = dispatchResult.TopIntent().intent;

            if (intent == Dispatch.Intent.l_General)
            {
                // If dispatch result is general luis model
                _services.LuisServices.TryGetValue("general", out var luisService);

                if (luisService == null)
                {
                    throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");
                }
                else
                {
                    var result = await luisService.RecognizeAsync<General>(dc.Context, CancellationToken.None);

                    var generalIntent = result?.TopIntent().intent;

                    // switch on general intents
                    switch (generalIntent)
                    {
                        case General.Intent.SpaServices:
                            {
                                var activityAccessor = _userState.CreateProperty<SpaServicesState>(nameof(SpaServicesState));
                                var activityState = await activityAccessor.GetAsync(dc.Context, () => new SpaServicesState());
                                await dc.BeginDialogAsync(nameof(SpaServicesDialog));
                                break;
                            }
                        case General.Intent.ReserveActivity:
                            {
                                var activityAccessor = _userState.CreateProperty<ActivityState>(nameof(ActivityState));
                                var activityState = await activityAccessor.GetAsync(dc.Context, () => new ActivityState());
                                await dc.BeginDialogAsync(nameof(ActivityDialog));
                                break;
                            }
                        case General.Intent.ShowReservations:
                            {
                                if (ReservationRepository._reservations.Count == 0)
                                {
                                    await _responder.ReplyWith(dc.Context, MainResponses.No_Reservations);
                                }
                                else
                                {
                                    string listOfReservations = "";
                                    foreach (var item in ReservationRepository._reservations)
                                    {
                                        listOfReservations = listOfReservations + item.ToString(); 
                                    }
                                    
                                    await _responder.ReplyWith(dc.Context, MainResponses.Show_Reservations, new  { listOfReservations } );
                                }

                                break;
                            }
                        case General.Intent.Greeting:
                            {
                                // send greeting response
                                await _responder.ReplyWith(dc.Context, MainResponses.Greeting);
                                break;
                            }

                        case General.Intent.Help:
                            {
                                // send help response
                                await _responder.ReplyWith(dc.Context, MainResponses.Help);
                                break;
                            }

                        case General.Intent.Cancel:
                            {
                                // send cancelled response
                                await _responder.ReplyWith(dc.Context, MainResponses.Cancelled);

                                // Cancel any active dialogs on the stack
                                await dc.CancelAllDialogsAsync();
                                break;
                            }

                        case General.Intent.Escalate:
                            {
                                // start escalate dialog
                                await dc.BeginDialogAsync(nameof(EscalateDialog));
                                break;
                            }

                        case General.Intent.None:
                        default:
                            {
                                // No intent was identified, send confused message
                                await _responder.ReplyWith(dc.Context, MainResponses.Confused);
                                break;
                            }
                    }
                }
            }
            else if (intent == Dispatch.Intent.q_FAQ)
            {
                _services.QnAServices.TryGetValue("faq", out var qnaService);

                if (qnaService == null)
                {
                    throw new Exception("The specified QnAMaker Service could not be found in your Bot Services configuration.");
                }
                else
                {
                    var answers = await qnaService.GetAnswersAsync(dc.Context);

                    if (answers != null && answers.Count() > 0)
                    {
                        await dc.Context.SendActivityAsync(answers[0].Answer);
                    }
                }
            }
        }

        protected override async Task CompleteAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The active dialog's stack ended with a complete status
            await _responder.ReplyWith(innerDc.Context, MainResponses.Completed);
        }
    }
}
