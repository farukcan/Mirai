using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mirai.Models;
using Mirai.Services;
using RethinkDb.Driver.Net;

namespace Mirai.Pages
{
    public class InformationsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IEnumerable<Information> Informations;
        private Rethink rethink;

        public InformationsModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Informations = new List<Information>();
            rethink = Rethink.Instance ?? throw new ArgumentNullException("Rethink is null");
        }

        public void OnGet()
        {
            Informations = rethink.Linq<Information>("Mirai","Informations")
                            .OrderByDescending(i=>i.Time)
                            .ToArray();
        }
    }
}