namespace Mirai.Models
{
    public class Prompt
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Prompt(string name, string text)
        {
            Name = name;
            Text = text;
        }
    }
}