using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.SinglePageCheckout.Models;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.SinglePageCheckout.Components;

/// <summary>
/// Builds the composite single-page checkout model by reusing nopCommerce's own checkout and
/// shopping-cart model factories, then renders the flat single-page layout.
/// </summary>
public class SinglePageCheckoutViewComponent : NopViewComponent
{
    #region Fields

    protected readonly AddressSettings _addressSettings;
    protected readonly ICheckoutModelFactory _checkoutModelFactory;
    protected readonly ICustomerService _customerService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;
    protected readonly SinglePageCheckoutSettings _settings;

    #endregion

    #region Ctor

    public SinglePageCheckoutViewComponent(AddressSettings addressSettings,
        ICheckoutModelFactory checkoutModelFactory,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        IOrderProcessingService orderProcessingService,
        IPaymentPluginManager paymentPluginManager,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext,
        SinglePageCheckoutSettings settings)
    {
        _addressSettings = addressSettings;
        _checkoutModelFactory = checkoutModelFactory;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _orderProcessingService = orderProcessingService;
        _paymentPluginManager = paymentPluginManager;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
        _settings = settings;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var model = new SinglePageCheckoutModel
        {
            OnePageCheckout = await _checkoutModelFactory.PrepareOnePageCheckoutModelAsync(cart),
            ShowDiscountBox = _settings.ShowDiscountBox,
            ShowGiftCardBox = _settings.ShowGiftCardBox,
            ShowCheckoutAttributes = _settings.ShowCheckoutAttributes,
            ShowEstimateShipping = _settings.ShowEstimateShipping
        };
        model.ShippingRequired = model.OnePageCheckout.ShippingRequired;

        //shipping address form + shipping methods (computed from the customer's current shipping
        //address; the methods are refreshed by AJAX once a new address is entered)
        if (model.ShippingRequired)
        {
            var shippingAddressModel = new Nop.Web.Models.Checkout.CheckoutShippingAddressModel();
            await _checkoutModelFactory.PrepareShippingAddressModelAsync(shippingAddressModel, cart,
                prePopulateNewAddressWithCustomerFields: true);
            model.ShippingAddress = shippingAddressModel;

            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
            model.ShippingMethod = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, shippingAddress);
        }

        //payment workflow
        model.PaymentRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);

        var filterByCountryId = 0;
        if (_addressSettings.CountryEnabled)
            filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
        model.PaymentMethod = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

        //payment info for the currently selected (or first available) payment method
        if (model.PaymentRequired)
        {
            var selectedSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(selectedSystemName, customer, store.Id);

            if (paymentMethod == null && model.PaymentMethod.PaymentMethods.Any())
            {
                var firstSystemName = model.PaymentMethod.PaymentMethods.First().PaymentMethodSystemName;
                paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(firstSystemName, customer, store.Id);
            }

            if (paymentMethod != null && !paymentMethod.SkipPaymentInfo)
                model.PaymentInfo = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
        }

        //order summary (cart items + discount/gift-card boxes + checkout attributes) and totals
        model.Cart = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart,
            isEditable: false, validateCheckoutAttributes: false, prepareAndDisplayOrderReviewData: true);
        model.OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, isEditable: false);

        //estimate shipping block
        if (_settings.ShowEstimateShipping && model.ShippingRequired)
            model.EstimateShipping = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(cart);

        var viewName = _settings.LayoutType == CheckoutLayoutType.Layout02
            ? "~/Plugins/Misc.SinglePageCheckout/Views/Components/SinglePageCheckout/Layout02.cshtml"
            : "~/Plugins/Misc.SinglePageCheckout/Views/Components/SinglePageCheckout/Default.cshtml";

        return View(viewName, model);
    }

    #endregion
}
