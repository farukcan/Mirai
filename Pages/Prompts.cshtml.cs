using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mirai.Pages
{
    public class PromptsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public PromptsModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}