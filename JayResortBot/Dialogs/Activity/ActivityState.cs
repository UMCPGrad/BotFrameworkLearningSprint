using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JayResortBot.Dialogs.Activity
{
    public class ActivityState
    {
        public string Name { get; set; }

        public string Level { get; set; }

        public DateTime dateTime { get; set; }

        public Boolean dateSet { get; set; }
    }
}
