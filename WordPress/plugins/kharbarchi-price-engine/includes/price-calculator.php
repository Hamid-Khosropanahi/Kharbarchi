<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * Final pricing rule:
 * Product price is imported from SQL and saved on product meta.
 * WooCommerce official price = credit/conditions final product price.
 * Cash price is not Woo sale_price; it is applied only when cash payment is selected.
 */
function kharbarchi_get_product_commodity($product_id)
{
    $terms = wp_get_post_terms($product_id, 'commodity');

    if (is_wp_error($terms) || empty($terms)) {
        return null;
    }

    return $terms[0];
}

function kharbarchi_direct_price_clean_decimal($value)
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

function kharbarchi_calculate_product_price($product_id)
{
    $credit_price = kharbarchi_direct_price_clean_decimal(
        get_post_meta($product_id, '_kharbarchi_sale_credit_price', true)
    );

    if ($credit_price === '' || floatval($credit_price) <= 0) {
        return false;
    }

    return $credit_price;
}

function kharbarchi_recalculate_product_price($product_id)
{
    static $is_running = false;

    if ($is_running) {
        return false;
    }

    if (get_post_type($product_id) !== 'product') {
        return false;
    }

    if (!function_exists('wc_get_product')) {
        return false;
    }

    $product = wc_get_product($product_id);

    if (!$product) {
        return false;
    }

    $sale_cash_price = kharbarchi_direct_price_clean_decimal(
        get_post_meta($product_id, '_kharbarchi_sale_cash_price', true)
    );

    $sale_credit_price = kharbarchi_direct_price_clean_decimal(
        get_post_meta($product_id, '_kharbarchi_sale_credit_price', true)
    );

    if ($sale_credit_price === '' || floatval($sale_credit_price) <= 0) {
        if (function_exists('kharbarchi_update_price_control_meta')) {
            kharbarchi_update_price_control_meta($product_id);
        }
        return false;
    }

    update_post_meta($product_id, '_kharbarchi_sale_credit_price', $sale_credit_price);
    update_post_meta($product_id, '_kharbarchi_sale_cash_price', $sale_cash_price);

    $is_running = true;

    $product->set_regular_price($sale_credit_price);
    $product->set_sale_price('');
    $product->set_price($sale_credit_price);
    $product->save();

    update_post_meta($product_id, '_regular_price', $sale_credit_price);
    update_post_meta($product_id, '_sale_price', '');
    update_post_meta($product_id, '_price', $sale_credit_price);

    $package_group = strtolower((string) get_post_meta($product_id, '_khb_package_group', true));
    $unit_measure = $package_group === 'bulk' ? 'گونی' : 'کارتن';
    update_post_meta($product_id, 'woodmart_price_unit_of_measure', $unit_measure);

    if (function_exists('kharbarchi_update_price_control_meta')) {
        kharbarchi_update_price_control_meta($product_id);
    }

    $is_running = false;

    return $sale_credit_price;
}

add_action('save_post_product', 'kharbarchi_recalculate_price_on_product_save', 50, 3);

function kharbarchi_recalculate_price_on_product_save($post_id, $post, $update)
{
    if (defined('DOING_AUTOSAVE') && DOING_AUTOSAVE) {
        return;
    }

    if (wp_is_post_revision($post_id)) {
        return;
    }

    if (!current_user_can('edit_post', $post_id)) {
        return;
    }

    kharbarchi_recalculate_product_price($post_id);
}
