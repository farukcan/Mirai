using Telegram.Bot.Types;

namespace Mirai.Models
{
    public class Dialog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Interlocutor { get; set; }
        public Message Message { get; set; }
        public enum State { Waiting, Analysis, Question, Order, Information, Answered, NoAnswer }
        public State Status;
        public Dictionary<string,object> Results { get; set; } = new();
        public string Answer { get; set; } = string.Empty;
        public bool IsQuestion { get; set; } = false;
        public bool IsInformation { get; set; } = false;
        public bool IsOrder { get; set; } = false;
        public Dialog(Message message, string interlocutor)
        {
            Message = message;
            Status = State.Waiting;
            Interlocutor = interlocutor;
        }
    }
}