
using JayResortBot.Dialogs.SpaServices.Resources;
using Microsoft.Bot.Builder.TemplateManager;

namespace JayResortBot.Dialogs.SpaServices
{


    public class SpaServicesResponses : TemplateManager
    {

        public const string _namePrompt = "namePrompt";
        public const string _haveName = "haveName";
        public const string _lengthPrompt = "lengthPrompt";
        public const string _haveLength = "haveLength";
        public const string _dateTimePrompt = "dateTimePrompt";
        public const string _haveDateTime = "haveDateTime";

        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    _namePrompt,
                    (context, data) => SpaServicesStrings.NAME_PROMPT
                },
                {
                    _haveName,
                    (context, data) => string.Format(SpaServicesStrings.HAVE_NAME, data.name)
                },
                {
                    _lengthPrompt,
                    (context, data) => SpaServicesStrings.LENGTH_PROMPT
                },
                {
                    _haveLength,
                    (context, data) => string.Format(SpaServicesStrings.HAVE_LENGTH, data.name, data.length)
                },
                {
                    _dateTimePrompt,
                    (context, data) => SpaServicesStrings.DATETIME_PROMPT
                },
                {
                    _haveDateTime,
                    (context, data) => string.Format(SpaServicesStrings.HAVE_DATETIME, data.name, data.length, data.serviceDate)
                },
            },
            ["en"] = new TemplateIdMap { },
            ["fr"] = new TemplateIdMap { },
        };

        public SpaServicesResponses()
        {
            this.Register(new DictionaryRenderer(_responseTemplates));
        }

    }
}