using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPDMStudio.Models
{
    public class Well
    {
        public string UWI { get; set; } = string.Empty;
        public string? WELL_NAME { get; set; }
        public string? WELL_GOVERNMENT_ID { get; set; }
        public string? OPERATOR { get; set; }
        public string? ASSIGNED_FIELD { get; set; }
        public string? CURRENT_STATUS { get; set; }
        public string? PROFILE_TYPE { get; set; }
        public DateTime? SPUD_DATE { get; set; }
        public decimal? SURFACE_LATITUDE { get; set; }
        public decimal? SURFACE_LONGITUDE { get; set; }
        // From WELL_LOCATION
        public string? COUNTY { get; set; }
        public string? COUNTY_NAME { get; set; }
        public string? STATE { get; set; }
    }
}
