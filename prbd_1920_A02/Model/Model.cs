using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.CompilerServices;
using PRBD_Framework;

namespace prbd_1920_A02 {

    public enum Role {
        Climber = 0, Professor = 1, Admin = 2
    }

    public enum CompetitionType
    {
        Block = 0, Swiftness = 1, Difficulty = 2
    }

    public enum SubscriptionType
    {
        DailyPass = 1, WeeklyPass = 7, MonthlyPass = 31, QuarterlyPass = 122, YearlyPass = 365
    }

    public enum PassType
    { 
        Pass = 0, Course = 1, Competition = 2
    }

    public class Model : DbContext {
        public Model() : base("prbd_1920_A02") {
            // la base de données est supprimée et recréée quand le modèle change
            Database.SetInitializer<Model>(
                new DropCreateDatabaseIfModelChanges<Model>()
            );
        }

        public DbSet<Member> Members { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<ForumThread> Threads { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Competition> Competitions { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<CompetitionResult> Competitionresult { get; set; }

        public virtual DbSet<Price> Prices { get; set; }

        public virtual DbSet<OpeningTime> OpeningTimes { get; set; }

        public Member CreateMember(string pseudo, string password, string firstName, string lastName, string Email, DateTime? birthDate, Role role = Role.Climber, string picturePath = null) {
            var member = Members.Create();
            member.Pseudo = pseudo;
            member.Password = password;
            member.Role = role;
            member.FirstName = firstName;
            member.LastName = lastName;
            member.Email = Email;
            member.BirthDate = birthDate;
            member.Age = DateTime.Now.Year - birthDate.Value.Year;
            member.PicturePath = picturePath;
            Members.Add(member);
            return member;
        }

        public Post CreatePost(Member author, ForumThread thread, string body, bool isParent = false)
        {
            var message = Posts.Create();
            message.Body = body;
            message.IsParent = isParent;
            // ici on établit les relations dans le sens N-1
            message.Author = author;
            message.Thread = thread;
            // ici on établit les relations dans le sens 1-N
            author.AuthorPosts.Add(message);
            thread.Posts.Add(message);
            // on ajoute le message au DbSet pour qu'il soit pris en compte par EF
            Posts.Add(message);
            SaveChanges();
            return message;
        }

        public ForumThread CreateThread(Member author, string body, string title, bool isParent = true)
        {
            int lastInserted = (from post in Posts orderby post.PostId descending select post.PostId).Count() == 0 ? 1 : (from post in Posts orderby post.PostId descending select post.PostId).First() + 1;
            var message = Posts.Create();
            var thread = Threads.Create();
            thread.Title = title;
            thread.ParentId = lastInserted;
            message.Body = body;
            message.IsParent = isParent;
            // ici on établit les relations dans le sens N-1
            message.Author = author;
            message.Thread = thread;
            // ici on établit les relations dans le sens 1-N
            author.AuthorPosts.Add(message);
            thread.Posts.Add(message);
            // on ajoute le message au DbSet pour qu'il soit pris en compte par EF
            Posts.Add(message);
            Threads.Add(thread);
            SaveChanges();
            return thread;
        }

        public Course CreateCourse(Member professor, int periodTime, DateTime? beginning, int maxParticipants, string location, string range)
        {
            var course = Courses.Create();
            course.PeriodTime = periodTime;
            course.Beginning = beginning;
            course.MaxParticipants = maxParticipants;
            course.Location = location;
            course.RangeOfAge = range;
            // ici on établit les relations dans le sens N-1
            course.Professor = professor;
            course.ProfessorPseudo = professor.Pseudo;
            Courses.Add(course);
            return course;
        }

        public Competition CreateCompetition(DateTime? beginning, int maxParticipants, string location, CompetitionType type, string range)
        {
            var competition = Competitions.Create();
            competition.Date = beginning;
            competition.MaxParticipants = maxParticipants;
            competition.Location = location;
            competition.Type = type;
            competition.RangeOfAge = range;
            competition.CompetitionResult = CreateCompetitionResult(competition.CompetitionId, "");
            Competitions.Add(competition);
            return competition;
        }

        // cette fonction ne sert qu'a initialiser les donnees
        private Price CreatePrice(PassType passType, SubscriptionType subscriptionType, string range, int cost)
        {
            var price = Prices.Create();
            price.PassType = passType;
            price.SubscriptionType = subscriptionType;
            price.RangeOfAge = range;
            price.Cost = cost;
            Prices.Add(price);
            return price;
        }

        private Subscription CreateSubscriptionToPass(Member member, DateTime? beginning, SubscriptionType subscriptionType)
        {
            //member.Sub(subscriptionType, beginning, price);
            return (CreateSubscription(member, PassType.Pass, beginning, subscriptionType));
        }

        private Subscription CreateSubscriptionToCourse(Member member, DateTime? beginning, SubscriptionType subscriptionType)
        {
            //member.SubToCourse(course);
            return (CreateSubscription(member, PassType.Course, beginning, subscriptionType));
        }

        private Subscription CreateSubscriptionToCompetition(Member member, DateTime? beginning, SubscriptionType subscriptionType)
        {
            //member.SubToCompetition(competition);
            return (CreateSubscription(member, PassType.Competition, beginning, subscriptionType));
        }

        public Subscription CreateSubscription(Member member, PassType passType, DateTime? beginning, SubscriptionType subscriptionType)
        {
            var subscription = Subscriptions.Create();
            subscription.PassType = passType;
            Price subscriptionCost = this.Prices.Where(
                p => p.SubscriptionType == subscriptionType && p.PassType == passType).ToList().First(p => p.IsInAgeRange(member.Age));
            if (subscriptionCost == null)
                Console.WriteLine("Price for this subscriptionType, passType and range of age doesn't exist");
            subscription.SubscriptionName = subscriptionCost.ToString();
            subscription.Beginning = beginning;
            subscription.Price = subscriptionCost;
            subscription.Member = member;
            subscription.Type = subscriptionType;
            Subscriptions.Add(subscription);
            member.Subscription.Add(subscription);
            return subscription;
        }

        public CompetitionResult CreateCompetitionResult(int CompetitionId, String MemberPseudo) {
            var competitionResult = Competitionresult.Create();
            competitionResult.CompetitionId = CompetitionId;
            competitionResult.MemberPseudo = MemberPseudo;
            Competitionresult.Add(competitionResult);
            return competitionResult;
        }

        public OpeningTime CreateOpeningTime(int dayOfWeek, int openingHour, int closureHour)
        {
            var openingTime = OpeningTimes.Create();
            openingTime.DayOFWeek = dayOfWeek;
            openingTime.OpeningHour = openingHour + "H";
            openingTime.ClosureHour = closureHour + "H";
            OpeningTimes.Add(openingTime);
            return openingTime;
        }

        public void SeedData() {
            if (Members.Count() == 0 && Prices.Count() == 0 && Competitions.Count() == 0 && Threads.Count() == 0 && Posts.Count() == 0) {

                Console.Write("Seeding Opening Hours... ");
                CreateOpeningTime(0, 7, 19);
                CreateOpeningTime(1, 8, 20);
                CreateOpeningTime(2, 8, 20);
                CreateOpeningTime(3, 8, 20);
                CreateOpeningTime(4, 8, 21);
                CreateOpeningTime(5, 13, 22);
                CreateOpeningTime(6, 13, 22);
                Console.WriteLine("ok");
                SaveChanges();

                Console.Write("Seeding members... ");
                var admin = CreateMember("admin", "admin", "", "", "admin@admin.be", new DateTime(1980, 01, 01), Role.Admin, "admin.jpg");
                var ben = CreateMember("ben", "ben", "Benoit", "Penelle", "ben@ben.be", new DateTime(1980, 01, 01), Role.Professor, "ben.jpg");
                var bruno = CreateMember("bruno", "bruno", "Bruno", "Lacroix", "bruno@bruno.be", new DateTime(1980, 01, 01), Role.Professor, "donald.jpg");
                var quentin = CreateMember("quentin", "quentin", "Quentin", "Locht", "quentin@quentin.be", new DateTime(1995, 01, 01), Role.Climber, "Quentin.jpg");
                var gautier = CreateMember("gautier", "gautier", "Gautier", "Kiss", "gautier@gautier.be", new DateTime(1996, 01, 01), Role.Climber, "Gautier.jpg");
                var guillaume = CreateMember("guillaume", "guillaume", "Guillaume", "Rigaux", "guillaume@guillaume.be", new DateTime(1997, 01, 01), Role.Climber, "Guillaume.jpg");
                Console.WriteLine("ok");
                SaveChanges();

                Console.Write("Seeding prices... ");
                // Prix pour les competitions
                Price comp8140 = CreatePrice(PassType.Competition, SubscriptionType.DailyPass, "08-14", 2);
                Price comp14180 = CreatePrice(PassType.Competition, SubscriptionType.DailyPass, "14-18", 5);
                Price comp18990 = CreatePrice(PassType.Competition, SubscriptionType.DailyPass, "18-99", 7);
                Price comp8141 = CreatePrice(PassType.Competition, SubscriptionType.WeeklyPass, "08-14", 8);
                Price comp14181 = CreatePrice(PassType.Competition, SubscriptionType.WeeklyPass, "14-18", 13);
                Price comp18991 = CreatePrice(PassType.Competition, SubscriptionType.WeeklyPass, "18-99", 15);
                Price comp8142 = CreatePrice(PassType.Competition, SubscriptionType.MonthlyPass, "08-14", 10);
                Price comp14182 = CreatePrice(PassType.Competition, SubscriptionType.MonthlyPass, "14-18", 20);
                Price comp18992 = CreatePrice(PassType.Competition, SubscriptionType.MonthlyPass, "18-99", 25);
                Price comp8143 = CreatePrice(PassType.Competition, SubscriptionType.QuarterlyPass, "08-14", 28);
                Price comp14183 = CreatePrice(PassType.Competition, SubscriptionType.QuarterlyPass, "14-18", 55);
                Price comp18993 = CreatePrice(PassType.Competition, SubscriptionType.QuarterlyPass, "18-99", 65);
                Price comp8144 = CreatePrice(PassType.Competition, SubscriptionType.YearlyPass, "08-14", 110);
                Price comp14184 = CreatePrice(PassType.Competition, SubscriptionType.YearlyPass, "14-18", 130);
                Price comp18994 = CreatePrice(PassType.Competition, SubscriptionType.YearlyPass, "18-99", 150);
                // Prix pour les abonnements des 0-10 ans
                Price sub0010Daily = CreatePrice(PassType.Pass, SubscriptionType.DailyPass, "0-10", 7);
                Price sub0010Weekly = CreatePrice(PassType.Pass, SubscriptionType.WeeklyPass, "0-10", 25);
                Price sub0010Monthly = CreatePrice(PassType.Pass, SubscriptionType.MonthlyPass, "0-10", 40);
                Price sub0010Quarterly = CreatePrice(PassType.Pass, SubscriptionType.QuarterlyPass, "0-10", 100);
                Price sub0010Yearly = CreatePrice(PassType.Pass, SubscriptionType.YearlyPass, "0-10", 250);
                // Prix pour les abonnements des 11-15 ans
                Price sub1115Daily = CreatePrice(PassType.Pass, SubscriptionType.DailyPass, "11-15", 9);
                Price sub1115Weekly = CreatePrice(PassType.Pass, SubscriptionType.WeeklyPass, "11-15", 30);
                Price sub1115Monthly = CreatePrice(PassType.Pass, SubscriptionType.MonthlyPass, "11-15", 51);
                Price sub1115Quarterly = CreatePrice(PassType.Pass, SubscriptionType.QuarterlyPass, "11-15", 128);
                Price sub1115Yearly = CreatePrice(PassType.Pass, SubscriptionType.YearlyPass, "11-15", 320);
                // Prix pour les abonnements des 16-21 ans
                Price sub1621Daily = CreatePrice(PassType.Pass, SubscriptionType.DailyPass, "16-21", 11);
                Price sub1621Weekly = CreatePrice(PassType.Pass, SubscriptionType.WeeklyPass, "16-21", 40);
                Price sub1621Monthly = CreatePrice(PassType.Pass, SubscriptionType.MonthlyPass, "16-21", 63);
                Price sub1621Quarterly = CreatePrice(PassType.Pass, SubscriptionType.QuarterlyPass, "16-21", 157);
                Price sub1621Yearly = CreatePrice(PassType.Pass, SubscriptionType.YearlyPass, "16-21", 393);
                // Prix pour les abonnements des 21-99 ans
                Price sub2199Daily = CreatePrice(PassType.Pass, SubscriptionType.DailyPass, "21-99", 15);
                Price sub2199Weekly = CreatePrice(PassType.Pass, SubscriptionType.WeeklyPass, "21-99", 50);
                Price sub2199Monthly = CreatePrice(PassType.Pass, SubscriptionType.MonthlyPass, "21-99", 85);
                Price sub2199Quarterly = CreatePrice(PassType.Pass, SubscriptionType.QuarterlyPass, "21-99", 214);
                Price sub2199Yearly = CreatePrice(PassType.Pass, SubscriptionType.YearlyPass, "21-99", 535);
                // Prix pour les cours
                Price priceCourses10120 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "10-12", 5);
                Price PriceCourses12140 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "12-14", 5);
                Price PriceCourses14160 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "14-16", 5);
                Price PriceCourses16180 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "16-18", 5);
                Price PriceCourses18200 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "18-20", 5);
                Price PriceCourses20990 = CreatePrice(PassType.Course, SubscriptionType.DailyPass, "20-99", 5);
                Price priceCourses10121 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "10-12", 7);
                Price PriceCourses12141 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "12-14", 8);
                Price PriceCourses14161 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "14-16", 9);
                Price PriceCourses16181 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "16-18", 11);
                Price PriceCourses18201 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "18-20", 12);
                Price PriceCourses20991 = CreatePrice(PassType.Course, SubscriptionType.WeeklyPass, "20-99", 15); 
                Price priceCourses10122 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "10-12", 18);
                Price PriceCourses12142 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "12-14", 22);
                Price PriceCourses14162 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "14-16", 24);
                Price PriceCourses16182 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "16-18", 28);
                Price PriceCourses18202 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "18-20", 30);
                Price PriceCourses20992 = CreatePrice(PassType.Course, SubscriptionType.MonthlyPass, "20-99", 36);
                Price priceCourses10123 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "10-12", 45);
                Price PriceCourses12143 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "12-14", 53);
                Price PriceCourses14163 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "14-16", 60);
                Price PriceCourses16183 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "16-18", 68);
                Price PriceCourses18203 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "18-20", 75);
                Price PriceCourses20993 = CreatePrice(PassType.Course, SubscriptionType.QuarterlyPass, "20-99", 90);
                Price priceCourses10124 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "10-12", 150);
                Price PriceCourses12144 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "12-14", 175);
                Price PriceCourses14164 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "14-16", 200);
                Price PriceCourses16184 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "16-18", 225);
                Price PriceCourses18204 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "18-20", 250);
                Price PriceCourses20994 = CreatePrice(PassType.Course, SubscriptionType.YearlyPass, "20-99", 300);
                Console.WriteLine("ok");
                SaveChanges();

                Console.Write("Seeding competitions... ");

                var compet1 = CreateCompetition(new DateTime(2020, 02, 22, 10, 00, 00), 20, "Grimp'EPFC", CompetitionType.Swiftness, "18-99");
                compet1.DeclareWinner(guillaume);
                var compet2 = CreateCompetition(new DateTime(2020, 02, 23, 10, 00, 00), 20, "Grimp'EPFC", CompetitionType.Swiftness, "14-18");
                var compet3 = CreateCompetition(new DateTime(2020, 03, 21, 10, 00, 00), 15, "Grimp'EPFC", CompetitionType.Block, "14-18");
                var compet4 = CreateCompetition(new DateTime(2020, 03, 22, 10, 00, 00), 10, "Grimp'EPFC", CompetitionType.Difficulty, "08-14");
                var compet5 = CreateCompetition(new DateTime(2020, 04, 18, 10, 00, 00), 20, "Grimp'EPFC", CompetitionType.Swiftness, "08-14");
                var compet6 = CreateCompetition(new DateTime(2020, 04, 19, 10, 00, 00), 10, "Grimp'EPFC", CompetitionType.Difficulty, "18-99");
                var compet7 = CreateCompetition(new DateTime(2020, 05, 30, 10, 00, 00), 15, "Grimp'EPFC", CompetitionType.Block, "18-99");
                var compet8 = CreateCompetition(new DateTime(2020, 05, 31, 10, 00, 00), 15, "Grimp'EPFC", CompetitionType.Block, "08-14");
                var compet9 = CreateCompetition(new DateTime(2020, 06, 21, 10, 00, 00), 10, "Grimp'EPFC", CompetitionType.Difficulty, "14-18");
                Console.WriteLine("ok");
                SaveChanges();

                Console.Write("Seeding courses... ");
                // Cours pour les 10-12 ans
                CreateCourse(ben, 1, new DateTime(2020, 09, 02, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                CreateCourse(ben, 1, new DateTime(2020, 09, 04, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                CreateCourse(ben, 1, new DateTime(2020, 09, 06, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 12, 00, 00), 15, "Grimp'EPFC", "10-12");
                // Cours pour les 12-14 ans
                CreateCourse(ben, 1, new DateTime(2020, 09, 02, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                CreateCourse(ben, 1, new DateTime(2020, 09, 04, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                CreateCourse(ben, 1, new DateTime(2020, 09, 06, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 13, 00, 00), 15, "Grimp'EPFC", "12-14");
                // Cours pour les 14-16 ans
                CreateCourse(ben, 1, new DateTime(2020, 09, 02, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                CreateCourse(ben, 1, new DateTime(2020, 09, 04, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                CreateCourse(ben, 1, new DateTime(2020, 09, 06, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 14, 00, 00), 15, "Grimp'EPFC", "14-16");
                // Cours pour les 16-18 ans
                CreateCourse(ben, 1, new DateTime(2020, 09, 02, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                CreateCourse(ben, 1, new DateTime(2020, 09, 04, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                CreateCourse(ben, 1, new DateTime(2020, 09, 06, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 15, 00, 00), 15, "Grimp'EPFC", "16-18");
                // Cours pour les 18-20 ans
                CreateCourse(ben, 1, new DateTime(2020, 09, 02, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                CreateCourse(ben, 1, new DateTime(2020, 09, 04, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                CreateCourse(ben, 1, new DateTime(2020, 09, 06, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 16, 00, 00), 15, "Grimp'EPFC", "18-20");
                // Cours pour les adultes
                var course1 = CreateCourse(ben, 1, new DateTime(2020, 09, 02, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                var course2 = CreateCourse(bruno, 1, new DateTime(2020, 09, 03, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                var course3 = CreateCourse(ben, 1, new DateTime(2020, 09, 04, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                var course4 = CreateCourse(bruno, 1, new DateTime(2020, 09, 05, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                var course5 = CreateCourse(ben, 1, new DateTime(2020, 09, 06, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                var course6 = CreateCourse(bruno, 1, new DateTime(2020, 09, 07, 17, 00, 00), 15, "Grimp'EPFC", "20-99");
                Console.WriteLine("ok");
                SaveChanges();

                // Subscriptions pour des cours
                CreateSubscriptionToCourse(guillaume, new DateTime(2020, 05, 22), SubscriptionType.YearlyPass);
                CreateSubscriptionToCourse(gautier, new DateTime(2020, 05, 22), SubscriptionType.YearlyPass);
                CreateSubscriptionToCourse(quentin, new DateTime(2020, 05, 22), SubscriptionType.YearlyPass);
                SaveChanges();

                guillaume.SubToCourse(course1);
                guillaume.SubToCourse(course2);
                gautier.SubToCourse(course3);
                gautier.SubToCourse(course4);
                quentin.SubToCourse(course5);
                quentin.SubToCourse(course6);
                quentin.SubToCourse(course1);

                // Subscriptions pour des competitions
                CreateSubscriptionToCompetition(guillaume, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                CreateSubscriptionToCompetition(ben, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                CreateSubscriptionToCompetition(bruno, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                CreateSubscriptionToCompetition(gautier, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                CreateSubscriptionToCompetition(quentin, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                SaveChanges();

                guillaume.SubToCompetition(compet1);
                guillaume.SubToCompetition(compet2);
                quentin.SubToCompetition(compet3);
                quentin.SubToCompetition(compet8);
                gautier.SubToCompetition(compet3);
                bruno.SubToCompetition(compet1);
                ben.SubToCompetition(compet1);

                // Subscriptions pour des pass
                CreateSubscriptionToPass(guillaume, new DateTime(2020, 05, 22), SubscriptionType.MonthlyPass);
                CreateSubscriptionToPass(quentin, new DateTime(2020, 05, 22), SubscriptionType.WeeklyPass);
                CreateSubscriptionToPass(gautier, new DateTime(2020, 05, 22), SubscriptionType.QuarterlyPass);
                CreateSubscriptionToPass(ben, new DateTime(2020, 05, 22), SubscriptionType.YearlyPass);
                SaveChanges();

                Console.Write("Seeding threads & posts... ");
                var thread1 = CreateThread(guillaume, "Hello everyone, I'm looking forward to buy some new material buy I don't really know what brands are the best. Could you help me ?",
                    "Do you know where I could buy some material ?");
                CreatePost(ben, thread1, "Actually, Decathlon has strong material and not that expensive");
                CreatePost(quentin, thread1, "I've bought my whole material at Decathlon, and I'm not disappointed at all");
                CreatePost(gautier, thread1, "Here's a list of brands for professionnals : Beal, Black Diamond, Camp, Climbing Technology, Dreamtime, Mammut, Petzl, Wild Country.");
                CreatePost(guillaume, thread1, "Thanks everyone for your answers!");

                var thread2 = CreateThread(admin, "You can directly sub in the competition section", "Grimp'EPFC is organizing the climb rock competition of the year");
                CreatePost(gautier, thread2, "I'm already subbed hehe");
                CreatePost(guillaume, thread2, "I'M IN !!!");
                CreatePost(quentin, thread2, "Well, one more competition to win");

                var thread3 = CreateThread(ben, "Due to the new season, all subscriptions will be deleted, you can sub to the course by clicking on it directly in the courses section",
                    "Reminder : members of the annual courses must re-sub");
                CreatePost(admin, thread3, "And a reminder : Prices have changed !!!");
                CreatePost(guillaume, thread3, "Well, prices have changed but ok...");
                CreatePost(quentin, thread3, "Wow... that's so expensive");
                CreatePost(gautier, thread3, "Come on guys, it's still affordable");

                var thread4 = CreateThread(admin, "The prices have changed, check the prices section !", "New annual pass price rating");
                CreatePost(guillaume, thread4, "WTF???????");
                CreatePost(gautier, thread4, "Ok, I'm leaving");
                CreatePost(quentin, thread4, "How can you do that?");
                CreatePost(admin, thread4, "Due to your reaction, we'll stick to the past season's rate. Sorry for that!");

                var thread5 = CreateThread(bruno, "I'm sorry, I'm sick and I have to rest a bit", "Friday's course is canceled");
                CreatePost(guillaume, thread5, "Get well soon");
                CreatePost(gautier, thread5, "It's ok, I didn't want to come");
                CreatePost(quentin, thread5, "gngng krkrkr");
                CreatePost(admin, thread5, "Get well soon Bruno!2");
                SaveChanges();
                Console.WriteLine("ok");

            }
        }

        // Force le model à créer une relation many to many entre course et member
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Course>()
                        .HasMany<Member>(s => s.Participants)
                        .WithMany(c => c.Follows)
                        .Map(cs =>
                        {
                            cs.MapLeftKey("CourseId");
                            cs.MapRightKey("ParticipantId");
                            cs.ToTable("MemberCourse");
                        });

        }

    }
}

