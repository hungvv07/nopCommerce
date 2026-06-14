using Microsoft.AspNetCore.Mvc;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.OrderItemsRowDetails.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
public class WidgetsOrderItemsRowDetailsController : BasePluginController
{
    #region Fields

    protected readonly IOrderModelFactory _orderModelFactory;
    protected readonly IOrderService _orderService;

    #endregion

    #region Ctor

    public WidgetsOrderItemsRowDetailsController(IOrderModelFactory orderModelFactory,
        IOrderService orderService)
    {
        _orderModelFactory = orderModelFactory;
        _orderService = orderService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the products of a single order as a partial table, loaded over AJAX
    /// when an order row is expanded in the admin order list.
    /// </summary>
    [HttpGet]
    [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
    public virtual async Task<IActionResult> OrderItems(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Content(string.Empty);

        //reuse the core factory so prices, tax display and vendor filtering match the order details page
        var model = await _orderModelFactory.PrepareOrderModelAsync(new OrderModel(), order);

        return View("~/Plugins/Widgets.OrderItemsRowDetails/Views/OrderItems.cshtml", model);
    }

    #endregion
}
