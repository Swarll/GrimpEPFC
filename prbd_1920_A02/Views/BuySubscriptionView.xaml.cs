using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prbd_1920_A02
{
    /// <summary>
    /// Logique d'interaction pour BuySubscriptionView.xaml
    /// </summary>
    public partial class BuySubscriptionView : UserControlBase
    {
        public Member Member { get; set; }

        public string RangeOA
        {
            get 
            {
                var age = Member.Age;
                var query = (from p in App.Model.Prices
                             where p.PassType == SelectedType
                             where p.RangeOfAge.Substring(0, 2).CompareTo(Member.Age.ToString()) <= 0
                             where p.RangeOfAge.Substring(2, 2).CompareTo(Member.Age.ToString()) > 0
                             select p.RangeOfAge).FirstOrDefault();
                return query;
            }
            set 
            {
                RangeOA = value;
                RaisePropertyChanged(nameof(RangeOA));
            }
        }

        public PassType SelectedType { get; set; }

        public SubscriptionType duration { get; set; }
        public SubscriptionType Duration 
        {
            get { return duration; } 
            set 
            {
                duration = value;
                EndingDate = DateTime.Now; // on appelle juste le setter, la valeur importe peu
                RaisePropertyChanged(nameof(Duration));
            } 
        }

        public string SubscriptionTitle
        {
            get { return SelectedType + " - " + RangeOA; }
            set
            {
                SubscriptionTitle = value;
                RaisePropertyChanged(nameof(SubscriptionTitle));
            }
        }

        public DateTime date { get; set; }
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                EndingDate = DateTime.Now; // on appelle juste le setter, la valeur importe peu
                RaisePropertyChanged(nameof(Date));
            } 
        }

        public DateTime endingDate { get; set; }
        public DateTime EndingDate 
        { 
            get { return endingDate; }
            set 
            {
                endingDate = Date.AddDays((int)Duration);
                Prix = ""; //on appelle juste le setter
                BuyText = "";
                RaisePropertyChanged(nameof(EndingDate)); 
            } 
        }

        public string Prix 
        {
            get
            {
                var query = (from p in App.Model.Prices
                             where p.PassType == SelectedType
                             where p.SubscriptionType == Duration
                             where p.RangeOfAge.Substring(0, 2).CompareTo(Member.Age.ToString()) <= 0
                             where p.RangeOfAge.Substring(2, 2).CompareTo(Member.Age.ToString()) > 0
                             select p.Cost).FirstOrDefault();
                return query.ToString() + "€";
            }
            set
            {
                RaisePropertyChanged(nameof(Prix));
            }
        }

        public string BuyText
        {
            get 
            {
                var expiration = MemberHasAlreadyPass();
                if (expiration != null)
                {
                    DateTime exp = (DateTime)expiration;
                    var toshow = exp.ToString("dd/MM/yyyy");

                    return Properties.Resources.BuySubscriptionView_Error1 + toshow;
                }
                else
                    return "";
            }
            set 
            {
                RaisePropertyChanged(nameof(BuyText));
            }
        }

        public DateTime? MemberHasAlreadyPass()
        {
            var query = (from s in App.Model.Subscriptions
                         where s.Member.MemberId == Member.MemberId
                         where DateTime.Compare(Date, s.ExpirationDate) < 0
                         where s.PassType == SelectedType
                         select (DateTime?)s.ExpirationDate).FirstOrDefault();
            return query;
        }

        public bool DatetimeIsValid()
        {
            return Date >= DateTime.Today;
        }

        private void FillComboBoxDuration()
        {
            comboBoxDuration.Items.Clear();
            foreach (SubscriptionType st in Enum.GetValues(typeof(SubscriptionType)))
            {
                comboBoxDuration.Items.Add(st);
            }
            Duration = SubscriptionType.DailyPass;
        }

        private void FillComboBox()
        {
            FillComboBoxDuration();
        }

        public ICommand BuySubscription { get; set; }

        public void BuyAction() 
        {
            App.Model.CreateSubscription(Member, SelectedType, Date, Duration);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_SUBSCRIPTIONS_CHANGED);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB);
        }

        public bool CanBuy()
        {
            return MemberHasAlreadyPass() == null && DatetimeIsValid();
        }

        public BuySubscriptionView(PassType selectedPass)
        {
            InitializeComponent();
            DataContext = this;
            FillComboBox();
            this.Member = App.CurrentUser;
            this.SelectedType = selectedPass;
            this.Date = DateTime.Now;
            BuySubscription = new RelayCommand(BuyAction, CanBuy);

            Refresh();
        }

        private void Refresh() { }

    }
}
