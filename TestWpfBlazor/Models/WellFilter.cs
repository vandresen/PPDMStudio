namespace PPDMStudio.Models
{
    public class WellFilter
    {
        public string? UWI { get; set; }
        public string? WellName { get; set; }
        public string? GovernmentId { get; set; }
        public string? Operator { get; set; }
        public string? AssignedField { get; set; }
        public string? CurrentStatus { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(WellName) &&
            string.IsNullOrWhiteSpace(GovernmentId) &&
            string.IsNullOrWhiteSpace(Operator) &&
            string.IsNullOrWhiteSpace(AssignedField) &&
            string.IsNullOrWhiteSpace(CurrentStatus) &&
            string.IsNullOrWhiteSpace(County) &&
            string.IsNullOrWhiteSpace(State) &&
            string.IsNullOrWhiteSpace(Country);

        public void Clear()
        {
            UWI = null;
            WellName = null;
            GovernmentId = null;
            Operator = null;
            AssignedField = null;
            CurrentStatus = null;
            County = null;
            State = null;
            Country = null;
        }
    }
}
