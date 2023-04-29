using Mirai.Models;
using Mirai.Services;

namespace Mirai.Controls
{
    public static class Test
    {
        public static void Load(){
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Send Message")
                    .SetDescription("Sends message to user")
                    .SetFunction("SendMessage(message)")
                    .SetParameter("message" , "Message string")
                    .SetHandler(SendMessage)
                    .Build()
            );
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Sum two number")
                    .SetDescription("Sums two numbers")
                    .SetFunction("Sum(x,y)")
                    .SetParameter("x" , "first number")
                    .SetParameter("y" , "second number")
                    .SetHandler(Sum)
                    .Build()
            );
        }

        private static async Task Sum(Dialog dialog, string[] args)
        {
            if(Bot.Instance is null) return;
            try
            {
                int x = int.Parse(args[0]);
                int y = int.Parse(args[1]);
                await Bot.Instance.Answer(dialog, $"Sum is {x+y}");
            }
            catch (System.Exception)
            {
                await Bot.Instance.Answer(dialog, $"Error, cannot sum {string.Join(",",args)}");
            }
        }

        private static async Task SendMessage(Dialog dialog, string[] args)
        {
            if(Bot.Instance is null) return;
            await Bot.Instance.Answer(dialog, args[0]);
        }
    }
}