using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PRBD_Framework;

namespace prbd_1920_A02
{
    /// <summary>
    /// Logique d'interaction pour CompetitionDetailView.xaml
    /// </summary>
    public partial class CompetitionDetailView : UserControlBase
    {
        public Member Member { get; set; }
        public Competition Competition { get; set; }

        private ObservableCollection<Member> members;
        public ObservableCollection<Member> Members
        {
            get => members;
            set
            {
                SetProperty(ref members, value);
                RaisePropertyChanged(nameof(ParticipantsView));
            }
        }

        private CollectionView participantsView = null;
        public CollectionView ParticipantsView
        {
            get
            {
                participantsView = (CollectionView)CollectionViewSource.GetDefaultView(members);
                if (participantsView != null && participantsView.SortDescriptions.Count == 0)
                    participantsView.SortDescriptions.Add(new SortDescription("Pseudo", ListSortDirection.Descending));
                return participantsView;
            }
        }

        public int MaxParticipants
        {
            get { return Competition.MaxParticipants; }
            set
            {
                Competition.MaxParticipants = value;
                RaisePropertyChanged(nameof(MaxParticipants));
            }
        }

        public int NbrOfParticipants
        {
            get { return Competition.NbrOfParticipants; }
            set
            {
                Competition.NbrOfParticipants = value;
                RaisePropertyChanged(nameof(NbrOfParticipants));
            }
        }

        private CompetitionType type { get; set; }
        public CompetitionType Type
        {
            get { return type; }
            set
            {
                type = value;
                RaisePropertyChanged(nameof(Type));
                RaisePropertyChanged(nameof(CompetitionName));
                Console.WriteLine(Type);
            }
        }

        private DateTime? date { get; set; }
        public DateTime? Date
        {
            get { return date; }
            set
            {
                date = value;
                RaisePropertyChanged(nameof(Date));
                RaisePropertyChanged(nameof(CompetitionName));
            }
        }

        private string location { get; set; }
        public string Location
        {
            get { return location; }
            set
            {
                location = value;
                RaisePropertyChanged(nameof(Location));
            }
        }

        public String RangeOfAge
        {
            get { return Competition.RangeOfAge; }
            set { }
        }

        private string competitionResult { get; set; }
        public string CompetitionResult
        {
            get
            {
                return competitionResult;
            }
            set
            {
                competitionResult = value;
                RaisePropertyChanged(nameof(CompetitionResult));
            }
        }

        public string SubscriptionText
        {
            get
            {
                if (!Member.HasCompetitionPass())
                {
                    return Properties.Resources.CompetitionDetailView_Subscription_Message_1;
                }
                else
                {
                    if (Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Competition)).IsValid())
                    {
                        return Properties.Resources.CompetitionDetailView_Subscription_Message_4;
                    }
                    else
                    {
                        if (Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Competition)).ExpirationDate < Competition.Date)
                        {
                            return Properties.Resources.CompetitionDetailView_Subscription_Message_2;
                        }
                        else
                        {
                            return Properties.Resources.CompetitionDetailView_Subscription_Message_3;
                        }
                    }
                };
            }
            set
            {
                RaisePropertyChanged(nameof(SubscriptionText));
            }
        }

        public string CompetitionName
        {
            get { return Competition.ToString(); }
            set
            {
                RaisePropertyChanged(nameof(CompetitionName));
            }
        }

        private Competition Basic { get; set; }

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }
        public ICommand Sub { get; set; }
        public ICommand Unsub { get; set; }

        private void CancelAction() {
            SetValues();
        }

        private void SaveAction() {
            if (Basic.Type != Type)
            {
                Competition.Type = Type;
                App.NotifyColleagues(AppMessages.MSG_COMPETITION_NAME_CHANGED,
                                     Competition.ToString());
            }
            if (Basic.Date != Date)
            {
                Competition.Date = Date;
                App.NotifyColleagues(AppMessages.MSG_COMPETITION_NAME_CHANGED,
                                     Competition.ToString());
            }
            if (Basic.Location.CompareTo(Location) != 0)
                Competition.Location = Location;
            if (Basic.CompetitionResult.MemberPseudo != CompetitionResult)
                Competition.CompetitionResult.MemberPseudo = CompetitionResult;

            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COMPETITION_CHANGED, Competition);
        }

        private bool CanSaveOrCancelAction() {
            if (Basic != null && Basic.CompetitionResult != null)
                return (Basic.Type != Type || Basic.Date != Date || Basic.Location.CompareTo(Location) != 0|| Basic.CompetitionResult.MemberPseudo != CompetitionResult);
            return false;
        }

        private void DeleteAction() 
        {
            App.Model.Competitions.Remove(Competition);
            Competition.Delete();
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COMPETITION_DELETED);
        }

        private bool CanDeleteAction() { return Member.Role == Role.Admin; }

        private void SubAction() {
            Competition modelCompetition = App.Model.Competitions.Find(Competition.CompetitionId);
            modelCompetition.AddParticipant(Member);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_PARTICIPANTS_COMPETITION_LIST_CHANGED);
        }

        private bool CanSubAction()
        {
            string[] str = Competition.RangeOfAge.Split('-');
            int minAge = int.Parse(str[0]), maxAge = int.Parse(str[1]);
            return (!Competition.Participants.Contains(Member) && Competition.Participants.Count() < MaxParticipants 
                && Member.Age >= minAge && Member.Age <= maxAge && (Member.Subscription != null && Member.HasCompetitionPass() 
                && !(Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Competition)).ExpirationDate < Competition.Date)
                && Competition.Date > DateTime.Today
                && (Member.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Competition)) ? Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Competition)).IsValid() : false))); 
        }

        private void UnsubAction()
        {
            Competition modelCompetition = App.Model.Competitions.Find(Competition.CompetitionId);
            modelCompetition.RemoveParticipant(Member);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_PARTICIPANTS_COMPETITION_LIST_CHANGED);
        }

        private bool CanUnsubAction() { return (Competition.Participants.Contains(Member)); }

        public bool IsAdmin { get => Member.Role == Role.Admin; }

        public bool IsNotAdmin { get => Member.Role != Role.Admin; }

        private void FillComboBoxType() {
            comboBoxType.Items.Clear();
            foreach (CompetitionType compettype in Enum.GetValues(typeof(CompetitionType)))
            {
                comboBoxType.Items.Add(compettype);
            }
        }

        private void FillComboBoxResult()
        {
            foreach (Member mbr in Competition.Participants)
            {
                comboBoxResult.Items.Add(mbr.Pseudo);
            }
        }

        private void FillComboBox() 
        {
            FillComboBoxType();
            FillComboBoxResult();
        }

        private void SetValues()
        {
            Basic = (from c in App.Model.Competitions
                     where c.CompetitionId == Competition.CompetitionId
                     select c).FirstOrDefault();
            Type = Competition.Type;
            Date = Competition.Date;
            Location = Competition.Location;
            CompetitionResult = Competition.CompetitionResult.MemberPseudo;
        }

        public CompetitionDetailView(Competition competition)
        {
            InitializeComponent();

            DataContext = this;
            Competition = competition;
            Member = App.CurrentUser;
            FillComboBox();
            SetValues();


            Save = new RelayCommand(SaveAction, CanSaveOrCancelAction);
            Cancel = new RelayCommand(CancelAction, CanSaveOrCancelAction);
            Delete = new RelayCommand(DeleteAction, CanDeleteAction);
            Sub = new RelayCommand(SubAction, CanSubAction);
            Unsub = new RelayCommand(UnsubAction, CanUnsubAction);
            App.Register(this, AppMessages.MSG_PARTICIPANTS_COMPETITION_LIST_CHANGED, () => Refresh());
            App.Register(this, AppMessages.MSG_SUBSCRIPTIONS_CHANGED, () => { RefreshSubButton(); });
            App.Register<Member>(this, AppMessages.MSG_MEMBER_DELETED, m => { Refresh(); });
            Refresh();
        }

        private void Refresh()
        {
            if (App.Model.Competitions.Cast<Competition>().FirstOrDefault(c => c.CompetitionId == Competition.CompetitionId) != null)
            {
                RaisePropertyChanged(nameof(NbrOfParticipants));
                Members = new ObservableCollection<Member>(App.Model.Competitions.Find(Competition.CompetitionId).Participants.OrderBy(m => m.Pseudo));
            }
        }

        private void RefreshSubButton()
        {
            SubscriptionText = "";
            Sub = new RelayCommand(SubAction, CanSubAction);
            Unsub = new RelayCommand(UnsubAction, CanUnsubAction);
        }
    }
}
