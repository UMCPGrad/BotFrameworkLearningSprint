using JayResortBot.Repository;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JayResortBot.Dialogs.SpaServices
{
    public class SpaServicesDialog : EnterpriseDialog
    {

        // Constants
        public const string NamePrompt = "namePrompt";
        public const string LengthPrompt = "lengthPrompt";
        public const string DateAndTimePrompt = "dateAndTimePrompt";

        // Fields
        private static SpaServicesResponses _responder = new SpaServicesResponses();
        private IStatePropertyAccessor<SpaServicesState> _accessor;
        private IStatePropertyAccessor<OnboardingState> _customerAccessor;
        private SpaServicesState _state;
        private OnboardingState _customerState;
        public SpaServicesDialog(BotServices botServices, IStatePropertyAccessor<SpaServicesState> accessor, IStatePropertyAccessor<OnboardingState> customerAccessor) 
            : base(botServices, nameof(SpaServicesDialog))
        {

            _accessor = accessor;
            _customerAccessor = customerAccessor;

            InitialDialogId = nameof(SpaServicesDialog);
            
            var spaServices = new WaterfallStep[]
            {
                AskForName,
                AskForLength,
                AskForDateTime,
                FinishOnboardingDialog,
            };

            AddDialog(new WaterfallDialog(InitialDialogId, spaServices));
            AddDialog(new ChoicePrompt(NamePrompt));
            AddDialog(new ChoicePrompt(LengthPrompt));
            AddDialog(new DateTimePrompt(DateAndTimePrompt));
            

        }

        public async Task<DialogTurnResult> AskForName(WaterfallStepContext sc, CancellationToken cancellationToken)
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
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", SpaServicesResponses._namePrompt),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Massage", "Manipedi", "Sauna" }),
                });

            }
        }



        public async Task<DialogTurnResult> AskForLength(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);

            var nameResult = (FoundChoice)sc.Result;
            _state.Name = nameResult.Value;

            var name = _state.Name;
          

            await _responder.ReplyWith(sc.Context, SpaServicesResponses._haveName, new { name });

            if (!string.IsNullOrEmpty(_state.Length))
            {
                return await sc.NextAsync(_state.Length);
            }
            else
            {
                return await sc.PromptAsync(LengthPrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", SpaServicesResponses._lengthPrompt),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "30 Minutes", "60 Minutes", "90 Minutes" }),
                });
            }
        }


        public async Task<DialogTurnResult> AskForDateTime(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);
            var lengthResult = (FoundChoice)sc.Result;

            _state.Length = lengthResult.Value;
            
            var name = _state.Name;
            var length = _state.Length;

            await _responder.ReplyWith(sc.Context, SpaServicesResponses._haveLength, new {name, length });

            if (!string.IsNullOrEmpty(_state.DateTime))
            {
                return await sc.NextAsync(_state.DateTime);
            }
            else
            {
                return await sc.PromptAsync(DateAndTimePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, "en", SpaServicesResponses._dateTimePrompt),
                });
            }
        }

        public async Task<DialogTurnResult> FinishOnboardingDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessor.GetAsync(sc.Context);
            _customerState = await _customerAccessor.GetAsync(sc.Context);

            var customerName = _customerState.CustomerName;
            var name = _state.Name;
            var length = _state.Length;
            var dateTime = (List<DateTimeResolution>)sc.Result;
            var serviceDate = dateTime?.FirstOrDefault()?.Value;
            _state.DateTime = serviceDate;

            await _responder.ReplyWith(sc.Context, SpaServicesResponses._haveDateTime, new { name, length, serviceDate });

            //Reservation reservation = new Reservation
            //{
            //    Name = _customerState.CustomerName,
            //    ActivityName = _state.Name,
            //    ActivityLevel = _state.Length,
            //    ActivityDateTime = serviceDate
            //};

            //ReservationRepository._reservations.Add(reservation);

            //_state.Name = null;
            //_state.Length = null;
            //_state.DateTime = null;

            return await sc.EndDialogAsync();
        }
    }
}
