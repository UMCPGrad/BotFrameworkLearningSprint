using JayResortBot.Dialogs.Activity.Resources;
using Microsoft.Bot.Builder.TemplateManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JayResortBot.Dialogs.Activity
{
    public class ActivityResponses : TemplateManager
    {
        public const string _activityNamePrompt = "activityNamePrompt";
        public const string _haveActivityName = "haveActivityName";
        public const string _activityLevelPrompt = "activityLevelPrompt";
        public const string _haveActivityLevel = "haveActivityLevel";
        public const string _activityDateTimePrompt = "activityDateTimePrompt";
        public const string _haveActivityDateTime = "haveActivityDateTime";

        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    _activityNamePrompt,
                    (context, data) => ActivityStrings.NAME_PROMPT
                },
                {
                    _haveActivityName,
                    (context, data) => string.Format(ActivityStrings.HAVE_NAME, data.name)
                },
                {
                    _activityLevelPrompt,
                    (context, data) => ActivityStrings.LEVEL_PROMPT
                },
                {
                    _haveActivityLevel,
                    (context, data) => string.Format(ActivityStrings.HAVE_LEVEL, data.name, data.level)
                },
                {
                    _activityDateTimePrompt,
                    (context, data) => ActivityStrings.DATETIME_PROMPT
                },
                {
                    _haveActivityDateTime,
                    (context, data) => string.Format(ActivityStrings.HAVE_DATETIME, data.name, data.level, data.activityDate)
                },
            },
            ["en"] = new TemplateIdMap { },
            ["fr"] = new TemplateIdMap { },
        };

        public ActivityResponses()
        {
            this.Register(new DictionaryRenderer(_responseTemplates));
        }

    }
}
