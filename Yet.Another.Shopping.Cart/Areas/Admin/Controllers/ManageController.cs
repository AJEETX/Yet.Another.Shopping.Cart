using Yet.Another.Shopping.Cart.Core.Domain.Messages;
using Yet.Another.Shopping.Cart.Core.Domain.User;
using Yet.Another.Shopping.Cart.Infrastructure.EFModels;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Messages;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Sale;
using Yet.Another.Shopping.Cart.Infrastructure.Services.User;
using Yet.Another.Shopping.Cart.Web.Areas.Admin.Controllers;
using Yet.Another.Shopping.Cart.Web.Areas.Admin.Models.Support;
using Yet.Another.Shopping.Cart.Web.Models;
using Yet.Another.Shopping.Cart.Web.Models.ManageViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yet.Another.Shopping.Cart.Areas.Admin.Controllers
{
    [Authorize]
    public class ManageController : AdminController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBillingAddressService _billingAddressService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContactUsService _contactUsService;

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IBillingAddressService billingAddressService,
            IHttpContextAccessor httpContextAccessor,
            IOrderService orderService,
            ILoggerFactory loggerFactory,
            IContactUsService contactUsService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _billingAddressService = billingAddressService;
            _httpContextAccessor = httpContextAccessor;
            _orderService = orderService;
            _contactUsService = contactUsService;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Users()
        {
            var users = _billingAddressService.GetBillingAddresses();
            var usersModel = users.Select(b => new BillingAddressModel
            {
                FirstName = b.FirstName,
                LastName = b.LastName,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                ZipPostalCode = b.ZipPostalCode,
                StateProvince = b.StateProvince,
                Email = b.Email,
                Telephone = b.Telephone
            });
            return View(usersModel.ToList());
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }


        // GET: /Manage/EditBillingAddress
        [HttpGet]
        public async Task<IActionResult> EditBillingAddress()
        {
            if (Session.GetString("BillingAddress") != null)
            {
                var userBilling = JsonConvert.DeserializeObject<BillingAddress>(Session.GetString("BillingAddress"));
                var userBillingModel = _mapper.Map<BillingAddress, BillingAddressModel>(userBilling);
                return View(userBillingModel);
            }

            var user = await GetCurrentUserAsync();
            var billingAddressEntity = _billingAddressService.GetBillingAddressById(user.BillingAddressId);
            if (billingAddressEntity == null)
                return View();

            var billingAddressModel = _mapper.Map<BillingAddress, BillingAddressModel>(billingAddressEntity);
            return View(billingAddressModel);
        }

        // GET: /Manage/EditBillingAddress
        public IActionResult EditBillingAddress(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {

            }
            var billingAddressEntity = _billingAddressService.GetBillingAddressByEmail(id);
            if (billingAddressEntity == null)
                return View();

            var billingAddressModel = _mapper.Map<BillingAddress, BillingAddressModel>(billingAddressEntity);

            return View(billingAddressModel);
        }

        // POST: /Manage/EditBillingAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBillingAddress(BillingAddressModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                Guid billingAddressId = Guid.NewGuid();

                // check if there are already billing address
                var billingAddressEntity = _billingAddressService.GetBillingAddressById(user.BillingAddressId);
                if (billingAddressEntity == null)
                {
                    billingAddressEntity = _mapper.Map<BillingAddressModel, BillingAddress>(model);
                    billingAddressEntity.Id = billingAddressId;

                    // save billing address in database
                    _billingAddressService.InsertBillingAddress(billingAddressEntity);
                }
                else
                {
                    billingAddressId = billingAddressEntity.Id;
                    billingAddressEntity = _mapper.Map<BillingAddressModel, BillingAddress>(model);
                    billingAddressEntity.Id = billingAddressId;

                    // update billing address in database
                    _billingAddressService.UpdateBillingAddress(billingAddressEntity);
                }


                // update user billing address
                user.BillingAddressId = billingAddressEntity.Id;
                await _userManager.UpdateAsync(user);
            }

            return View(model);
        }

        // GET: /Manage/OrderHistoryList
        public async Task<IActionResult> OrderHistoryList()
        {
            // get user
            var user = await GetCurrentUserAsync();

            // get all orders of current user
            var orderEntites = _orderService.GetAllOrdersByUserId(Guid.Parse(user.Id));

            var orderModels = new List<OrderModel>();
            foreach (var order in orderEntites)
            {
                var orderModel = new OrderModel
                {
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    OrderPlacedDateTime = order.OrderPlacementDateTime,
                };
                orderModels.Add(orderModel);
            }

            return View(orderModels.OrderByDescending(x => x.OrderPlacedDateTime));
        }

        // GET: /Manage/OrderHistory
        public IActionResult OrderHistory(string id)
        {
            // get all orders of current user
            var orderEntity = _orderService.GetOrderByOrderId(id);
            if (orderEntity == null)
                return RedirectToAction("OrderHistoryList");

            var orderModel = new OrderModel
            {
                UserId = orderEntity.UserId,
                OrderNumber = orderEntity.OrderNumber,
                BillingAddressId = orderEntity.BillingAddressId,
                Status = orderEntity.Status,
                OrderPlacedDateTime = orderEntity.OrderPlacementDateTime,
                Items = orderEntity.Items.ToList(),
                TotalOrderPrice = orderEntity.TotalOrderPrice
            };

            // get billing address
            var billingAddressEntity = _billingAddressService.GetBillingAddressById(orderEntity.BillingAddressId);
            if (billingAddressEntity != null)
            {
                orderModel.BillingAdddress = _mapper.Map<BillingAddress, BillingAddressModel>(billingAddressEntity); ;
            }

            return View(orderModel);
        }
        public IActionResult Email()
        {
            return RedirectToAction("List");
        }
        public async Task<IActionResult> List()
        {
            var user = await GetCurrentUserAsync();
            var entities = _contactUsService.GetAllMessages(user.Email);
            var messages = new List<ContactUsMessageModel>();
            foreach (var entity in entities)
            {
                messages.Add(_mapper.Map<ContactUsMessage, ContactUsMessageModel>(entity));
            }
            return View(messages);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            return RedirectToAction("Index");
        }
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            Error
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
        #endregion
    }
}
