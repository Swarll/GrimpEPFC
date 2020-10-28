using PRBD_Framework;
using prbd_1920_A02.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace prbd_1920_A02 {
    public enum AppMessages {
        MSG_DISPLAY_MEMBER, MSG_DISPLAY_MEMBERS, MSG_PSEUDO_CHANGED, MSG_MEMBER_CHANGED, MSG_MEMBER_DELETED,
        MSG_DISPLAY_COURSES, MSG_COURSE_CHANGED, MSG_COURSE_DELETED, MSG_DISPLAY_COURSE, MSG_PARTICPANTS_LIST_CHANGED,
        MSG_DISPLAY_COMPETITIONS, MSG_COMPETITION_CHANGED, MSG_COMPETITION_DELETED, MSG_DISPLAY_COMPETITION,
        MSG_COURSE_NAME_CHANGED, MSG_PARTICIPANTS_COMPETITION_LIST_CHANGED, MSG_PARTICIPANTS_LIST_CHANGED,
        MSG_COMPETITION_NAME_CHANGED, MSG_DISPLAY_NEW_COMPETITION, MSG_CLOSE_TAB, MSG_DISPLAY_NEW_COURSE,
        MSG_DISPLAY_SUBSCRIPTIONS, MSG_SELECTEDPASS_CHANGED, MSG_DISPLAY_BUY_SUBSCRIPTION, MSG_SUBSCRIPTIONS_CHANGED
    }

    public partial class App : ApplicationBase {
        public static Model Model { get; private set; } = new Model();

        public static readonly string IMAGE_PATH =
            Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../images");
        public static Member CurrentUser { get; set; }

        public static void CancelChanges() {
            Model.Dispose(); // Retire de la mémoire le modèle actuel
            Model = new Model(); // Recréation d'un nouveau modèle à partir de la DB
        }
        public App() {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Culture);
            ColdStart();
        }

        private void ColdStart() {
            Model.SeedData();
        }
    }
}
