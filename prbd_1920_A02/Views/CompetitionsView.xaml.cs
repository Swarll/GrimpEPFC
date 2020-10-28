using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Logique d'interaction pour CompetitionsView.xaml
    /// </summary>
    public partial class CompetitionsView : UserControlBase
    {
        public Member Member { get; set; }

        public bool IsAdmin { get => Member.Role == Role.Admin; }

        private ObservableCollection<Competition> competitions;
        public ObservableCollection<Competition> Competitions { get => competitions; set => SetProperty(ref competitions, value, ()=> RaisePropertyChanged(nameof(CompetitionsListView))); }

        public ICommand DisplayCompetitionDetails { get; set; }
        public ICommand DisplayNewCompetition { get; set; }

        private CollectionView competitionsListView = null;
        public CollectionView CompetitionsListView
        {
            get
            {
                competitionsListView = (CollectionView)CollectionViewSource.GetDefaultView(competitions);
                if (competitionsListView != null && competitionsListView.SortDescriptions.Count == 0)
                    competitionsListView.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
                return competitionsListView;
            }
        }

        public CompetitionsView()
        {
            InitializeComponent();
            DataContext = this;
            Member = App.CurrentUser;

            DisplayCompetitionDetails = new RelayCommand<Competition>(c => {
                App.NotifyColleagues(AppMessages.MSG_DISPLAY_COMPETITION, c);
            });
            DisplayNewCompetition = new RelayCommand<Competition>(c => {
                App.NotifyColleagues(AppMessages.MSG_DISPLAY_NEW_COMPETITION, c);
            });

            App.Register<Competition>(this, AppMessages.MSG_COMPETITION_CHANGED,
                         competition => { Refresh(); });

            App.Register(this, AppMessages.MSG_COMPETITION_DELETED, () => { Refresh(); });
            App.Register(this, (AppMessages.MSG_PARTICIPANTS_COMPETITION_LIST_CHANGED), () => { Refresh(); });

            Refresh();
        }

        private void Refresh()
        {
            Competitions = new ObservableCollection<Competition>(App.Model.Competitions.OrderBy(c => c.CompetitionId));
        }
    }
}
