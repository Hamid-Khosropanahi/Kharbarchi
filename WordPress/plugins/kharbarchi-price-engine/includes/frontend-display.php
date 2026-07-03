<?php

if (!defined('ABSPATH')) {
    exit;
}

function kharbarchi_get_product_package_info($product_id)
{
    $commodity = kharbarchi_get_product_commodity($product_id);

    $base_image_id = 0;
    $base_gallery_ids = '';

    if ($commodity) {
        $base_image_id = absint(get_term_meta($commodity->term_id, 'base_image_id', true));
        $base_gallery_ids = get_term_meta($commodity->term_id, 'base_gallery_ids', true);
    }

    $package_id = absint(get_post_meta($product_id, '_kharbarchi_package_id', true));
    $package_name = get_post_meta($product_id, '_khb_package_title', true);

    if ($package_name === '' && $package_id > 0) {
        $package_name = get_the_title($package_id);
    }

    $unit_weight = get_post_meta($product_id, '_khb_unit_weight', true);
    if (($unit_weight === '' || floatval($unit_weight) <= 0) && $package_id > 0) {
        $unit_weight = get_post_meta($package_id, '_khb_unit_weight', true);
    }

    $carton_count = absint(get_post_meta($product_id, '_khb_product_carton_count', true));
    if ($carton_count <= 0 && $package_id > 0) {
        $carton_count = absint(get_post_meta($package_id, '_khb_default_carton_count', true));
    }

    $image_tag = get_post_meta($product_id, '_khb_image_tag', true);
    if ($image_tag === '' && $package_id > 0) {
        $image_tag = get_post_meta($package_id, '_khb_image_tag', true);
    }

    return [
        'commodity_id'       => $commodity ? $commodity->term_id : 0,
        'commodity_name'     => $commodity ? $commodity->name : get_post_meta($product_id, '_kharbarchi_commodity_name', true),
        'base_image_id'      => $base_image_id,
        'base_gallery_ids'   => $base_gallery_ids,
        'package_id'         => $package_id,
        'package_name'       => $package_name,
        'package_code'       => get_post_meta($product_id, '_khb_package_code', true),
        'package_group'      => get_post_meta($product_id, '_khb_package_group', true),
        'unit_weight'        => $unit_weight,
        'carton_count'       => $carton_count,
        'bulk_weight_kg'     => get_post_meta($product_id, '_khb_bulk_weight_kg', true),
        'min_purchase_kg'    => get_post_meta($product_id, '_khb_min_purchase_kg', true),
        'image_tag'          => $image_tag,
        'brand_name'         => get_post_meta($product_id, '_kharbarchi_brand_name', true),
        'sale_credit'        => get_post_meta($product_id, '_kharbarchi_sale_credit_price', true),
        'sale_cash'          => get_post_meta($product_id, '_kharbarchi_sale_cash_price', true),
        'sale_credit_per_kg' => get_post_meta($product_id, '_kharbarchi_sale_credit_price_per_kg', true),
        'sale_cash_per_kg'   => get_post_meta($product_id, '_kharbarchi_sale_cash_price_per_kg', true),
    ];
}

function kharbarchi_format_public_price($value)
{
    $value = kharbarchi_price_control_float($value);
    if ($value <= 0) {
        return '';
    }
    return number_format($value, 0) . ' تومان';
}

add_action('woocommerce_single_product_summary', 'kharbarchi_show_product_info_frontend', 11);

function kharbarchi_show_product_info_frontend()
{
    global $product;

    if (!$product) {
        return;
    }

    $info = kharbarchi_get_product_package_info($product->get_id());

    if (!$info) {
        return;
    }

    $sale_credit = kharbarchi_price_control_float($info['sale_credit']);
    $sale_cash = kharbarchi_price_control_float($info['sale_cash']);
    $sale_credit_per_kg = kharbarchi_price_control_float($info['sale_credit_per_kg']);
    $sale_cash_per_kg = kharbarchi_price_control_float($info['sale_cash_per_kg']);
    $cash_discount = ($sale_credit > 0 && $sale_cash > 0 && $sale_cash < $sale_credit) ? $sale_credit - $sale_cash : 0;
    $cash_kg_discount = ($sale_credit_per_kg > 0 && $sale_cash_per_kg > 0 && $sale_cash_per_kg < $sale_credit_per_kg) ? $sale_credit_per_kg - $sale_cash_per_kg : 0;
    $is_bulk = strtolower((string) $info['package_group']) === 'bulk';
    ?>
    <div class="kharbarchi-product-info">
        <?php if (!empty($info['brand_name'])) : ?><div><strong>برند:</strong> <?php echo esc_html($info['brand_name']); ?></div><?php endif; ?>
        <?php if (!empty($info['commodity_name'])) : ?><div><strong>کالای پایه:</strong> <?php echo esc_html($info['commodity_name']); ?></div><?php endif; ?>
        <?php if (!empty($info['package_name'])) : ?><div><strong>نوع بسته‌بندی:</strong> <?php echo esc_html($info['package_name']); ?></div><?php endif; ?>

        <?php if ($sale_credit > 0) : ?><div class="khb-public-price"><strong>قیمت فروش کالا:</strong> <?php echo esc_html(kharbarchi_format_public_price($sale_credit)); ?></div><?php endif; ?>
        <?php if ($cash_discount > 0) : ?><div class="khb-public-cash-discount"><strong>تخفیف تسویه نقدی:</strong> <?php echo esc_html(kharbarchi_format_public_price($cash_discount)); ?></div><?php endif; ?>
        <?php if ($sale_cash > 0 && $sale_cash < $sale_credit) : ?><div><strong>قیمت نقدی:</strong> <?php echo esc_html(kharbarchi_format_public_price($sale_cash)); ?></div><?php endif; ?>
        <?php if ($sale_credit_per_kg > 0) : ?><div><strong>قیمت کیلویی فروش:</strong> <?php echo esc_html(kharbarchi_format_public_price($sale_credit_per_kg)); ?></div><?php endif; ?>
        <?php if ($cash_kg_discount > 0) : ?><div><strong>تخفیف نقدی کیلویی:</strong> <?php echo esc_html(kharbarchi_format_public_price($cash_kg_discount)); ?></div><?php endif; ?>

        <?php if ($is_bulk && !empty($info['bulk_weight_kg'])) : ?><div><strong>کیلو خرید:</strong> <?php echo esc_html($info['bulk_weight_kg']); ?> کیلو</div><?php endif; ?>
        <?php if (!$is_bulk && !empty($info['carton_count'])) : ?><div><strong>تعداد در کارتن:</strong> <?php echo esc_html($info['carton_count']); ?> بسته</div><?php endif; ?>
        <?php if (!empty($info['unit_weight'])) : ?><div><strong>وزن واحد:</strong> <?php echo esc_html($info['unit_weight']); ?> کیلوگرم</div><?php endif; ?>
        <?php if ($is_bulk && !empty($info['min_purchase_kg'])) : ?><div><strong>حداقل خرید:</strong> <?php echo esc_html($info['min_purchase_kg']); ?> کیلو</div><?php endif; ?>
    </div>
    <?php
}

add_filter('woocommerce_product_get_image_id', 'kharbarchi_use_commodity_base_image', 10, 2);

function kharbarchi_use_commodity_base_image($image_id, $product)
{
    if ($image_id || !$product) {
        return $image_id;
    }

    $info = kharbarchi_get_product_package_info($product->get_id());

    if (!$info || empty($info['base_image_id'])) {
        return $image_id;
    }

    return absint($info['base_image_id']);
}

add_filter('woocommerce_product_get_gallery_image_ids', 'kharbarchi_use_commodity_base_gallery', 10, 2);

function kharbarchi_use_commodity_base_gallery($gallery_image_ids, $product)
{
    if (!empty($gallery_image_ids) || !$product) {
        return $gallery_image_ids;
    }

    $info = kharbarchi_get_product_package_info($product->get_id());

    if (!$info || empty($info['base_gallery_ids'])) {
        return $gallery_image_ids;
    }

    $ids = explode(',', $info['base_gallery_ids']);
    $ids = array_map('absint', $ids);
    $ids = array_filter($ids);

    return $ids;
}
