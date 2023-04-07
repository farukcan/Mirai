using Mirai.Models;

namespace Mirai.Commands
{
    public static class Repository
    {
        // singleton
        public static List<Command> Commands { get; set; } = new List<Command>();
        public static void Load(){
            Test.Load();
        }
    }
}