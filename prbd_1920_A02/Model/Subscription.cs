using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class Subscription : EntityBase<Model>
    {
        public int SubscriptionId { get; set; }
        public PassType PassType { get; set; }
        public SubscriptionType Type { get; set; }
        public string SubscriptionName { get; set; }
        public DateTime? Beginning { get; set; }
        public DateTime ExpirationDate { 
            get 
            {
                DateTime? temp = Beginning;
                return temp.Value.AddDays((int)Type); 
            } 
            set { } 
        }
        public virtual Price Price { get; set; }
        public virtual Member Member { get; set; }

        public bool IsValid()
        {
            return DateTime.Now >= Beginning && DateTime.Now < ExpirationDate;
        }
    }
}
