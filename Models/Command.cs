namespace Mirai.Models
{
    public class Command
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Function { get; set; }
        public Dictionary<string,string> Parameters { get; set; }
        public delegate Task CommandFunction(Dialog dialog, string[] args);
        public CommandFunction Handler { get; set; }
        public Command(string name, string description, string function, Dictionary<string,string> parameters)
        {
            Name = name;
            Description = description;
            Function = function;
            Parameters = parameters;
            Handler = EmptyHandler;
        }
        public static Factory Builder{
            get => new Factory();
        }
        Task EmptyHandler(Dialog dialog, string[] args){
            throw new NotImplementedException();
        }
        public class Factory{
            public Command command;
            public Factory(){
                command = new Command("","","",new Dictionary<string,string>());
            }
            public Factory SetName(string name){
                command.Name = name;
                return this;
            }
            public Factory SetDescription(string description){
                command.Description = description;
                return this;
            }
            public Factory SetFunction(string function){
                command.Function = function;
                return this;
            }
            public Factory SetParameter(string name, string description){
                command.Parameters[name] = description;
                return this;
            }
            public Factory SetHandler(CommandFunction handler){
                command.Handler = handler;
                return this;
            }
            public Command Build(){
                return command;
            }
        }
    }
}