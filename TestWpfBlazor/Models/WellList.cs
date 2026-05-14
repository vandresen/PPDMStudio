namespace PPDMStudio.Models
{
    public class WellList
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string ModifiedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public int WellCount { get; set; }
    }
}
