namespace SkillTestPlatform.Models
{
    public class TaskItemCreateModel
    {
        public required string Title { get; set; }
        public required string Level { get; set; }
        public required string Description { get; set; }
        public required string Code { get; set; }
    }
}
