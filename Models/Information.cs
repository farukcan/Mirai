namespace Mirai.Models
{
    public class Information
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public string Source { get; set; }
        public string Data { get; set; }
        public DateTime Time { get; set; }
        public List<string> Tags { get; set; } = new();
        public Information(string type, string source, string data)
        {
            Type = type;
            Source = source;
            Data = data;
            Time = DateTime.UtcNow;
        }
        public Information AddTag(string tag){
            Tags.Add(tag);
            return this;
        }
        public Information AddTags(IEnumerable<string> tags){
            Tags.AddRange(tags);
            return this;
        }
        public static Information FromUser(string user, string data){
            return new Information("UserInformation",user,data).AddTags(new []{"user",user});
        }
    }
}