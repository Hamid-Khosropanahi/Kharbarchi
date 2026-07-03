<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * Kharbarchi payment pricing rules.
 *
 * Final policy:
 * - The official WooCommerce price is the credit/terms price.
 * - Cash is a fast-settlement discount.
 * - Payment type is inferred from the selected WooCommerce payment gateway, not a customer radio field.
 * - Internal buy totals are saved as hidden order meta only and are never displayed by the plugin.
 */

const KHARBARCHI_ORDER_BUY_CASH_TOTAL_META = '_khb_o_bct';
const KHARBARCHI_ORDER_BUY_CREDIT_TOTAL_META = '_khb_o_brt';

function kharbarchi_get_default_payment_type()
{
    $default = get_option('kharbarchi_default_payment_type', 'credit');
    return in_array($default, ['cash', 'credit'], true) ? $default : 'credit';
}

function kharbarchi_normalize_gateway_ids($gateway_ids)
{
    if (!is_array($gateway_ids)) {
        return [];
    }

    $normalized = [];

    foreach ($gateway_ids as $gateway_id) {
        $gateway_id = sanitize_key((string) $gateway_id);

        if ($gateway_id !== '') {
            $normalized[] = $gateway_id;
        }
    }

    return array_values(array_unique($normalized));
}

function kharbarchi_get_cash_gateway_ids()
{
    return kharbarchi_normalize_gateway_ids((array) get_option('kharbarchi_cash_gateway_ids', []));
}

function kharbarchi_get_credit_gateway_ids()
{
    return kharbarchi_normalize_gateway_ids((array) get_option('kharbarchi_credit_gateway_ids', []));
}

function kharbarchi_resolve_payment_type_from_gateway($gateway_id)
{
    $gateway_id = sanitize_key((string) $gateway_id);

    if ($gateway_id === '') {
        return kharbarchi_get_default_payment_type();
    }

    if (in_array($gateway_id, kharbarchi_get_cash_gateway_ids(), true)) {
        return 'cash';
    }

    if (in_array($gateway_id, kharbarchi_get_credit_gateway_ids(), true)) {
        return 'credit';
    }

    return kharbarchi_get_default_payment_type();
}

function kharbarchi_get_current_gateway_id_from_request()
{
    if (isset($_POST['payment_method'])) {
        return sanitize_key(wp_unslash($_POST['payment_method']));
    }

    return '';
}

function kharbarchi_get_current_gateway_id_from_session()
{
    if (!WC()->session) {
        return '';
    }

    $gateway_id = WC()->session->get('chosen_payment_method', '');

    return sanitize_key((string) $gateway_id);
}

function kharbarchi_set_session_payment_mapping($gateway_id)
{
    if (!WC()->session) {
        return kharbarchi_get_default_payment_type();
    }

    $gateway_id = sanitize_key((string) $gateway_id);
    $payment_type = kharbarchi_resolve_payment_type_from_gateway($gateway_id);

    if ($gateway_id !== '') {
        WC()->session->set('chosen_payment_method', $gateway_id);
    }

    WC()->session->set('kharbarchi_payment_gateway_id', $gateway_id);
    WC()->session->set('kharbarchi_payment_type', $payment_type);

    return $payment_type;
}

function kharbarchi_get_current_payment_type()
{
    if (!WC()->session) {
        return kharbarchi_get_default_payment_type();
    }

    $gateway_id = kharbarchi_get_current_gateway_id_from_session();

    if ($gateway_id !== '') {
        return kharbarchi_set_session_payment_mapping($gateway_id);
    }

    $session_payment_type = WC()->session->get('kharbarchi_payment_type', '');

    if (in_array($session_payment_type, ['cash', 'credit'], true)) {
        return $session_payment_type;
    }

    return kharbarchi_get_default_payment_type();
}

add_action('woocommerce_checkout_update_order_review', 'kharbarchi_update_payment_type_from_gateway', 5);

function kharbarchi_update_payment_type_from_gateway($post_data)
{
    parse_str($post_data, $data);

    $gateway_id = '';

    if (isset($data['payment_method'])) {
        $gateway_id = sanitize_key(wp_unslash($data['payment_method']));
    }

    kharbarchi_set_session_payment_mapping($gateway_id);
}

add_action('woocommerce_review_order_before_payment', 'kharbarchi_show_gateway_payment_type_notice');

function kharbarchi_show_gateway_payment_type_notice()
{
    $payment_type = kharbarchi_get_current_payment_type();
    $gateway_id = kharbarchi_get_current_gateway_id_from_session();

    if ($payment_type === 'cash') {
        $label = 'تسویه نقدی';
        $note = 'قیمت نقدی به‌عنوان تخفیف تسویه سریع اعمال می‌شود.';
    } else {
        $label = 'خرید شرایطی';
        $note = 'قیمت رسمی/شرایطی کالا اعمال می‌شود.';
    }

    echo '<div class="kharbarchi-payment-type kharbarchi-payment-type-auto">';
    echo '<h3>نوع پرداخت خواربارچی</h3>';
    echo '<p><strong>' . esc_html($label) . '</strong></p>';
    echo '<p><small>' . esc_html($note) . '</small></p>';

    if ($gateway_id !== '') {
        echo '<p><small>روش پرداخت انتخاب‌شده: <code>' . esc_html($gateway_id) . '</code></small></p>';
    }

    echo '</div>';
}

function kharbarchi_get_product_cash_credit_prices($product_id, $product = null)
{
    $cash_price = floatval(get_post_meta($product_id, '_kharbarchi_sale_cash_price', true));
    $credit_price = floatval(get_post_meta($product_id, '_kharbarchi_sale_credit_price', true));

    if ($credit_price <= 0 && $product) {
        $credit_price = floatval($product->get_regular_price());
    }

    if ($cash_price <= 0) {
        $cash_price = $credit_price;
    }

    if ($credit_price > 0 && $cash_price > $credit_price) {
        $cash_price = $credit_price;
    }

    return [$cash_price, $credit_price];
}

function kharbarchi_get_product_buy_prices($product_id)
{
    $buy_cash_price = floatval(get_post_meta($product_id, '_kharbarchi_buy_cash_price', true));
    $buy_credit_price = floatval(get_post_meta($product_id, '_kharbarchi_buy_credit_price', true));

    return [$buy_cash_price, $buy_credit_price];
}

add_action('woocommerce_before_calculate_totals', 'kharbarchi_apply_payment_type_prices', 20);

function kharbarchi_apply_payment_type_prices($cart)
{
    if (is_admin() && !defined('DOING_AJAX')) {
        return;
    }

    if (!$cart || $cart->is_empty()) {
        return;
    }

    $request_gateway_id = kharbarchi_get_current_gateway_id_from_request();

    if ($request_gateway_id !== '') {
        $payment_type = kharbarchi_set_session_payment_mapping($request_gateway_id);
    } else {
        $payment_type = kharbarchi_get_current_payment_type();
    }

    foreach ($cart->get_cart() as $cart_item) {
        if (empty($cart_item['data']) || empty($cart_item['product_id'])) {
            continue;
        }

        $product_id = $cart_item['product_id'];
        $product = $cart_item['data'];
        [$cash_price, $credit_price] = kharbarchi_get_product_cash_credit_prices($product_id, $product);

        if ($payment_type === 'cash') {
            $product->set_price($cash_price);
        } else {
            $product->set_price($credit_price);
        }
    }
}

add_filter('woocommerce_cart_item_name', 'kharbarchi_show_cash_discount_in_cart', 20, 3);

function kharbarchi_show_cash_discount_in_cart($product_name, $cart_item, $cart_item_key)
{
    $payment_type = kharbarchi_get_current_payment_type();

    if ($payment_type !== 'cash' || empty($cart_item['product_id'])) {
        return $product_name;
    }

    [$cash_price, $credit_price] = kharbarchi_get_product_cash_credit_prices($cart_item['product_id']);

    if ($credit_price <= 0 || $cash_price <= 0 || $cash_price >= $credit_price) {
        return $product_name;
    }

    $discount_amount = $credit_price - $cash_price;
    $discount_percent = ($discount_amount / $credit_price) * 100;

    $product_name .= '<br><small class="kharbarchi-cash-discount">';
    $product_name .= 'تخفیف تسویه نقدی: ' . wc_price($discount_amount);
    $product_name .= ' معادل ' . number_format($discount_percent, 1) . '%';
    $product_name .= '</small>';

    return $product_name;
}

function kharbarchi_get_cart_total_cash_discount()
{
    if (!WC()->cart) {
        return 0;
    }

    $payment_type = kharbarchi_get_current_payment_type();

    if ($payment_type !== 'cash') {
        return 0;
    }

    $total_discount = 0;

    foreach (WC()->cart->get_cart() as $cart_item) {
        if (empty($cart_item['product_id'])) {
            continue;
        }

        $qty = absint($cart_item['quantity']);
        [$cash_price, $credit_price] = kharbarchi_get_product_cash_credit_prices($cart_item['product_id']);

        if ($credit_price > 0 && $cash_price > 0 && $cash_price < $credit_price) {
            $total_discount += ($credit_price - $cash_price) * $qty;
        }
    }

    return $total_discount;
}

function kharbarchi_get_cart_internal_buy_totals()
{
    $totals = [
        'cash' => 0,
        'credit' => 0,
    ];

    if (!WC()->cart) {
        return $totals;
    }

    foreach (WC()->cart->get_cart() as $cart_item) {
        if (empty($cart_item['product_id'])) {
            continue;
        }

        $qty = absint($cart_item['quantity']);
        [$buy_cash_price, $buy_credit_price] = kharbarchi_get_product_buy_prices($cart_item['product_id']);

        if ($buy_cash_price > 0) {
            $totals['cash'] += $buy_cash_price * $qty;
        }

        if ($buy_credit_price > 0) {
            $totals['credit'] += $buy_credit_price * $qty;
        }
    }

    return $totals;
}

add_action('woocommerce_cart_totals_before_order_total', 'kharbarchi_show_total_cash_discount');
add_action('woocommerce_review_order_before_order_total', 'kharbarchi_show_total_cash_discount');

function kharbarchi_show_total_cash_discount()
{
    $discount = kharbarchi_get_cart_total_cash_discount();

    if ($discount <= 0) {
        return;
    }

    echo '<tr class="kharbarchi-total-cash-discount">';
    echo '<th>تخفیف کل تسویه نقدی</th>';
    echo '<td>' . wc_price($discount) . '</td>';
    echo '</tr>';
}

add_action('woocommerce_checkout_create_order', 'kharbarchi_save_payment_type_to_order', 20, 2);

function kharbarchi_save_payment_type_to_order($order, $data)
{
    $gateway_id = '';

    if (is_array($data) && isset($data['payment_method'])) {
        $gateway_id = sanitize_key($data['payment_method']);
    }

    if ($gateway_id === '') {
        $gateway_id = kharbarchi_get_current_gateway_id_from_session();
    }

    $payment_type = kharbarchi_resolve_payment_type_from_gateway($gateway_id);

    if (WC()->session) {
        WC()->session->set('kharbarchi_payment_gateway_id', $gateway_id);
        WC()->session->set('kharbarchi_payment_type', $payment_type);
    }

    $discount = kharbarchi_get_cart_total_cash_discount();
    $buy_totals = kharbarchi_get_cart_internal_buy_totals();

    $order->update_meta_data('_kharbarchi_payment_type', $payment_type);
    $order->update_meta_data('_kharbarchi_payment_gateway_id', $gateway_id);
    $order->update_meta_data('_kharbarchi_cash_discount_total', $discount);

    // Hidden internal cost totals. The plugin never displays these values in admin or frontend.
    $order->update_meta_data(KHARBARCHI_ORDER_BUY_CASH_TOTAL_META, $buy_totals['cash']);
    $order->update_meta_data(KHARBARCHI_ORDER_BUY_CREDIT_TOTAL_META, $buy_totals['credit']);
}

add_action('woocommerce_admin_order_data_after_order_details', 'kharbarchi_show_order_payment_info_admin');

function kharbarchi_show_order_payment_info_admin($order)
{
    $payment_type = $order->get_meta('_kharbarchi_payment_type');
    $discount = floatval($order->get_meta('_kharbarchi_cash_discount_total'));
    $gateway_id = $order->get_meta('_kharbarchi_payment_gateway_id');

    echo '<p><strong>نوع پرداخت خواربارچی:</strong> ' . ($payment_type === 'credit' ? 'شرایطی' : 'نقدی') . '</p>';

    if (!empty($gateway_id)) {
        echo '<p><strong>روش پرداخت:</strong> <code>' . esc_html($gateway_id) . '</code></p>';
    }

    if ($discount > 0) {
        echo '<p><strong>تخفیف کل تسویه نقدی:</strong> ' . wc_price($discount) . '</p>';
    }
}

add_action('woocommerce_order_details_after_order_table', 'kharbarchi_show_order_payment_info_front');

function kharbarchi_show_order_payment_info_front($order)
{
    if (!$order instanceof WC_Order) {
        return;
    }

    $payment_type = $order->get_meta('_kharbarchi_payment_type');
    $discount = floatval($order->get_meta('_kharbarchi_cash_discount_total'));

    echo '<section class="kharbarchi-order-payment-info">';
    echo '<h2>اطلاعات پرداخت</h2>';
    echo '<p>نوع پرداخت: ' . ($payment_type === 'credit' ? 'شرایطی' : 'نقدی') . '</p>';

    if ($discount > 0) {
        echo '<p>تخفیف کل تسویه نقدی: ' . wc_price($discount) . '</p>';
    }

    echo '</section>';
}
