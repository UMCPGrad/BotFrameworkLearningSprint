using JayResortBot.Repository;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JayResortBot.Dialogs.Activity
{
    public class ActivityDialog : EnterpriseDialog
    {
        // Constants
        public const string NamePrompt = "activityNamePrompt";
        public const string LevelPrompt = "activityLevelPrompt";
        public const string dateTimePrompt = "activityDateTimePrompt";

        // Fields
        private static ActivityResponses _responder = new ActivityResponses();
        private IStatePropertyAccessor<ActivityState> _accessor;
        private IStatePropertyAccessor<OnboardingState> _customerAccessor;
        private ActivityState _state;
        private OnboardingState _customerState;

        public ActivityDialog(BotServices botServices, IStatePropertyAccessor<ActivityState> accessor, IStatePropertyAccessor<OnboardingState> customerAccessor)
            : base(botServices, nameof(ActivityDialog))
        {
            _accessor = accessor;
            _customerAccessor = customerAccessor;

            InitialDialogId = nameof(ActivityDialog);

            var activity = new WaterfallStep[]
            {
                AskForActivityName,
                AskForActivityLevel,
                AskForActivityDateTime,
                FinishActivityDialog,
            };

            AddDialog(new WaterfallDialog(InitialDialogId, activity));
            AddDialog(new ChoicePrompt(NamePrompt));
            AddDialog(new ChoicePrompt(LevelPrompt));
            AddDialog(new DateTimePrompt(dateTimePrompt));

        }

        public async Task<DialogTurnResult> AskForActivityName(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);

            if (!string.IsNullOrEmpty(_state.Name))
            {
                return await sc.NextAsync(_state.Name);
            }
            else
            {
                return await sc.PromptAsync(NamePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", ActivityResponses._activityNamePrompt),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Snorkeling", "Hiking", "Yoga" }),
                });
            }
        }

        public async Task<DialogTurnResult> AskForActivityLevel(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);
            var nameResult = (FoundChoice)sc.Result;
            _state.Name = nameResult.Value;
            var name = _state.Name;

            await _responder.ReplyWith(sc.Context, ActivityResponses._haveActivityName, new { name });

            if (!string.IsNullOrEmpty(_state.Level))
            {
                return await sc.NextAsync(_state.Level);
            }
            else
            {
                return await sc.PromptAsync(LevelPrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", ActivityResponses._activityLevelPrompt),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Beginner", "Average", "Expert "}),
                });
            }
        }

        public async Task<DialogTurnResult> AskForActivityDateTime(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);
            var levelResult = (FoundChoice)sc.Result;
            _state.Level = levelResult.Value;
            var level = _state.Level;
            var name = _state.Name;

            await _responder.ReplyWith(sc.Context, ActivityResponses._haveActivityLevel, new { name, level });

            if (_state.dateSet)
            {
                return await sc.NextAsync(_state.dateTime);
            }
            else
            {
                return await sc.PromptAsync(dateTimePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", ActivityResponses._activityDateTimePrompt),
                });
            }
        }

        public async Task<DialogTurnResult> FinishActivityDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);
            _customerState = await _customerAccessor.GetAsync(sc.Context);

            var customerName = _customerState.CustomerName;
            var name = _state.Name;
            var level = _state.Level;
            var dateTime = (List<DateTimeResolution>)sc.Result;
            var activityDate = dateTime?.FirstOrDefault()?.Value;
            //_state.dateTime = (DateTime)sc.Result;
            _state.dateSet = true;

            await _responder.ReplyWith(sc.Context, ActivityResponses._haveActivityDateTime, new { name, level, activityDate });

            Reservation reservation = new Reservation
            {
                Name = _customerState.CustomerName,
                ActivityName = _state.Name,
                ActivityLevel = _state.Level,
                ActivityDateTime = activityDate
            };

            ReservationRepository._reservations.Add(reservation);

            _state.Name = null;
            _state.Level = null;
            _state.dateSet = false;

            return await sc.EndDialogAsync();
        }
    }
}
