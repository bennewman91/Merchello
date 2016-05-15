﻿namespace Merchello.Web.Store.Controllers.Payment
{
    using System;
    using System.Web.Mvc;

    using Merchello.Core.Gateways.Payment;
    using Merchello.Core.Models;
    using Merchello.Providers.Payment.Braintree;
    using Merchello.Web.Controllers;
    using Merchello.Web.Factories;
    using Merchello.Web.Store.Factories;
    using Merchello.Web.Store.Models;
    using Merchello.Web.Store.Models.Async;

    /// <summary>
    /// A base controller for rendering and processing Braintree payments.
    /// </summary>
    /// <typeparam name="TPaymentModel">
    /// The type of <see cref="BraintreePaymentModel"/>
    /// </typeparam>
    public abstract class BraintreePaymentControllerBase<TPaymentModel> : CheckoutPaymentControllerBase<TPaymentModel>
        where TPaymentModel : BraintreePaymentModel, new()
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BraintreePaymentControllerBase{TPaymentModel}"/> class.
        /// </summary>
        protected BraintreePaymentControllerBase()
            : this(
                  new BraintreePaymentModelFactory<TPaymentModel>(),
                  new CheckoutContextSettingsFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BraintreePaymentControllerBase{TPaymentModel}"/> class.
        /// </summary>
        /// <param name="checkoutPaymentModelFactory">
        /// The <see cref="BraintreePaymentModelFactory{TPaymentModel}"/>.
        /// </param>
        protected BraintreePaymentControllerBase(
            BraintreePaymentModelFactory<TPaymentModel> checkoutPaymentModelFactory)
            : this(checkoutPaymentModelFactory, new CheckoutContextSettingsFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BraintreePaymentControllerBase{TPaymentModel}"/> class.
        /// </summary>
        /// <param name="checkoutPaymentModelFactory">
        /// The <see cref="BraintreePaymentModelFactory{TPaymentModel}"/>.
        /// </param>
        /// <param name="contextSettingsFactory">
        /// The <see cref="CheckoutContextSettingsFactory"/>.
        /// </param>
        protected BraintreePaymentControllerBase(
            BraintreePaymentModelFactory<TPaymentModel> checkoutPaymentModelFactory,
            CheckoutContextSettingsFactory contextSettingsFactory)
            : base(checkoutPaymentModelFactory, contextSettingsFactory)
        {
        }

        #endregion


        /// <summary>
        /// Processes a Braintree payment.
        /// </summary>
        /// <param name="nonce">
        /// The payment method nonce
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Process(string nonce)
        {
            var result = ProcessPayment(nonce);

            var response = this.BuildPaymentResponse(result);

            return Json(response);
        }

        /// <summary>
        /// Retries a payment.
        /// </summary>
        /// <param name="nonce">
        /// The nonce.
        /// </param>
        /// <param name="invoiceKey">
        /// The invoice Key.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Retry(string nonce, Guid invoiceKey)
        {

            var invoice = MerchelloServices.InvoiceService.GetByKey(invoiceKey);

            var result = ProcessPayment(nonce, invoice);

            var response = this.BuildPaymentResponse(result);

            return Json(response);
        }

        /// <summary>
        /// Performs the work of processing the payment with Braintree.
        /// </summary>
        /// <param name="nonce">
        /// The 'nonce' generated by Braintree that we use to bill against.
        /// </param>
        /// <param name="invoice">
        /// The invoice.
        /// </param>
        /// <returns>
        /// The <see cref="IPaymentResult"/>.
        /// </returns>
        protected virtual IPaymentResult ProcessPayment(string nonce, IInvoice invoice = null)
        {
            // gets the payment method
            var paymentMethod = CheckoutManager.Payment.GetPaymentMethod();

            // You need a ProcessorArgumentCollection for this transaction to store the payment method nonce
            // The braintree package includes an extension method off of the ProcessorArgumentCollection - SetPaymentMethodNonce([nonce]);
            var args = new ProcessorArgumentCollection();
            args.SetPaymentMethodNonce(nonce);

            // We want this to be an AuthorizeCapture(paymentMethod.Key, args);
            return invoice == null
                       ? CheckoutManager.Payment.AuthorizeCapturePayment(paymentMethod.Key, args)
                       : invoice.AuthorizeCapturePayment(paymentMethod.Key, args);
        }

        /// <summary>
        /// Builds a response for payment attempts.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="PaymentResultAsyncResponse"/>.
        /// </returns>
        protected PaymentResultAsyncResponse BuildPaymentResponse(IPaymentResult result)
        {
            var response = new PaymentResultAsyncResponse
            {
                Success = result.Payment.Success,
                InvoiceKey = result.Invoice.Key,
                PaymentKey = result.Payment.Result.Key
            };

            if (!result.Payment.Success) response.Messages.Add(result.Payment.Exception.Message);

            return response;
        }
    }
}