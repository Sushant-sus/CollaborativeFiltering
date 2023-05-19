using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OnlineShop.Views.Shared.Components._menuViewComponent
{
	public class _menuViewComponent : ViewComponent
    {
        private readonly ILogger _logger;

        public _menuViewComponent(ILogger logger)
        {

            _logger = logger;

        }
        public async Task<IViewComponentResult> InvokeAsync()
		{
            return View();
		}

    }
}
