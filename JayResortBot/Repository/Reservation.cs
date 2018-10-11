using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JayResortBot.Repository
{
    public class Reservation
    {
        public string Name { get; set; }
        public string ActivityName { get; set; }
        public string ActivityLevel { get; set; }
        public string  ActivityDateTime { get; set; }

        public override string ToString()
        {
            return "\n\nCustomer Name: " + this.Name + "\n" +
                   "Activity: " + this.ActivityName + "\n" +
                   "Skill Level: " + this.ActivityLevel + "\n" +
                   "Date/Time: " + this.ActivityDateTime + "\n\n";
        }
    }
}
