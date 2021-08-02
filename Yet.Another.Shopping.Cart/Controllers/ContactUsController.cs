using System;
using Microsoft.AspNetCore.Mvc;
using Yet.Another.Shopping.Cart.Web.Models.ContactUsViewModels;
using Yet.Another.Shopping.Cart.Core.Domain.Messages;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Messages;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Yet.Another.Shopping.Cart.Web.Controllers
{
    public class ContactUsController : Controller
    {
        #region Fields

        private readonly IContactUsService _contactUsService;

        #endregion

        #region Constructor

        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        #endregion

        #region Methods

        // GET: /ContactUs/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMessage(ContactUsModel model)
        {
            bool err = true;
            if(ModelState.IsValid)
            {
                var messageEntity = new ContactUsMessage
                {
                    Name = model.Name,
                    Email = model.Email,
                    Title = model.Title,
                    Message = model.Message,
                    Read = false,
                    SendDate = DateTime.Now
                };

                _contactUsService.InsertMessage(messageEntity);
                err = false;
            }

            TempData["ContactUsErr"] = err;
            return RedirectToAction("Index");
        }

        #endregion
    }
}
