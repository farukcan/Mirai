using Microsoft.AspNetCore.Mvc.RazorPages;
using Mirai.Models;
using Mirai.Services;

namespace Mirai.Pages
{
    public class DialogsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IEnumerable<Dialog> Dialogs;
        private Rethink rethink;

        public DialogsModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Dialogs = new List<Dialog>();
            rethink = Rethink.Instance ?? throw new ArgumentNullException("Rethink is null");
        }

        public void OnGet()
        {
            Dialogs = rethink.Linq<Dialog>("Mirai","Dialogs")
                            .OrderByDescending(i=>i.UpdatedAt)
                            .ToArray();
        }
    }
}