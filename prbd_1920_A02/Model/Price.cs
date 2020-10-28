using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class Price : EntityBase<Model>
    {
        [Key]
        public int PriceId { get; set; }
        public PassType PassType { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public string RangeOfAge { get; set; }
        public int Cost { get; set; }

        public override string ToString()
        {
            string str = IsCompetition() ? "Competition pass" : IsCourse() ? "Course pass" : "Normal pass";
            return "Price for " + str + " age range : " + RangeOfAge;
        }

        private bool IsCompetition() => this.PassType.Equals(PassType.Competition);
        private bool IsCourse() => this.PassType.Equals(PassType.Course);
        public bool IsInAgeRange(int age)
        {
            string[] str = RangeOfAge.Split('-');
            int minAge = int.Parse(str[0]), maxAge = int.Parse(str[1]);
            return age >= minAge && age <= maxAge;
        }

    }
}
