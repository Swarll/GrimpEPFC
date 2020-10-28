using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PRBD_Framework;
using System.Text.RegularExpressions;

namespace prbd_1920_A02
{
    /// <summary>
    /// Logique d'interaction pour CompetitionDetailView.xaml
    /// </summary>
    public partial class CompetitionCreateView : UserControlBase
    {
        public Member Member { get; set; }
        public Competition Competition { get; set; }

        public int MaxParticipants
        {
            get { return Competition.MaxParticipants; }
            set
            {
                Competition.MaxParticipants = value;
                RaisePropertyChanged(nameof(MaxParticipants));
            }
        }

        public CompetitionType Type
        {
            get { return Competition.Type; }
            set
            {
                Competition.Type = value;
                RaisePropertyChanged(nameof(Type));
                RaisePropertyChanged(nameof(CompetitionName));
                App.NotifyColleagues(AppMessages.MSG_COMPETITION_NAME_CHANGED,
                                     Competition.ToString());
            }
        }

        public DateTime? Date
        {
            get { return Competition.Date; }
            set
            {
                Competition.Date = value;
                RaisePropertyChanged(nameof(Date));
                RaisePropertyChanged(nameof(CompetitionName));
                App.NotifyColleagues(AppMessages.MSG_COMPETITION_NAME_CHANGED,
                                     Competition.ToString());
            }
        }

        public String Location
        {
            get { return Competition.Location; }
            set
            {
                Competition.Location = value;
                RaisePropertyChanged(nameof(Location));
            }
        }

        public String RangeOfAge
        {
            get { return Competition.RangeOfAge; }
            set
            {
                Competition.RangeOfAge = value;
                RaisePropertyChanged(nameof(RangeOfAge));
            }
        }

        public String CompetitionName
        {
            get { return Competition.ToString(); }
            set
            {
                RaisePropertyChanged(nameof(CompetitionName));
            }
        }

        public ICommand Create { get; set; }
        public ICommand Cancel { get; set; }

        private void CancelAction()
        {
            App.Model.Competitions.Remove(Competition);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COMPETITION_DELETED);
        }

        private void CreateAction()
        {
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COMPETITION_CHANGED, Competition);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB);
        }

        private bool CanCreateAction()
        {
            var change = (from c in App.Model.ChangeTracker.Entries<Competition>()
                          where c.Entity == Competition
                          select c).FirstOrDefault();
            return change != null && change.State != EntityState.Unchanged && Member.Role == Role.Admin
                && (Competition.Date != null && Competition.Date >= DateTime.Today && Competition.Location != "" && Competition.MaxParticipants != 0);
        }

        private bool CanCancelAction()
        {
            return true;
        }

        public bool IsAdmin { get => Member.Role == Role.Admin; }

        public void FillComboBoxType()
        {
            comboBoxType.Items.Clear();
            foreach (CompetitionType compettype in Enum.GetValues(typeof(CompetitionType)))
            {
                comboBoxType.Items.Add(compettype);
            }
        }

        public void FillComboBoxRangeOfAge()
        {
            var query = (from c in App.Model.Competitions
                         group c by new { c.RangeOfAge }
                          into roa
                         select roa.FirstOrDefault()).Distinct();
            var result = query.ToList();
            foreach (Competition res in result)
            {
                comboBoxRangeOfAge.Items.Add(res.RangeOfAge);
            }
        }

        public void FillComboBox()
        {
            FillComboBoxType();
            FillComboBoxRangeOfAge();
        }

        public CompetitionCreateView()
        {
            InitializeComponent();

            DataContext = this;
            Competition = App.Model.CreateCompetition(null, 0, "", CompetitionType.Block, "08-14");
            Member = App.CurrentUser;
            FillComboBox();


            Create = new RelayCommand(CreateAction, CanCreateAction);
            Cancel = new RelayCommand(CancelAction, CanCancelAction);
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
