using Mirai.Models;

namespace Mirai.Controls
{
    public static class Guideline
    {
        // singleton
        public static List<Command> Commands { get; set; } = new List<Command>();
        public static void Load(){
            Test.Load();
            RssBot.Load();
        }
    }
}