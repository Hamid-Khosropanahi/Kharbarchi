<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * گرفتن همه محصولات وابسته به یک کالای پایه
 */
function kharbarchi_get_products_by_commodity($commodity_id)
{
    $product_ids = get_posts([
        'post_type'      => 'product',
        'post_status'    => ['publish', 'draft', 'pending', 'private'],
        'posts_per_page' => -1,
        'fields'         => 'ids',
        'tax_query'      => [
            [
                'taxonomy' => 'commodity',
                'field'    => 'term_id',
                'terms'    => absint($commodity_id),
            ],
        ],
    ]);

    return $product_ids;
}

/**
 * بروزرسانی قیمت محصولات یک کالای پایه
 */
function kharbarchi_update_products_by_commodity($commodity_id)
{
    if (!function_exists('kharbarchi_recalculate_product_price')) {
        return 0;
    }

    $product_ids = kharbarchi_get_products_by_commodity($commodity_id);

    $updated_count = 0;

    foreach ($product_ids as $product_id) {
        $result = kharbarchi_recalculate_product_price($product_id);

        if ($result !== false) {
            $updated_count++;
        }
    }

    return $updated_count;
}

/**
 * بروزرسانی همه محصولات خواربارچی
 */
function kharbarchi_update_all_product_prices()
{
    if (!function_exists('kharbarchi_recalculate_product_price')) {
        return 0;
    }

    $product_ids = get_posts([
        'post_type'      => 'product',
        'post_status'    => ['publish', 'draft', 'pending', 'private'],
        'posts_per_page' => -1,
        'fields'         => 'ids',
        'meta_query'     => [
            [
                'key'     => '_kharbarchi_package_id',
                'compare' => 'EXISTS',
            ],
        ],
    ]);

    $updated_count = 0;

    foreach ($product_ids as $product_id) {
        $result = kharbarchi_recalculate_product_price($product_id);

        if ($result !== false) {
            $updated_count++;
        }
    }

    return $updated_count;
}

/**
 * وقتی قیمت کالای پایه ویرایش شد،
 * همه محصولات همان کالای پایه بروزرسانی شوند.
 */
add_action(
    'edited_commodity',
    'kharbarchi_update_prices_after_commodity_edit',
    20,
    1
);

function kharbarchi_update_prices_after_commodity_edit($term_id)
{
    kharbarchi_update_products_by_commodity($term_id);
}

/**
 * وقتی کالای پایه جدید ساخته شد، فعلاً محصولی ندارد؛
 * ولی هوک را می‌گذاریم برای کامل بودن.
 */
add_action(
    'created_commodity',
    'kharbarchi_update_prices_after_commodity_create',
    20,
    1
);

function kharbarchi_update_prices_after_commodity_create($term_id)
{
    kharbarchi_update_products_by_commodity($term_id);
}

/**
 * اگر نوع بسته‌بندی تغییر کرد،
 * همه محصولاتی که از آن بسته‌بندی استفاده می‌کنند آپدیت شوند.
 */
add_action(
    'save_post_kharbarchi_package',
    'kharbarchi_update_prices_after_package_save',
    30,
    3
);

function kharbarchi_update_prices_after_package_save($post_id, $post, $update)
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

    $product_ids = get_posts([
        'post_type'      => 'product',
        'post_status'    => ['publish', 'draft', 'pending', 'private'],
        'posts_per_page' => -1,
        'fields'         => 'ids',
        'meta_query'     => [
            [
                'key'   => '_kharbarchi_package_id',
                'value' => absint($post_id),
            ],
        ],
    ]);

    foreach ($product_ids as $product_id) {
        kharbarchi_recalculate_product_price($product_id);
    }
}

/**
 * دکمه بروزرسانی کلی در صفحه تنظیمات
 */
add_action(
    'admin_post_kharbarchi_update_all_prices',
    'kharbarchi_handle_update_all_prices'
);

function kharbarchi_handle_update_all_prices()
{
    if (!current_user_can('manage_woocommerce')) {
        wp_die('دسترسی غیرمجاز');
    }

    if (
        !isset($_GET['_wpnonce']) ||
        !wp_verify_nonce(
            $_GET['_wpnonce'],
            'kharbarchi_update_all_prices'
        )
    ) {
        wp_die('درخواست نامعتبر است');
    }

    $updated_count = kharbarchi_update_all_product_prices();

    wp_redirect(
        add_query_arg(
            [
                'post_type' => 'product',
                'page' => 'kharbarchi-settings',
                'kharbarchi_updated' => $updated_count,
            ],
            admin_url('edit.php')
        )
    );

    exit;
}
function kharbarchi_update_products_by_package($package_id)
{
    if (!function_exists('kharbarchi_recalculate_product_price')) {
        return 0;
    }

    $product_ids = get_posts([
        'post_type'      => 'product',
        'post_status'    => ['publish', 'draft', 'pending', 'private'],
        'posts_per_page' => -1,
        'fields'         => 'ids',
        'meta_query'     => [
            [
                'key'   => '_kharbarchi_package_id',
                'value' => absint($package_id),
            ],
        ],
    ]);

    $updated_count = 0;

    foreach ($product_ids as $product_id) {
        $result = kharbarchi_recalculate_product_price($product_id);

        if ($result !== false) {
            $updated_count++;
        }
    }

    return $updated_count;
}