﻿namespace Merchello.FastTrack.Controllers.Payment
{
    using System.Web.Mvc;

    using Merchello.Core.Gateways;
    using Merchello.FastTrack.Models.Payment;
    using Merchello.Web.Store.Controllers.Payment;

    using Umbraco.Core;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// A controller responsible for rendering and processing Braintree PayPal payments.
    /// </summary>
    [PluginController("FastTrack")]
    [GatewayMethodUi("BrainTree.PayPal.OneTime")]
    public class BraintreePayPalController : BraintreePaymentControllerBase<FastTrackBraintreePaymentModel>
    {
        /// <summary>
        /// Responsible for rendering the BrainTree PayPal payment form.
        /// </summary>
        /// <param name="view">
        /// The optional view.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [ChildActionOnly]
        [GatewayMethodUi("BrainTree.PayPal.OneTime")]
        public override ActionResult PaymentForm(string view = "")
        {
            var paymentMethod = this.CheckoutManager.Payment.GetPaymentMethod();
            if (paymentMethod == null) return this.InvalidCheckoutStagePartial();

            var model = this.CheckoutPaymentModelFactory.Create(CurrentCustomer, paymentMethod);

            return view.IsNullOrWhiteSpace() ? this.PartialView(model) : this.PartialView(view, model);
        }
    }
}