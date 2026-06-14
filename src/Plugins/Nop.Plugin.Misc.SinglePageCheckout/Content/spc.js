/* Single Page Checkout - orchestrates nopCommerce's existing Opc* checkout endpoints
 * from a single flat page. On "Complete" it saves each section in order
 * (billing -> shipping -> shipping method -> payment method -> payment info -> confirm)
 * and then redirects to the order completed page. */
(function () {
    'use strict';

    $(function () {
        var $spc = $('.spc');
        if (!$spc.length)
            return;

        var base = $spc.data('checkout-url');
        var completedUrl = $spc.data('completed-url');
        var failMsg = $spc.data('fail-msg') || 'We could not process your order. Please review your details and try again.';
        var shippingRequired = $spc.data('shipping-required') === true || $spc.data('shipping-required') === 'true';
        var paymentRequired = $spc.data('payment-required') === true || $spc.data('payment-required') === 'true';
        var hasPaymentInfo = $spc.data('payment-info') === true || $spc.data('payment-info') === 'true';

        var billingNewAddressId = $spc.data('billing-newaddr-id') || 0;

        var $button = $('#spc-place-order');
        var $wait = $('#spc-please-wait');
        var $messages = $('#spc-messages');

        //reuse nop's Billing/Shipping modules so the address-book dropdown and the
        //"new address" form behave exactly as on the default checkout
        function initAddressForms() {
            if (typeof Billing !== 'undefined') {
                Billing.init('#co-billing-form', base + 'GetAddressById/', base + 'OpcSaveBilling/', false, false);
                if ($('#billing-address-select').length)
                    Billing.newAddress(!$('#billing-address-select').val());
            }
            if (shippingRequired && typeof Shipping !== 'undefined') {
                Shipping.init('#co-shipping-form', base + 'OpcSaveShipping/');
                if ($('#shipping-address-select').length)
                    Shipping.newAddress($('#shipping-address-select').val(), billingNewAddressId);
            }
            //country -> state cascade is wired globally by public.countryselect.js
            $('select[data-trigger="country-select"]').each(function () {
                if ($.fn.countrySelect) $(this).countrySelect();
            });
        }

        //hide the shipping address block when "ship to same address" is checked
        function syncShipToSameAddress() {
            var $cb = $('#ShipToSameAddress');
            if ($cb.length)
                $('#opc-shipping').toggle(!$cb.is(':checked'));
        }

        //containers updated as the server returns fresh section html
        var sectionContainers = {
            'shipping': '#checkout-shipping-load',
            'shipping-method': '#checkout-shipping-method-load',
            'payment-method': '#checkout-payment-method-load',
            'payment-info': '#checkout-payment-info-load'
        };

        function antiForgeryToken() {
            return $('input[name=__RequestVerificationToken]').first().val();
        }

        function postForm(action, formSelector) {
            var data = $(formSelector).serialize();
            var token = antiForgeryToken();
            if (token)
                data += (data ? '&' : '') + '__RequestVerificationToken=' + encodeURIComponent(token);
            return $.ajax({ url: base + action, type: 'POST', cache: false, data: data });
        }

        function postData(action, obj) {
            obj = obj || {};
            var token = antiForgeryToken();
            if (token)
                obj.__RequestVerificationToken = token;
            return $.ajax({ url: base + action, type: 'POST', cache: false, data: obj });
        }

        function isError(response) {
            return response && response.error;
        }

        function errorMessages(response) {
            if (!response)
                return [failMsg];
            if (typeof response.message === 'string')
                return [response.message];
            if (Array.isArray(response.message))
                return response.message;
            return [failMsg];
        }

        function showErrors(messages) {
            var $ul = $messages.find('ul').empty();
            $.each(messages, function (i, m) { $ul.append($('<li/>').text(m)); });
            $messages.show();
            $('html, body').animate({ scrollTop: $messages.offset().top - 80 }, 200);
        }

        function clearErrors() {
            $messages.hide().find('ul').empty();
        }

        function setWaiting(on) {
            $button.prop('disabled', on);
            $wait.toggle(on);
        }

        //inject any fresh section html the server returned so selections stay valid
        function applySection(response) {
            if (response && response.update_section && response.update_section.name) {
                var selector = sectionContainers[response.update_section.name];
                if (selector && typeof response.update_section.html === 'string')
                    $(selector).html(response.update_section.html);
            }
        }

        function selectedOrFirst(name) {
            var $checked = $('input[name="' + name + '"]:checked');
            if ($checked.length)
                return $checked.val();
            var $first = $('input[name="' + name + '"]').first();
            if ($first.length) {
                $first.prop('checked', true);
                return $first.val();
            }
            return null;
        }

        async function placeOrder() {
            clearErrors();
            setWaiting(true);
            try {
                //1) billing address
                var billing = await postForm('OpcSaveBilling', '#co-billing-form');
                if (isError(billing)) { showErrors(errorMessages(billing)); return; }
                applySection(billing);
                var billingNext = billing.update_section ? billing.update_section.name : null;

                //2) shipping address (skipped when "ship to same address"/pickup advanced us past it)
                if (shippingRequired) {
                    if (billingNext === 'shipping') {
                        var shipping = await postForm('OpcSaveShipping', '#co-shipping-form');
                        if (isError(shipping)) { showErrors(errorMessages(shipping)); return; }
                        applySection(shipping);
                    }

                    //3) shipping method
                    var shippingOption = selectedOrFirst('shippingoption');
                    if (shippingOption) {
                        var shipMethod = await postData('OpcSaveShippingMethod', { shippingoption: shippingOption });
                        if (isError(shipMethod)) { showErrors(errorMessages(shipMethod)); return; }
                        applySection(shipMethod);
                    }
                }

                //4) + 5) payment method and payment info
                if (paymentRequired) {
                    var paymentMethod = selectedOrFirst('paymentmethod');
                    var payMethod = await postData('OpcSavePaymentMethod', { paymentmethod: paymentMethod });
                    if (isError(payMethod)) { showErrors(errorMessages(payMethod)); return; }
                    applySection(payMethod);

                    if (hasPaymentInfo && $('#co-payment-info-form').length) {
                        var payInfo = await postForm('OpcSavePaymentInfo', '#co-payment-info-form');
                        if (isError(payInfo)) { showErrors(errorMessages(payInfo)); return; }
                        applySection(payInfo);
                    }
                }

                //6) confirm + place order
                var confirm = await postData('OpcConfirmOrder', {});
                if (confirm.error) { showErrors(errorMessages(confirm)); return; }
                if (confirm.redirect) { window.location = confirm.redirect; return; }
                if (confirm.success) { window.location = completedUrl; return; }

                //confirm came back with warnings rendered as a section
                if (confirm.update_section && typeof confirm.update_section.html === 'string') {
                    var warnings = $('<div/>').html(confirm.update_section.html).find('.message-error li, .warning li');
                    if (warnings.length) {
                        showErrors(warnings.map(function () { return $(this).text(); }).get());
                        return;
                    }
                }
                showErrors([failMsg]);
            } catch (e) {
                showErrors([failMsg]);
            } finally {
                setWaiting(false);
            }
        }

        initAddressForms();
        syncShipToSameAddress();
        $('#ShipToSameAddress').on('change', syncShipToSameAddress);
        $button.on('click', placeOrder);
    });
})();
