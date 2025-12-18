using SkillTestPlatform.Constants;

namespace SkillTestPlatform.Models
{
    public class Candidate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Seniority { get; set; }

        public Candidate(CandidateCreateModel model)
        {
            Name = model.Name;
            Surname = model.Surname;
            PhoneNumber = model.PhoneNumber;
            Seniority = model.Seniority;
        }

        public Candidate() { }
    }
}
