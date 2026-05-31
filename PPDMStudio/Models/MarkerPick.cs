using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPDMStudio.Models
{
    public class MarkerPick
    {
        public required string UWI { get; set; }
        public required string STRAT_NAME_SET_ID { get; set; }
        public required string STRAT_UNIT_ID { get; set; }
        public required string INTERP_ID { get; set; }
        public double? PICK_DEPTH { get; set; }
        public double? PICK_TVD { get; set; }
        public DateOnly? PICK_DATE { get; set; }
        public string? DOMINANT_LITHOLOGY { get; set; }
        public string? REMARK { get; set; }
    }
}
