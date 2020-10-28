using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class Competition : EntityBase<Model>
    {
        [Key]
        public int CompetitionId { get; set; }
        public int MaxParticipants { get; set; }
        public int NbrOfParticipants 
        {
            get { return this.Participants.Count(); }
            set { }
        }
        public string Location { get; set; }
        public CompetitionType Type { get; set; }
        public DateTime? Date { get; set; }
        // au format E.g : "15-25"
        public String RangeOfAge { get; set; }
        public virtual ICollection<Member> Participants { get; set; } = new HashSet<Member>();
        public virtual CompetitionResult CompetitionResult { get; set; }

        public bool AddParticipant(Member participant)
        {
            string[] str = RangeOfAge.Split('-');
            int minAge = int.Parse(str[0]), maxAge = int.Parse(str[1]);
            if (Participants.Count() < MaxParticipants && participant.Age >= minAge && participant.Age <= maxAge && participant.HasCompetitionPass() && (participant.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Competition)) ? participant.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Competition)).IsValid() : false))
            {
                Participants.Add(participant);
                participant.Competitions.Add(this);
                return true;
            }else{
                return false;
            }
        }

        public bool RemoveParticipant(Member participant)
        {
            if (Participants.Contains(participant))
            {
                Participants.Remove(participant);
                participant.Competitions.Remove(this);
                return true;
            }else{
                return false;
            }
        }

        public bool DeclareWinner(Member participant)
        {
            if (Participants.Contains(participant))
            {
                var competitionResult = this.CompetitionResult;
                competitionResult.MemberPseudo = participant.Pseudo;
                // Rajoute la compétition gagnée à la liste des compétitions remportées du participant
                participant.CompetitionsWon.Add(competitionResult);
                // Rajoute les résultats de la compétition à la compétition
                CompetitionResult = competitionResult;
                // Insère les résultats de la compétition en Db
                Model.Competitionresult.Create();
                Model.Competitionresult.Add(competitionResult);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Delete()
        {
            // Supprime la compétition de la liste des participants
            foreach (Member member in Participants)
            {
                member.Competitions.Remove(this);
            }
            // Supprime les participants de la compétition
            Participants.Clear();
            // Supprime le lien avec Competitionresult
            this.CompetitionResult = null;
            // Supprime la compétition elle-même
            Model.Competitions.Remove(this);
        }

        public override String ToString()
        {
            return (this.Type + " - " + this.Date);
        }

        public Member GetWinner() 
        {
            var MemberPseudo = this.CompetitionResult.MemberPseudo;
            return Member.GetMemberByPseudo(MemberPseudo);
        }
    }
}
