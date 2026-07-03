<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('init', 'kharbarchi_register_product_rest_meta', 99);

function kharbarchi_product_meta_permission()
{
    return current_user_can('edit_products') || current_user_can('manage_woocommerce');
}

function kharbarchi_sanitize_decimal_meta($value, $meta_key = '', $object_type = '', $object_subtype = '')
{
    if ($value === '' || $value === null) {
        return '';
    }

    $value = str_replace(',', '', (string) $value);
    $value = str_replace('٫', '.', $value);

    if (!is_numeric($value)) {
        return '';
    }

    return wc_format_decimal($value, 4);
}

function kharbarchi_sanitize_int_meta($value, $meta_key = '', $object_type = '', $object_subtype = '')
{
    if ($value === '' || $value === null) {
        return 0;
    }

    return absint($value);
}

function kharbarchi_register_product_rest_meta()
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
        '_khb_expected_sale_credit_price',
        '_khb_expected_sale_cash_price',
        '_khb_expected_buy_credit_price',
        '_khb_expected_buy_cash_price',
        '_khb_sale_credit_diff',
        '_khb_sale_cash_diff',
        '_khb_buy_credit_diff',
        '_khb_buy_cash_diff',
        '_khb_price_check_percent',
        '_khb_price_check_amount',
    ];

    foreach ($decimal_keys as $key) {
        register_post_meta('product', $key, [
            'type'              => 'string',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'kharbarchi_sanitize_decimal_meta',
            'auth_callback'     => 'kharbarchi_product_meta_permission',
        ]);
    }

    $int_keys = [
        '_kharbarchi_package_id',
        '_kharbarchi_min_cartons',
        '_kharbarchi_max_cartons',
        '_kharbarchi_carton_step',
        '_kharbarchi_commodity_id',
        '_khb_product_carton_count',
        '_khb_source_id',
        '_khb_source_row_number',
        '_khb_need_fix',
    ];

    foreach ($int_keys as $key) {
        register_post_meta('product', $key, [
            'type'              => 'integer',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'kharbarchi_sanitize_int_meta',
            'auth_callback'     => 'kharbarchi_product_meta_permission',
        ]);
    }

    $string_keys = [
        '_khb_package_code',
        '_khb_package_title',
        '_khb_package_group',
        '_khb_image_tag',
        '_khb_fix_note',
        '_khb_price_source_mode',
        '_khb_price_check_status',
        '_khb_price_check_code',
        '_khb_price_check_note',
        '_kharbarchi_brand_name',
        '_kharbarchi_brand_english_name',
        '_kharbarchi_commodity_name',
        '_kharbarchi_commodity_slug',
        'woodmart_price_unit_of_measure',
    ];

    foreach ($string_keys as $key) {
        register_post_meta('product', $key, [
            'type'              => 'string',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'sanitize_text_field',
            'auth_callback'     => 'kharbarchi_product_meta_permission',
        ]);
    }
}

add_action('woocommerce_product_options_general_product_data', 'kharbarchi_add_product_fields');

function kharbarchi_add_product_fields()
{
    if (!function_exists('woocommerce_wp_select') || !function_exists('woocommerce_wp_text_input')) {
        return;
    }

    global $post;
    $product_id = $post ? $post->ID : 0;

    echo '<div class="options_group khb-admin-price-check-group">';
    echo '<p class="form-field"><strong>وضعیت کنترل قیمت:</strong> ';
    if ($product_id && function_exists('kharbarchi_get_price_control_badge_html')) {
        echo kharbarchi_get_price_control_badge_html($product_id); // phpcs:ignore WordPress.Security.EscapeOutput.OutputNotEscaped
    } else {
        echo '<span class="khb-price-check-badge khb-price-check-yellow">● زرد <code>NEED_FIX</code></span>';
    }
    echo '</p>';

    if ($product_id) {
        $expected_credit = get_post_meta($product_id, '_khb_expected_sale_credit_price', true);
        $expected_cash = get_post_meta($product_id, '_khb_expected_sale_cash_price', true);
        $diff_amount = get_post_meta($product_id, '_khb_price_check_amount', true);
        $diff_percent = get_post_meta($product_id, '_khb_price_check_percent', true);
        $note = get_post_meta($product_id, '_khb_price_check_note', true);
        echo '<p class="form-field khb-price-check-readonly"><span>مورد انتظار شرایطی: ' . esc_html($expected_credit) . '</span> | <span>مورد انتظار نقدی: ' . esc_html($expected_cash) . '</span> | <span>اختلاف: ' . esc_html($diff_amount) . ' / ' . esc_html($diff_percent) . '%</span></p>';
        if ($note !== '') {
            echo '<p class="form-field khb-price-check-note"><span>' . esc_html($note) . '</span></p>';
        }
    }
    echo '</div>';

    echo '<div class="options_group">';

    woocommerce_wp_select([
        'id'          => '_kharbarchi_package_id',
        'label'       => 'نوع بسته‌بندی خواربارچی',
        'description' => 'هر quantity در ووکامرس یعنی تعداد کارتن/واحد فروش نهایی.',
        'desc_tip'    => true,
        'options'     => kharbarchi_get_package_options(),
    ]);

    woocommerce_wp_select([
        'id'          => '_khb_price_source_mode',
        'label'       => 'منبع کنترل قیمت',
        'description' => 'این گزینه فقط برای کنترل اختلاف استفاده می‌شود، نه محاسبه قیمت ووکامرس.',
        'desc_tip'    => true,
        'options'     => [
            'final_price'     => 'قیمت نهایی محصول منبع اصلی است',
            'per_kg_price'    => 'قیمت کیلویی منبع کنترل اصلی است',
            'manual_override' => 'اصلاح دستی/نیازمند بررسی',
        ],
    ]);

    woocommerce_wp_text_input(['id' => '_khb_package_code', 'label' => 'کد بسته‌بندی', 'type' => 'text']);
    woocommerce_wp_text_input(['id' => '_khb_package_title', 'label' => 'عنوان بسته‌بندی', 'type' => 'text']);
    woocommerce_wp_text_input(['id' => '_khb_package_group', 'label' => 'گروه بسته‌بندی bulk/retail/none', 'type' => 'text']);
    woocommerce_wp_text_input(['id' => '_khb_unit_weight', 'label' => 'وزن واحد بسته/کالا - کیلو', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '0.001']]);
    woocommerce_wp_text_input(['id' => '_khb_product_carton_count', 'label' => 'تعداد بسته در کارتن همین محصول', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_khb_bulk_weight_kg', 'label' => 'وزن فروش فله - کیلو', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '0.001']]);
    woocommerce_wp_text_input(['id' => '_khb_min_purchase_kg', 'label' => 'حداقل خرید کیلویی', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '0.001']]);
    woocommerce_wp_text_input(['id' => '_khb_image_tag', 'label' => 'تگ تصویر', 'type' => 'text']);

    echo '</div>';

    echo '<div class="options_group">';

    woocommerce_wp_text_input(['id' => '_kharbarchi_sale_credit_price', 'label' => 'قیمت فروش کالا / شرایطی نهایی', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_kharbarchi_sale_cash_price', 'label' => 'قیمت نقدی پس از تخفیف تسویه', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_kharbarchi_sale_credit_price_per_kg', 'label' => 'قیمت فروش کیلویی شرایطی - فقط کنترل/نمایش', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_kharbarchi_sale_cash_price_per_kg', 'label' => 'قیمت فروش کیلویی نقدی - فقط کنترل/نمایش', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);

    if (current_user_can('manage_woocommerce')) {
        woocommerce_wp_text_input(['id' => '_kharbarchi_buy_credit_price', 'label' => 'قیمت خرید شرایطی - فقط مدیر', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
        woocommerce_wp_text_input(['id' => '_kharbarchi_buy_cash_price', 'label' => 'قیمت خرید نقد - فقط مدیر', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
        woocommerce_wp_text_input(['id' => '_kharbarchi_buy_credit_price_per_kg', 'label' => 'قیمت خرید کیلویی شرایطی - فقط کنترل/گزارش', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
        woocommerce_wp_text_input(['id' => '_kharbarchi_buy_cash_price_per_kg', 'label' => 'قیمت خرید کیلویی نقد - فقط کنترل/گزارش', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    }

    echo '</div>';

    echo '<div class="options_group">';
    woocommerce_wp_text_input(['id' => '_kharbarchi_brand_name', 'label' => 'برند', 'type' => 'text']);
    woocommerce_wp_text_input(['id' => '_kharbarchi_brand_english_name', 'label' => 'برند انگلیسی', 'type' => 'text']);
    woocommerce_wp_text_input(['id' => '_kharbarchi_min_cartons', 'label' => 'حداقل خرید کارتن/واحد فروش', 'type' => 'number', 'custom_attributes' => ['min' => '1', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_kharbarchi_max_cartons', 'label' => 'حداکثر خرید کارتن/واحد فروش', 'type' => 'number', 'custom_attributes' => ['min' => '0', 'step' => '1']]);
    woocommerce_wp_text_input(['id' => '_kharbarchi_carton_step', 'label' => 'گام خرید کارتن/واحد فروش', 'type' => 'number', 'custom_attributes' => ['min' => '1', 'step' => '1']]);
    echo '</div>';

    echo '<div class="options_group"><p class="form-field"><strong>قانون نهایی:</strong> قیمت ووکامرس از قیمت فروش شرایطی نهایی می‌آید. وزن، تعداد در کارتن و قیمت کیلویی فقط برای کنترل خودکار، گزارش و نمایش استفاده می‌شوند.</p></div>';
}

function kharbarchi_get_package_options()
{
    $options = ['' => 'انتخاب کنید'];

    $packages = get_posts([
        'post_type'      => 'kharbarchi_package',
        'post_status'    => ['publish', 'draft', 'private'],
        'posts_per_page' => -1,
        'orderby'        => 'title',
        'order'          => 'ASC',
    ]);

    foreach ($packages as $package) {
        $options[$package->ID] = $package->post_title;
    }

    return $options;
}

add_action('woocommerce_process_product_meta', 'kharbarchi_save_product_fields');

function kharbarchi_save_product_fields($product_id)
{
    if (!current_user_can('edit_post', $product_id)) {
        return;
    }

    $int_keys = [
        '_kharbarchi_package_id' => 0,
        '_kharbarchi_min_cartons' => 1,
        '_kharbarchi_max_cartons' => 0,
        '_kharbarchi_carton_step' => 1,
        '_khb_product_carton_count' => 0,
    ];

    foreach ($int_keys as $key => $default) {
        $value = isset($_POST[$key]) ? absint(wp_unslash($_POST[$key])) : $default;
        if (in_array($key, ['_kharbarchi_min_cartons', '_kharbarchi_carton_step'], true) && $value <= 0) {
            $value = 1;
        }
        update_post_meta($product_id, $key, $value);
    }

    $decimal_keys = [
        '_kharbarchi_sale_cash_price',
        '_kharbarchi_sale_credit_price',
        '_kharbarchi_sale_cash_price_per_kg',
        '_kharbarchi_sale_credit_price_per_kg',
        '_khb_unit_weight',
        '_khb_bulk_weight_kg',
        '_khb_min_purchase_kg',
    ];

    if (current_user_can('manage_woocommerce')) {
        $decimal_keys[] = '_kharbarchi_buy_cash_price';
        $decimal_keys[] = '_kharbarchi_buy_credit_price';
        $decimal_keys[] = '_kharbarchi_buy_cash_price_per_kg';
        $decimal_keys[] = '_kharbarchi_buy_credit_price_per_kg';
    }

    foreach ($decimal_keys as $key) {
        $value = isset($_POST[$key]) ? kharbarchi_sanitize_decimal_meta(wp_unslash($_POST[$key])) : '';
        update_post_meta($product_id, $key, $value);
    }

    $string_keys = [
        '_khb_package_code',
        '_khb_package_title',
        '_khb_package_group',
        '_khb_image_tag',
        '_khb_price_source_mode',
        '_kharbarchi_brand_name',
        '_kharbarchi_brand_english_name',
    ];

    foreach ($string_keys as $key) {
        $value = isset($_POST[$key]) ? sanitize_text_field(wp_unslash($_POST[$key])) : '';
        update_post_meta($product_id, $key, $value);
    }

    if (get_post_meta($product_id, '_khb_price_source_mode', true) === '') {
        update_post_meta($product_id, '_khb_price_source_mode', 'final_price');
    }

    $unit_measure = strtolower((string) get_post_meta($product_id, '_khb_package_group', true)) === 'bulk' ? 'گونی' : 'کارتن';
    update_post_meta($product_id, 'woodmart_price_unit_of_measure', $unit_measure);

    if (function_exists('kharbarchi_recalculate_product_price')) {
        kharbarchi_recalculate_product_price($product_id);
    }

    if (function_exists('kharbarchi_update_price_control_meta')) {
        kharbarchi_update_price_control_meta($product_id);
    }
}
