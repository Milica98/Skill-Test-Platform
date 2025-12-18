namespace SkillTestPlatform.Models
{
    public class TaskItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Title { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }

        public TaskItem(TaskItemCreateModel model)
        {
            Title = model.Title;
            Level = model.Level;
            Description = model.Description;
            Code = model.Code;
        }

        public TaskItem() { }
    }
}
