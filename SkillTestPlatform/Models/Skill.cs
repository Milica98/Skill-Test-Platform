namespace SkillTestPlatform.Models
{
    public class Skill
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }

        public Skill(SkillCreateModel model)
        {
            Name = model.Name;
        }

        public Skill() { }
    }
}
