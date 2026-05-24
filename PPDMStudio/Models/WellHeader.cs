namespace PPDMStudio.Models
{
    public class WellHeader
    {
        public string UWI { get; set; } = string.Empty;
        public string? WELL_NAME { get; set; }
        public string? ASSIGNED_FIELD { get; set; }
        public string? OPERATOR { get; set; }
        public string? CURRENT_STATUS { get; set; }
        public string? PROFILE_TYPE { get; set; }
        public string? DEPTH_DATUM { get; set; }
        public decimal? KB_ELEV { get; set; }
        public decimal? GROUND_ELEV { get; set; }
        public decimal? FINAL_TD { get; set; }
        public decimal? LOG_TD { get; set; }
        public decimal? DRILL_TD { get; set; }
        public decimal? MAX_TVD { get; set; }
        public DateTime? SPUD_DATE { get; set; }
        public DateTime? FINAL_DRILL_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        public DateTime? ABANDONMENT_DATE { get; set; }
        public decimal? SURFACE_LATITUDE { get; set; }
        public decimal? SURFACE_LONGITUDE { get; set; }
        public decimal? BOTTOM_HOLE_LATITUDE { get; set; }
        public decimal? BOTTOM_HOLE_LONGITUDE { get; set; }
        public string? REMARK { get; set; }
        // From WELL_AREA
        public string? STATE { get; set; }
        public string? COUNTY { get; set; }
    }
}
