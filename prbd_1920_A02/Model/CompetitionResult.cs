using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class CompetitionResult : EntityBase<Model>
    {
        [Key]
        public int CompetitionId { get; set; }
        public string MemberPseudo { get; set; }

    }
}
