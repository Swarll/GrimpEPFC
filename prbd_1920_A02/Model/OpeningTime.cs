using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class OpeningTime : EntityBase<Model>
    {
        [Key]
        public int OpeningId { get; set; }
        public int DayOFWeek { get; set; }
        public string OpeningHour { get; set; }
        public string ClosureHour { get; set; }
        public string Name { get => this.ToString(); set { } }

        public override string ToString()
        {
            switch (this.DayOFWeek)
            {
                case 0 : return "Monday";
                case 1 : return "Tuesday";
                case 2 : return "Wednesday";
                case 3 : return "Thursday";
                case 4 : return "Friday";
                case 5 : return "SaturDay";
                case 6 : return "Sunday";
                default: return "Bad day format";
            }
        }
    }
}
