<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('rest_api_init', 'kharbarchi_register_import_upsert_endpoints');

function kharbarchi_import_api_can_manage()
{
    return current_user_can('manage_woocommerce') || current_user_can('edit_products');
}

function kharbarchi_register_import_upsert_endpoints()
{
    register_rest_route('khb/v1', '/category/upsert', [
        'methods' => WP_REST_Server::CREATABLE,
        'callback' => 'kharbarchi_api_upsert_product_category',
        'permission_callback' => 'kharbarchi_import_api_can_manage',
    ]);

    register_rest_route('khb/v1', '/commodity/upsert', [
        'methods' => WP_REST_Server::CREATABLE,
        'callback' => 'kharbarchi_api_upsert_commodity',
        'permission_callback' => 'kharbarchi_import_api_can_manage',
    ]);

    register_rest_route('khb/v1', '/package/upsert', [
        'methods' => WP_REST_Server::CREATABLE,
        'callback' => 'kharbarchi_api_upsert_package_type',
        'permission_callback' => 'kharbarchi_import_api_can_manage',
    ]);

    register_rest_route('khb/v1', '/product/link', [
        'methods' => WP_REST_Server::CREATABLE,
        'callback' => 'kharbarchi_api_link_product_relations',
        'permission_callback' => 'kharbarchi_import_api_can_manage',
    ]);
}

function kharbarchi_request_string(WP_REST_Request $request, $key, $default = '')
{
    $value = $request->get_param($key);
    if ($value === null || $value === '') {
        return $default;
    }
    return sanitize_text_field(wp_unslash((string) $value));
}

function kharbarchi_request_decimal(WP_REST_Request $request, $key, $default = '')
{
    $value = $request->get_param($key);
    if ($value === null || $value === '') {
        return $default;
    }
    $value = str_replace(',', '', (string) wp_unslash($value));
    return is_numeric($value) ? wc_format_decimal($value) : $default;
}

function kharbarchi_api_upsert_product_category(WP_REST_Request $request)
{
    $name = kharbarchi_request_string($request, 'name');
    $slug = sanitize_title(kharbarchi_request_string($request, 'slug'));

    if ($name === '' || $slug === '') {
        return new WP_Error('invalid_category', 'Category name and slug are required.', ['status' => 400]);
    }

    $existing = get_term_by('slug', $slug, 'product_cat');
    if ($existing) {
        wp_update_term($existing->term_id, 'product_cat', ['name' => $name, 'slug' => $slug]);
        $term_id = (int) $existing->term_id;
    } else {
        $created = wp_insert_term($name, 'product_cat', ['slug' => $slug]);
        if (is_wp_error($created)) {
            return $created;
        }
        $term_id = (int) $created['term_id'];
    }

    update_term_meta($term_id, '_khb_english_name', kharbarchi_request_string($request, 'english_name'));

    return rest_ensure_response(['success' => true, 'id' => $term_id, 'slug' => $slug]);
}

function kharbarchi_api_upsert_commodity(WP_REST_Request $request)
{
    $name = kharbarchi_request_string($request, 'name');
    $slug = sanitize_title(kharbarchi_request_string($request, 'slug'));

    if ($name === '' || $slug === '') {
        return new WP_Error('invalid_commodity', 'Commodity name and slug are required.', ['status' => 400]);
    }

    $existing = get_term_by('slug', $slug, 'commodity');
    if ($existing) {
        wp_update_term($existing->term_id, 'commodity', ['name' => $name, 'slug' => $slug]);
        $term_id = (int) $existing->term_id;
    } else {
        $created = wp_insert_term($name, 'commodity', ['slug' => $slug]);
        if (is_wp_error($created)) {
            return $created;
        }
        $term_id = (int) $created['term_id'];
    }

    update_term_meta($term_id, '_khb_english_name', kharbarchi_request_string($request, 'english_name'));
    update_term_meta($term_id, '_khb_category_slug', kharbarchi_request_string($request, 'category_slug'));
    update_term_meta($term_id, 'base_image_id', absint($request->get_param('base_image_id')));
    update_term_meta($term_id, 'base_gallery_ids', kharbarchi_request_string($request, 'base_gallery_ids'));

    // Final rule: commodity has no kg price source.
    delete_term_meta($term_id, 'price_per_kg');

    return rest_ensure_response(['success' => true, 'id' => $term_id, 'slug' => $slug]);
}

function kharbarchi_api_upsert_package_type(WP_REST_Request $request)
{
    $title = kharbarchi_request_string($request, 'title');
    $code = kharbarchi_request_string($request, 'package_code', kharbarchi_request_string($request, 'slug'));

    if ($title === '' || $code === '') {
        return new WP_Error('invalid_package', 'Package title and package_code are required.', ['status' => 400]);
    }

    $existing = get_posts([
        'post_type' => 'kharbarchi_package',
        'post_status' => ['publish', 'draft', 'private'],
        'posts_per_page' => 1,
        'meta_key' => '_khb_package_code',
        'meta_value' => $code,
        'fields' => 'ids',
    ]);

    $postarr = [
        'post_title' => $title,
        'post_type' => 'kharbarchi_package',
        'post_status' => 'publish',
    ];

    if (!empty($existing)) {
        $postarr['ID'] = (int) $existing[0];
        $post_id = wp_update_post($postarr, true);
    } else {
        $post_id = wp_insert_post($postarr, true);
    }

    if (is_wp_error($post_id)) {
        return $post_id;
    }

    $group = kharbarchi_request_string($request, 'package_group');
    $unit_weight = kharbarchi_request_decimal($request, 'unit_weight', '0');
    $default_carton_count = absint($request->get_param('default_carton_count'));
    if ($default_carton_count <= 0) {
        $default_carton_count = absint($request->get_param('carton_count'));
    }
    $image_tag = kharbarchi_request_string($request, 'image_tag');

    update_post_meta($post_id, '_khb_package_code', $code);
    update_post_meta($post_id, '_khb_package_group', $group);
    update_post_meta($post_id, '_khb_unit_weight', $unit_weight);
    update_post_meta($post_id, '_khb_default_carton_count', $default_carton_count);
    update_post_meta($post_id, '_khb_image_tag', $image_tag);

    // Legacy mirrors for compatibility; not used for price calculation.
    update_post_meta($post_id, '_khb_carton_count', $default_carton_count);
    update_post_meta($post_id, '_khb_apply_packing_cost', 0);

    return rest_ensure_response([
        'success' => true,
        'id' => (int) $post_id,
        'package_code' => $code,
        'default_carton_count' => $default_carton_count,
    ]);
}

function kharbarchi_api_link_product_relations(WP_REST_Request $request)
{
    $product_id = absint($request->get_param('product_id'));
    if (!$product_id || get_post_type($product_id) !== 'product') {
        return new WP_Error('invalid_product', 'Valid WooCommerce product_id is required.', ['status' => 400]);
    }

    $category_slug = sanitize_title(kharbarchi_request_string($request, 'category_slug'));
    $commodity_slug = sanitize_title(kharbarchi_request_string($request, 'commodity_slug'));
    $package_code = kharbarchi_request_string($request, 'package_code');

    if ($category_slug !== '') {
        $category = get_term_by('slug', $category_slug, 'product_cat');
        if ($category) {
            wp_set_object_terms($product_id, [(int) $category->term_id], 'product_cat', false);
        }
    }

    if ($commodity_slug !== '') {
        $commodity = get_term_by('slug', $commodity_slug, 'commodity');
        if ($commodity) {
            wp_set_object_terms($product_id, [(int) $commodity->term_id], 'commodity', false);
            update_post_meta($product_id, '_kharbarchi_commodity_id', (int) $commodity->term_id);
        }
    }

    if ($package_code !== '') {
        $package_id = kharbarchi_find_package_id_by_code($package_code);
        if ($package_id > 0) {
            update_post_meta($product_id, '_kharbarchi_package_id', $package_id);
        }
        update_post_meta($product_id, '_khb_package_code', $package_code);
    }

    $meta = $request->get_param('meta');
    if (is_array($meta)) {
        $allowed_prefixes = ['_kharbarchi_', '_khb_'];
        foreach ($meta as $key => $value) {
            $key = sanitize_key((string) $key);
            $allowed = false;
            foreach ($allowed_prefixes as $prefix) {
                if (strpos($key, $prefix) === 0) {
                    $allowed = true;
                    break;
                }
            }
            if (!$allowed) {
                continue;
            }
            update_post_meta($product_id, $key, is_scalar($value) ? sanitize_text_field((string) $value) : wp_json_encode($value));
        }
    }

    $package_group = strtolower((string) get_post_meta($product_id, '_khb_package_group', true));
    update_post_meta($product_id, 'woodmart_price_unit_of_measure', $package_group === 'bulk' ? 'گونی' : 'کارتن');

    if (get_post_meta($product_id, '_khb_price_source_mode', true) === '') {
        update_post_meta($product_id, '_khb_price_source_mode', 'final_price');
    }

    if (function_exists('kharbarchi_recalculate_product_price')) {
        kharbarchi_recalculate_product_price($product_id);
    }

    if (function_exists('kharbarchi_update_price_control_meta')) {
        kharbarchi_update_price_control_meta($product_id);
    }

    return rest_ensure_response(['success' => true, 'product_id' => $product_id]);
}

function kharbarchi_find_package_id_by_code($package_code)
{
    $ids = get_posts([
        'post_type' => 'kharbarchi_package',
        'post_status' => ['publish', 'draft', 'private'],
        'posts_per_page' => 1,
        'meta_key' => '_khb_package_code',
        'meta_value' => $package_code,
        'fields' => 'ids',
    ]);

    return empty($ids) ? 0 : absint($ids[0]);
}
