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
    /// Logique d'interaction pour Index.xaml
    /// </summary>
    public partial class IndexView : UserControlBase
    {

        private ObservableCollection<OpeningTime> openingTimes;
        public ObservableCollection<OpeningTime> OpeningTimes
        {
            get => openingTimes; set
            {
                SetProperty(ref openingTimes, value);
                RaisePropertyChanged(nameof(OpeningTimesListView));
            }
        }

        private CollectionView openingTimesListView = null;
        public CollectionView OpeningTimesListView
        {
            get
            {
                openingTimesListView = (CollectionView)CollectionViewSource.GetDefaultView(OpeningTimes);
                if (openingTimesListView != null && openingTimesListView.SortDescriptions.Count == 0)
                    openingTimesListView.SortDescriptions.Add(new SortDescription("int", ListSortDirection.Descending));
                return openingTimesListView;
            }
        }

        public IndexView()
        {
            InitializeComponent();
            DataContext = this;
            OpeningTimes = new ObservableCollection<OpeningTime>(App.Model.OpeningTimes.OrderBy(o => o.DayOFWeek));
        }
    }
}
