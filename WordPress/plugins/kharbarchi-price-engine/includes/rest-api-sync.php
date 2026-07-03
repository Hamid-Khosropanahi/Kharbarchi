<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('woocommerce_rest_insert_product_object', 'kharbarchi_sync_after_rest_api_product_save', 20, 3);

function kharbarchi_sync_after_rest_api_product_save($product, $request, $creating)
{
    if (!$product || !is_a($product, 'WC_Product')) {
        return;
    }

    $product_id = $product->get_id();

    if (!$product_id) {
        return;
    }

    kharbarchi_normalize_api_price_meta($product_id);

    if (function_exists('kharbarchi_recalculate_product_price')) {
        kharbarchi_recalculate_product_price($product_id);
    }

    if (function_exists('kharbarchi_update_price_control_meta')) {
        kharbarchi_update_price_control_meta($product_id);
    }
}

function kharbarchi_normalize_api_price_meta($product_id)
{
    $decimal_keys = [
        '_kharbarchi_sale_cash_price',
        '_kharbarchi_sale_credit_price',
        '_kharbarchi_buy_cash_price',
        '_kharbarchi_buy_credit_price',
        '_kharbarchi_sale_cash_price_per_kg',
        '_kharbarchi_sale_credit_price_per_kg',
        '_kharbarchi_buy_cash_price_per_kg',
        '_kharbarchi_buy_credit_price_per_kg',
        '_khb_unit_weight',
        '_khb_bulk_weight_kg',
        '_khb_min_purchase_kg',
    ];

    foreach ($decimal_keys as $key) {
        $value = kharbarchi_clean_decimal(get_post_meta($product_id, $key, true));
        update_post_meta($product_id, $key, $value);
    }

    $package_group = strtolower((string) get_post_meta($product_id, '_khb_package_group', true));
    update_post_meta($product_id, '_sale_price', '');
    update_post_meta($product_id, 'woodmart_price_unit_of_measure', $package_group === 'bulk' ? 'گونی' : 'کارتن');

    if (get_post_meta($product_id, '_khb_price_source_mode', true) === '') {
        update_post_meta($product_id, '_khb_price_source_mode', 'final_price');
    }
}

function kharbarchi_clean_decimal($value)
{
    if ($value === '' || $value === null) {
        return '';
    }

    $value = wp_unslash($value);
    $value = str_replace(',', '', (string) $value);
    $value = str_replace('٫', '.', $value);

    if (!is_numeric($value)) {
        return '';
    }

    return wc_format_decimal($value, 4);
}
