using Yet.Another.Shopping.Cart.Infrastructure.Services.Catalog;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Yet.Another.Shopping.Cart.Web.ViewComponents
{
    [ViewComponent(Name = "Category")]
    public class CategoryViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IViewComponentResult Invoke()
        {
            return View(
                _categoryService.GetAllCategoriesWithoutParent().Where(x => x.Published)
            );
        }
    }
}
