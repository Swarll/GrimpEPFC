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
    /// Logique d'interaction pour CoursesView.xaml
    /// </summary>
    public partial class CoursesView : UserControlBase
    {
        public Member Member { get; set; }

        private ObservableCollection<Course> courses;
        public ObservableCollection<Course> Courses { get => courses; set
            {
                SetProperty(ref courses, value);
                RaisePropertyChanged(nameof(CoursesListView));
            }
        }

        public ICommand DisplayCourseDetails { get; set; }

        public ICommand DisplayNewCourse { get; set; }

        public bool HasEditRights
        {
            get { return App.CurrentUser.Role == Role.Professor || App.CurrentUser.Role == Role.Admin; }
        }

        private CollectionView coursesListView = null;
        public CollectionView CoursesListView
        {
            get
            {
                coursesListView = (CollectionView)CollectionViewSource.GetDefaultView(Courses);
                if (coursesListView != null && coursesListView.SortDescriptions.Count == 0)
                    coursesListView.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
                return coursesListView;
            }
        }

        private Course Course { get; set; }

        public ICollection<int> ParticipantsNbr
        {
            get
            {
                int[] participants = new int[20];
                for (int i = 0; i < 20; i++)
                {
                    participants[i] = i + 1;
                }
                return participants;
            }
            set { }
        }

        public int ParticipantsCount
        {
            get { return Course.Participants.Count(); }
            set
            {
                Course.MaxParticipants = value;
                RaisePropertyChanged(nameof(ParticipantsCount));
            }
        }

        public ICollection<string> Ranges
        {
            get
            {
                string[] ranges = { "10-12", "12-14", "14-16", "16-18", "18-20", "20-99" };
                return ranges;
            }
            set { }
        }

        public string RangeOfAge
        {
            get { return Course.RangeOfAge; }
            set
            {
                Course.RangeOfAge = value;
                RaisePropertyChanged(nameof(RangeOfAge));
            }
        }

        public void CreateNewCourse()
        {
            var professor = (from m in App.Model.Members
                             where m.Role == Role.Professor
                             select m).FirstOrDefault();
            var course = App.Model.CreateCourse(professor, 1, new DateTime(2020, 09, 02, 12, 00, 00), ParticipantsCount, "", RangeOfAge);
            App.NotifyColleagues(AppMessages.MSG_DISPLAY_NEW_COURSE, course);
        }

        public CoursesView()
        {
            InitializeComponent();
            DataContext = this;

            var professor = (from m in App.Model.Members
                             where m.Role == Role.Professor
                             select m).FirstOrDefault();
            Course = App.Model.CreateCourse(professor, 1, new DateTime(2020, 09, 02, 12, 00, 00), 15, "", "10-12");

            DisplayNewCourse = new RelayCommand(CreateNewCourse);

            DisplayCourseDetails = new RelayCommand<Course>(c => {
                App.NotifyColleagues(AppMessages.MSG_DISPLAY_COURSE, c);
            });

            App.Register<Course>(this, AppMessages.MSG_COURSE_CHANGED,
                        c => { Refresh(); });
            App.Register<ICollection<Course>>(this, AppMessages.MSG_COURSE_DELETED, c => { Refresh(); });

            App.Register<Member>(this, AppMessages.MSG_MEMBER_DELETED, m => { Refresh(); });

            App.Register(this, (AppMessages.MSG_PARTICIPANTS_LIST_CHANGED), () => { Refresh(); });

            Refresh();
        }

        private void Refresh()
        {
            Courses = new ObservableCollection<Course>(App.Model.Courses.OrderBy(c => c.CourseId));
        }
    }
}
