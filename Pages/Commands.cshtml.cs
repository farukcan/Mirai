using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mirai.Models;

namespace Mirai.Pages
{
    public class CommandsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IEnumerable<Command> Commands;

        public CommandsModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Commands = Mirai.Controls.Guideline.Commands;
        }
    }
}