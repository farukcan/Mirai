namespace Mirai.Models
{
    public class Prompt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Text { get; set; }
        public Prompt(string name, string text)
        {
            Name = name;
            Text = text;
        }
        public PromptUsage Use(){
            return new PromptUsage(this);
        }
        public class PromptUsage{
            public Prompt Prompt { get; set; }
            public string Text;
            public PromptUsage(Prompt prompt){
                Prompt = prompt;
                Text = prompt.Text;
            }
            public PromptUsage Set(string key,string value){
                // find [key] and set it to value
                Text = Text.Replace($"[{key}]", value);
                return this;
            }
            public string Get() => Text;
        }
    }
}