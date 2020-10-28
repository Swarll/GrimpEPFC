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
    /// Logique d'interaction pour SubscriptionsView.xaml
    /// </summary>
    public partial class SubscriptionsView : UserControlBase
    {
        public Member Member { get; set; }
        public bool IsAdmin { get => Member.Role == Role.Admin; }

        private DataView prices;
        public DataView Prices
        {
            get { return prices; }
            set
            {
                prices = value;
                RaisePropertyChanged(nameof(Prices));
            }
        }

        public ICommand DisplayBuySubscription { get; set; }

        public SubscriptionType subscriptionType { get; set; }

        private PassType selectedPass { get; set; }
        public PassType SelectedPass 
        { 
            get { return selectedPass; } 
            set 
            {
                selectedPass = value;
                RaisePropertyChanged(nameof(SelectedPass)); 
                App.NotifyColleagues(AppMessages.MSG_SELECTEDPASS_CHANGED);
        } 
        }

        public ICollection<PassType> PassTypes
        { 
            get {
                PassType[] types = new PassType[3];
                types[0] = PassType.Pass;
                types[1] = PassType.Course;
                types[2] = PassType.Competition;
                return types;
            } 
            set { } 
        }

        public bool memberHasNoSubscription { get; set; }
        public bool MemberHasNoSubscription
        {
            get { return memberHasNoSubscription; }
            set 
            { 
                memberHasNoSubscription = !(Member.HasCoursePass() || Member.HasNormalPass() || Member.HasCompetitionPass());
                RaisePropertyChanged(nameof(MemberHasNoSubscription));
            }
        }

        public bool memberHasSubscription { get; set; }
        public bool MemberHasSubscription
        {
            get { return memberHasSubscription; }
            set
            {
                memberHasSubscription = (Member.HasCoursePass() || Member.HasNormalPass() || Member.HasCompetitionPass());
                RaisePropertyChanged(nameof(MemberHasSubscription));
            }
        }

        private DataView subscriptions;
        public DataView Subscriptions
        {
            get { return subscriptions; }
            set
            {
                subscriptions = value;
                RaisePropertyChanged(nameof(Subscriptions));
            }
        }

        public SubscriptionsView()
        {
            InitializeComponent();
            DataContext = this;
            Member = App.CurrentUser;
            SelectedPass = PassType.Pass;

            DisplayBuySubscription = new RelayCommand(NotifySubscriptionPayment);

            App.Register(this, AppMessages.MSG_SUBSCRIPTIONS_CHANGED, () => { Refresh(); });
            App.Register(this, AppMessages.MSG_SELECTEDPASS_CHANGED, () => { RefreshPricesTable(); });
            App.Register<Member>(this, AppMessages.MSG_MEMBER_DELETED, m => { RefreshSubscriptionsTable(); });

            Refresh();
        }

        private void Refresh()
        {
            MemberHasNoSubscription = true;
            MemberHasSubscription = true;
            // refresh table des prix
            RefreshPricesTable();
            //refresh table des subscriptions
            RefreshSubscriptionsTable();

            DisplayBuySubscription = new RelayCommand(NotifySubscriptionPayment);
        }

        private void NotifySubscriptionPayment()
        {
            App.NotifyColleagues(AppMessages.MSG_DISPLAY_BUY_SUBSCRIPTION, SelectedPass);
        }

        private void RefreshPricesTable()
        {
            var table = new DataTable();
            var columns = new Dictionary<int, String>();
            table = new DataTable();
            table.Columns.Add("Age range");
            int i = 1;
            var oldRow = "";
            foreach (String st in Enum.GetNames(typeof(SubscriptionType)))
            {
                table.Columns.Add(st);
                columns[i] = st;
                ++i;
            }
            foreach (var p in (App.Model.Prices
                                        .Where(p => p.PassType == SelectedPass)
                                        .OrderBy(p => p.RangeOfAge)
                                        .Select(p => new { p.PriceId, p.PassType, p.SubscriptionType, p.RangeOfAge, p.Cost })))
            {
                if (p.RangeOfAge != oldRow)
                {
                    var row = table.NewRow();
                    row[0] = p.RangeOfAge;
                    for (int j = 1; j < table.Columns.Count; ++j)
                    {
                        String st = columns[j];
                        var query = (from prix in App.Model.Prices
                                     where prix.RangeOfAge == p.RangeOfAge
                                     where prix.SubscriptionType.ToString().CompareTo(st) == 0
                                     where prix.PassType == SelectedPass
                                     select prix).FirstOrDefault();
                        if (query != null && p.RangeOfAge == query.RangeOfAge)
                            row[j] = query.Cost;
                    }
                    table.Rows.Add(row);
                    oldRow = p.RangeOfAge;
                }
            }
            Prices = table.DefaultView;
        }

        private void RefreshSubscriptionsTable() 
        {
            var table = new DataTable();
            var columns = new Dictionary<int, String>();
            table = new DataTable();

            table.Columns.Add("Pass type");
            columns[0] = "Pass type";
            table.Columns.Add("Duration");
            columns[1] = "Duration";
            table.Columns.Add("Beginning");
            columns[2] = "Beginning";
            table.Columns.Add("Ending");
            columns[3] = "Ending";
            table.Columns.Add("Price");
            columns[4] = "Price";

            foreach (var s in (App.Model.Subscriptions
                                        .Where(s => s.Member.MemberId == Member.MemberId)
                                        .Select(s => new { s.SubscriptionId, s.PassType, s.Type, s.Beginning, s.ExpirationDate, s.Price.Cost})))
            {
                var row = table.NewRow();
                row[0] = s.PassType;
                row[1] = (int)s.Type + " day(s)";
                var beg = s.Beginning;
                row[2] = String.Format("{0:dd/MM/yyyy}", beg);
                var end = s.ExpirationDate;
                row[3] = String.Format("{0:dd/MM/yyyy}", end);
                row[4] = s.Cost + " €";
                table.Rows.Add(row);
            }
            Subscriptions = table.DefaultView;
        }
    }
}
