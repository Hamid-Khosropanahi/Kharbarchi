<?php
/*
Plugin Name: Kharbarchi Price Engine
Plugin URI: https://www.Kharbarchi.ir/
Description: Direct Price Engine With Automated Price Control And Gateway Payment Mapping For Kharbarchi WooCommerce
Version: 1.3.0-payment-map
Author: Hamid Khosropanahi
Author URI: https://www.Kharbarchi.ir/
Text Domain: kharbarchi-price-engine
*/

if (!defined('ABSPATH')) {
    exit;
}

define('KHARBARCHI_PLUGIN_PATH', plugin_dir_path(__FILE__));
define('KHARBARCHI_PLUGIN_URL', plugin_dir_url(__FILE__));

add_action('plugins_loaded', 'kharbarchi_price_engine_bootstrap');

function kharbarchi_price_engine_bootstrap()
{
    if (!class_exists('WooCommerce')) {
        add_action('admin_notices', 'kharbarchi_woocommerce_missing_notice');
        return;
    }

    require_once KHARBARCHI_PLUGIN_PATH . 'includes/bulk-update.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/commodity-meta.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/commodity-taxonomy.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/frontend-display.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/package-type-cpt.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/package-type-meta.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/product-meta.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/price-control.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/price-calculator.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/settings-page.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/cart-validation.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/payment-pricing.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/rest-api-sync.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/api-endpoints.php';
    require_once KHARBARCHI_PLUGIN_PATH . 'includes/api-upsert-endpoints.php';

}

function kharbarchi_woocommerce_missing_notice()
{
    echo '<div class="notice notice-error"><p>';
    echo 'افزونه Kharbarchi Price Engine نیاز دارد ووکامرس فعال باشد.';
    echo '</p></div>';
}

add_action('admin_enqueue_scripts', 'kharbarchi_admin_assets');

function kharbarchi_admin_assets($hook)
{
    wp_enqueue_media();

    wp_enqueue_style(
        'kharbarchi-admin-css',
        KHARBARCHI_PLUGIN_URL . 'assets/css/admin.css',
        [],
        '1.0.0'
    );

    wp_enqueue_script(
        'kharbarchi-admin-js',
        KHARBARCHI_PLUGIN_URL . 'assets/js/admin.js',
        ['jquery'],
        '1.0.0',
        true
    );
}

add_action('wp_enqueue_scripts', 'kharbarchi_frontend_assets');

function kharbarchi_frontend_assets()
{
    wp_enqueue_style(
        'kharbarchi-frontend-css',
        KHARBARCHI_PLUGIN_URL . 'assets/css/frontend.css',
        [],
        '1.0.0'
    );
}
add_action('admin_head', 'kharbarchi_hide_default_wc_price_fields');

function kharbarchi_hide_default_wc_price_fields()
{
    $screen = get_current_screen();

    if (
        !$screen ||
        empty($screen->post_type) ||
        $screen->post_type !== 'product'
    ) {
        return;
    }

    ?>
    <style>
        .options_group.pricing {
            display: none !important;
        }
    </style>
    <?php
}
