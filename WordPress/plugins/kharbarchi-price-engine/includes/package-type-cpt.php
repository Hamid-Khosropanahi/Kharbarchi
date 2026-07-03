<?php

if (!defined('ABSPATH')) {
    exit;
}

function kharbarchi_register_package_type_cpt()
{
    register_post_type('kharbarchi_package', [
        'labels' => [
            'name'          => 'انواع بسته‌بندی',
            'singular_name' => 'نوع بسته‌بندی',
            'menu_name'     => 'انواع بسته‌بندی',
            'add_new_item'  => 'افزودن نوع بسته‌بندی',
            'edit_item'     => 'ویرایش نوع بسته‌بندی',
        ],
        'public'          => false,
        'show_ui'         => true,
        'show_in_menu'    => 'edit.php?post_type=product',
        'show_in_rest'    => true,
        'rest_base'       => 'kharbarchi_package',
        'supports'        => ['title', 'custom-fields'],
        'map_meta_cap'    => true,
        'capability_type' => 'post',
    ]);
}
add_action('init', 'kharbarchi_register_package_type_cpt');

function kharbarchi_package_rest_meta_permission()
{
    return current_user_can('edit_products') || current_user_can('manage_woocommerce') || current_user_can('edit_posts');
}

function kharbarchi_sanitize_package_float($value, $meta_key = '', $object_type = '', $object_subtype = '')
{
    if ($value === '' || $value === null) {
        return 0;
    }

    return (float) str_replace(',', '.', (string) $value);
}

function kharbarchi_sanitize_package_integer($value, $meta_key = '', $object_type = '', $object_subtype = '')
{
    if ($value === '' || $value === null) {
        return 0;
    }

    return absint($value);
}

function kharbarchi_register_package_rest_meta()
{
    $string_keys = ['_khb_package_code', '_khb_package_group', '_khb_image_tag'];

    foreach ($string_keys as $meta_key) {
        register_post_meta('kharbarchi_package', $meta_key, [
            'type'              => 'string',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'sanitize_text_field',
            'auth_callback'     => 'kharbarchi_package_rest_meta_permission',
        ]);
    }

    register_post_meta('kharbarchi_package', '_khb_unit_weight', [
        'type'              => 'number',
        'single'            => true,
        'show_in_rest'      => true,
        'sanitize_callback' => 'kharbarchi_sanitize_package_float',
        'auth_callback'     => 'kharbarchi_package_rest_meta_permission',
    ]);

    register_post_meta('kharbarchi_package', '_khb_default_carton_count', [
        'type'              => 'integer',
        'single'            => true,
        'show_in_rest'      => true,
        'sanitize_callback' => 'kharbarchi_sanitize_package_integer',
        'auth_callback'     => 'kharbarchi_package_rest_meta_permission',
    ]);

    // Legacy keys are registered only so old REST payloads do not fail. They are not price drivers.
    register_post_meta('kharbarchi_package', '_khb_apply_packing_cost', [
        'type'              => 'integer',
        'single'            => true,
        'show_in_rest'      => true,
        'sanitize_callback' => 'kharbarchi_sanitize_package_integer',
        'auth_callback'     => 'kharbarchi_package_rest_meta_permission',
    ]);

    register_post_meta('kharbarchi_package', '_khb_carton_count', [
        'type'              => 'integer',
        'single'            => true,
        'show_in_rest'      => true,
        'sanitize_callback' => 'kharbarchi_sanitize_package_integer',
        'auth_callback'     => 'kharbarchi_package_rest_meta_permission',
    ]);
}
add_action('init', 'kharbarchi_register_package_rest_meta', 99);
