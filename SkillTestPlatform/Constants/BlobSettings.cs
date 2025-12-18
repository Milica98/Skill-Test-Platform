namespace SkillTestPlatform.Constants
{
    public static class BlobSettings
    {
        public const string Candidates = "candidates";
        public const string Skills = "skills";
        public const string Tasks = "tasks";

        public static string BlobName(string id) => $"{id}.json";
    }
}
