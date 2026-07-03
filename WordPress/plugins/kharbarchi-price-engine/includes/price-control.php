<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * Automated price-control layer.
 * The plugin never rebuilds the final product price from kg/weight/carton values.
 * These calculations are used only for validation, reporting, status icons, and publish guard.
 */

add_action('init', 'kharbarchi_register_price_control_meta', 100);
add_action('woocommerce_process_product_meta', 'kharbarchi_update_price_control_meta', 80);
add_action('woocommerce_rest_insert_product_object', 'kharbarchi_update_price_control_after_rest', 40, 3);
add_action('save_post_product', 'kharbarchi_update_price_control_on_product_save', 90, 3);
add_filter('manage_edit-product_columns', 'kharbarchi_add_price_control_admin_column', 30);
add_action('manage_product_posts_custom_column', 'kharbarchi_render_price_control_admin_column', 10, 2);

function kharbarchi_register_price_control_meta()
{
    $decimal_keys = [
        '_kharbarchi_sale_cash_price_per_kg',
        '_kharbarchi_sale_credit_price_per_kg',
        '_kharbarchi_buy_cash_price_per_kg',
        '_kharbarchi_buy_credit_price_per_kg',
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
            'sanitize_callback' => 'kharbarchi_sanitize_price_control_decimal_meta',
            'auth_callback'     => 'kharbarchi_price_control_meta_permission',
        ]);
    }

    $string_keys = [
        '_khb_price_source_mode',
        '_khb_price_check_status',
        '_khb_price_check_code',
        '_khb_price_check_note',
    ];

    foreach ($string_keys as $key) {
        register_post_meta('product', $key, [
            'type'              => 'string',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'sanitize_text_field',
            'auth_callback'     => 'kharbarchi_price_control_meta_permission',
        ]);
    }
}

function kharbarchi_price_control_meta_permission()
{
    return current_user_can('edit_products') || current_user_can('manage_woocommerce');
}

function kharbarchi_sanitize_price_control_decimal_meta($value, $meta_key = '', $object_type = '', $object_subtype = '')
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

function kharbarchi_price_control_float($value)
{
    if ($value === '' || $value === null) {
        return 0.0;
    }

    $value = str_replace(',', '', (string) $value);
    $value = str_replace('٫', '.', $value);

    if (!is_numeric($value)) {
        return 0.0;
    }

    return (float) $value;
}

function kharbarchi_price_control_meta_float($product_id, $key)
{
    return kharbarchi_price_control_float(get_post_meta($product_id, $key, true));
}

function kharbarchi_price_control_format_decimal($value, $precision = 4)
{
    if ($value === null || $value === '') {
        return '';
    }

    $formatted = wc_format_decimal((float) $value, $precision);
    $formatted = rtrim(rtrim($formatted, '0'), '.');

    return $formatted === '-0' ? '0' : $formatted;
}

function kharbarchi_price_control_status_rank($status)
{
    if ($status === 'red') {
        return 3;
    }
    if ($status === 'yellow') {
        return 2;
    }
    return 1;
}

function kharbarchi_price_control_add_issue(&$issues, $status, $code, $note)
{
    $issues[] = [
        'status' => $status,
        'code'   => $code,
        'note'   => $note,
    ];
}

function kharbarchi_price_control_get_total_weight_context($product_id)
{
    $group = strtolower(trim((string) get_post_meta($product_id, '_khb_package_group', true)));
    $code = strtoupper(trim((string) get_post_meta($product_id, '_khb_package_code', true)));
    $unit_weight = kharbarchi_price_control_meta_float($product_id, '_khb_unit_weight');
    $carton_count = absint(get_post_meta($product_id, '_khb_product_carton_count', true));
    $bulk_weight = kharbarchi_price_control_meta_float($product_id, '_khb_bulk_weight_kg');
    $min_purchase = kharbarchi_price_control_meta_float($product_id, '_khb_min_purchase_kg');

    $is_retail = $group === 'retail' || in_array($code, ['450', '900', '450G', '900G'], true);
    $is_bulk = $group === 'bulk' || strpos($code, 'B') === 0;

    $weight = 0.0;
    $unit_label = 'کارتن';
    $qty_label = '';
    $issues = [];

    if ($is_retail) {
        $unit_label = 'کارتن';

        if ($unit_weight <= 0) {
            kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_UNIT_WEIGHT', 'وزن واحد بسته‌بندی خالی است.');
        }

        if ($carton_count <= 0) {
            kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_CARTON_COUNT', 'تعداد بسته در کارتن خالی است.');
        }

        if ($unit_weight > 0 && $carton_count > 0) {
            $weight = $unit_weight * $carton_count;
            $qty_label = $carton_count . ' بسته';
        }
    } elseif ($is_bulk) {
        $unit_label = 'گونی';

        if ($bulk_weight > 0) {
            $weight = $bulk_weight;
        } elseif ($unit_weight > 0) {
            $weight = $unit_weight;
        } elseif ($min_purchase > 0) {
            $weight = $min_purchase;
        }

        if ($weight <= 0) {
            kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_BULK_WEIGHT', 'وزن فروش فله خالی است.');
        }

        if ($min_purchase <= 0) {
            kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_MIN_PURCHASE', 'حداقل خرید کیلویی برای کالای فله خالی است.');
        }

        if ($weight > 0) {
            $qty_label = kharbarchi_price_control_format_decimal($weight, 3) . ' کیلو';
        }
    } else {
        $unit_label = 'واحد';

        if ($unit_weight > 0 && $carton_count > 0) {
            $weight = $unit_weight * $carton_count;
            $qty_label = $carton_count . ' بسته';
        } elseif ($bulk_weight > 0) {
            $weight = $bulk_weight;
            $qty_label = kharbarchi_price_control_format_decimal($bulk_weight, 3) . ' کیلو';
        } elseif ($unit_weight > 0) {
            $weight = $unit_weight;
            $qty_label = kharbarchi_price_control_format_decimal($unit_weight, 3) . ' کیلو';
        } else {
            kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_WEIGHT', 'وزن کالا خالی است.');
        }
    }

    return [
        'package_group' => $group,
        'package_code'  => $code,
        'total_weight'  => $weight,
        'unit_label'    => $unit_label,
        'qty_label'     => $qty_label,
        'issues'        => $issues,
    ];
}

function kharbarchi_price_control_compare_pair(&$issues, &$max_abs_diff, &$max_percent_diff, $actual, $expected, $diff_meta_key, $label)
{
    if ($expected <= 0 || $actual <= 0) {
        return '';
    }

    $diff = $actual - $expected;
    $abs_diff = abs($diff);
    $percent = $expected > 0 ? ($abs_diff / $expected) * 100 : 0;

    $max_abs_diff = max($max_abs_diff, $abs_diff);
    $max_percent_diff = max($max_percent_diff, $percent);

    $green_tolerance = max(5000, $expected * 0.005);
    $yellow_tolerance = max(20000, $expected * 0.02);

    if ($abs_diff <= $green_tolerance) {
        return kharbarchi_price_control_format_decimal($diff, 4);
    }

    if ($abs_diff <= $yellow_tolerance) {
        kharbarchi_price_control_add_issue(
            $issues,
            'yellow',
            'MINOR_PRICE_DIFF',
            $label . ' با مقدار مورد انتظار اختلاف قابل بررسی دارد.'
        );
        return kharbarchi_price_control_format_decimal($diff, 4);
    }

    kharbarchi_price_control_add_issue(
        $issues,
        'red',
        'MAJOR_PRICE_DIFF',
        $label . ' با مقدار مورد انتظار اختلاف جدی دارد.'
    );

    return kharbarchi_price_control_format_decimal($diff, 4);
}

function kharbarchi_update_price_control_after_rest($product, $request, $creating)
{
    if ($product && is_a($product, 'WC_Product')) {
        kharbarchi_update_price_control_meta($product->get_id());
    }
}

function kharbarchi_update_price_control_on_product_save($post_id, $post, $update)
{
    if (defined('DOING_AUTOSAVE') && DOING_AUTOSAVE) {
        return;
    }

    if (wp_is_post_revision($post_id)) {
        return;
    }

    kharbarchi_update_price_control_meta($post_id);
}

function kharbarchi_update_price_control_meta($product_id)
{
    static $running = false;

    if ($running || get_post_type($product_id) !== 'product') {
        return false;
    }

    $running = true;

    $issues = [];
    $max_abs_diff = 0.0;
    $max_percent_diff = 0.0;

    $sale_credit = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_sale_credit_price');
    $sale_cash = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_sale_cash_price');
    $buy_credit = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_buy_credit_price');
    $buy_cash = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_buy_cash_price');

    $sale_credit_per_kg = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_sale_credit_price_per_kg');
    $sale_cash_per_kg = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_sale_cash_price_per_kg');
    $buy_credit_per_kg = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_buy_credit_price_per_kg');
    $buy_cash_per_kg = kharbarchi_price_control_meta_float($product_id, '_kharbarchi_buy_cash_price_per_kg');

    if ($sale_credit <= 0) {
        kharbarchi_price_control_add_issue($issues, 'red', 'MISSING_SALE_CREDIT_PRICE', 'قیمت فروش شرایطی نهایی محصول خالی است.');
    }

    if ($sale_cash <= 0) {
        kharbarchi_price_control_add_issue($issues, 'yellow', 'MISSING_SALE_CASH_PRICE', 'قیمت فروش نقدی محصول خالی است.');
    }

    if ($buy_credit <= 0) {
        kharbarchi_price_control_add_issue($issues, 'yellow', 'MISSING_BUY_CREDIT_PRICE', 'قیمت خرید شرایطی محصول خالی است.');
    }

    if ($buy_cash <= 0) {
        kharbarchi_price_control_add_issue($issues, 'yellow', 'MISSING_BUY_CASH_PRICE', 'قیمت خرید نقدی محصول خالی است.');
    }

    if ($sale_cash > 0 && $sale_credit > 0 && $sale_cash > $sale_credit) {
        kharbarchi_price_control_add_issue($issues, 'red', 'CASH_GREATER_THAN_CREDIT', 'قیمت فروش نقدی از قیمت فروش شرایطی بیشتر است.');
    }

    if ($buy_cash > 0 && $buy_credit > 0 && $buy_cash > $buy_credit) {
        kharbarchi_price_control_add_issue($issues, 'red', 'CASH_GREATER_THAN_CREDIT', 'قیمت خرید نقدی از قیمت خرید شرایطی بیشتر است.');
    }

    if ($sale_cash_per_kg > 0 && $sale_credit_per_kg > 0 && $sale_cash_per_kg > $sale_credit_per_kg) {
        kharbarchi_price_control_add_issue($issues, 'red', 'CASH_GREATER_THAN_CREDIT', 'قیمت فروش کیلویی نقدی از شرایطی بیشتر است.');
    }

    $weight_context = kharbarchi_price_control_get_total_weight_context($product_id);
    foreach ($weight_context['issues'] as $issue) {
        $issues[] = $issue;
    }

    $total_weight = (float) $weight_context['total_weight'];

    $expected_sale_credit = $sale_credit_per_kg > 0 && $total_weight > 0 ? $sale_credit_per_kg * $total_weight : 0;
    $expected_sale_cash = $sale_cash_per_kg > 0 && $total_weight > 0 ? $sale_cash_per_kg * $total_weight : 0;
    $expected_buy_credit = $buy_credit_per_kg > 0 && $total_weight > 0 ? $buy_credit_per_kg * $total_weight : 0;
    $expected_buy_cash = $buy_cash_per_kg > 0 && $total_weight > 0 ? $buy_cash_per_kg * $total_weight : 0;

    if ($total_weight > 0 && $sale_credit_per_kg <= 0) {
        kharbarchi_price_control_add_issue($issues, 'yellow', 'PER_KG_PRICE_MISMATCH', 'قیمت فروش کیلویی شرایطی برای کنترل وارد نشده است.');
    }

    if ($total_weight > 0 && $sale_cash > 0 && $sale_cash_per_kg <= 0) {
        kharbarchi_price_control_add_issue($issues, 'yellow', 'PER_KG_PRICE_MISMATCH', 'قیمت فروش کیلویی نقدی برای کنترل وارد نشده است.');
    }

    $sale_credit_diff = kharbarchi_price_control_compare_pair($issues, $max_abs_diff, $max_percent_diff, $sale_credit, $expected_sale_credit, '_khb_sale_credit_diff', 'قیمت فروش شرایطی');
    $sale_cash_diff = kharbarchi_price_control_compare_pair($issues, $max_abs_diff, $max_percent_diff, $sale_cash, $expected_sale_cash, '_khb_sale_cash_diff', 'قیمت فروش نقدی');
    $buy_credit_diff = kharbarchi_price_control_compare_pair($issues, $max_abs_diff, $max_percent_diff, $buy_credit, $expected_buy_credit, '_khb_buy_credit_diff', 'قیمت خرید شرایطی');
    $buy_cash_diff = kharbarchi_price_control_compare_pair($issues, $max_abs_diff, $max_percent_diff, $buy_cash, $expected_buy_cash, '_khb_buy_cash_diff', 'قیمت خرید نقدی');

    $status = 'green';
    $code = 'OK';
    $notes = [];

    foreach ($issues as $issue) {
        if (kharbarchi_price_control_status_rank($issue['status']) > kharbarchi_price_control_status_rank($status)) {
            $status = $issue['status'];
            $code = $issue['code'];
        }
        $notes[] = $issue['code'] . ': ' . $issue['note'];
    }

    if ($status === 'green' && $max_abs_diff > 0) {
        $code = 'ROUNDING_DIFF';
        $notes[] = 'ROUNDING_DIFF: اختلاف فقط در محدوده گرد کردن مجاز است.';
    }

    $note = implode(' | ', array_unique($notes));

    update_post_meta($product_id, '_khb_expected_sale_credit_price', kharbarchi_price_control_format_decimal($expected_sale_credit, 4));
    update_post_meta($product_id, '_khb_expected_sale_cash_price', kharbarchi_price_control_format_decimal($expected_sale_cash, 4));
    update_post_meta($product_id, '_khb_expected_buy_credit_price', kharbarchi_price_control_format_decimal($expected_buy_credit, 4));
    update_post_meta($product_id, '_khb_expected_buy_cash_price', kharbarchi_price_control_format_decimal($expected_buy_cash, 4));
    update_post_meta($product_id, '_khb_sale_credit_diff', $sale_credit_diff);
    update_post_meta($product_id, '_khb_sale_cash_diff', $sale_cash_diff);
    update_post_meta($product_id, '_khb_buy_credit_diff', $buy_credit_diff);
    update_post_meta($product_id, '_khb_buy_cash_diff', $buy_cash_diff);
    update_post_meta($product_id, '_khb_price_check_status', $status);
    update_post_meta($product_id, '_khb_price_check_code', $code);
    update_post_meta($product_id, '_khb_price_check_note', $note);
    update_post_meta($product_id, '_khb_price_check_percent', kharbarchi_price_control_format_decimal($max_percent_diff, 4));
    update_post_meta($product_id, '_khb_price_check_amount', kharbarchi_price_control_format_decimal($max_abs_diff, 4));

    if ($status === 'red') {
        update_post_meta($product_id, '_khb_need_fix', 1);
        update_post_meta($product_id, '_khb_fix_note', $note !== '' ? $note : 'محصول دارای خطای کنترلی قرمز است.');
    } else {
        update_post_meta($product_id, '_khb_need_fix', 0);
    }

    kharbarchi_price_control_apply_publish_guard($product_id, $status);

    $running = false;

    return [
        'status' => $status,
        'code'   => $code,
        'note'   => $note,
    ];
}

function kharbarchi_price_control_apply_publish_guard($product_id, $status)
{
    static $guard_running = false;

    if ($guard_running || $status !== 'red') {
        return;
    }

    $current_status = get_post_status($product_id);
    if (!in_array($current_status, ['publish', 'future', 'pending'], true)) {
        return;
    }

    $guard_running = true;
    wp_update_post([
        'ID'          => $product_id,
        'post_status' => 'draft',
    ]);
    $guard_running = false;
}

function kharbarchi_get_price_control_badge_html($product_id)
{
    $status = get_post_meta($product_id, '_khb_price_check_status', true);
    $code = get_post_meta($product_id, '_khb_price_check_code', true);
    $note = get_post_meta($product_id, '_khb_price_check_note', true);

    if ($status === '') {
        $status = 'yellow';
        $code = 'NEED_FIX';
        $note = 'وضعیت کنترل هنوز محاسبه نشده است.';
    }

    $labels = [
        'green'  => 'سبز',
        'yellow' => 'زرد',
        'red'    => 'قرمز',
    ];

    $icons = [
        'green'  => '●',
        'yellow' => '●',
        'red'    => '●',
    ];

    $label = isset($labels[$status]) ? $labels[$status] : 'نامشخص';
    $icon = isset($icons[$status]) ? $icons[$status] : '●';

    return sprintf(
        '<span class="khb-price-check-badge khb-price-check-%1$s" title="%4$s"><span class="khb-price-check-dot">%2$s</span> %3$s <code>%5$s</code></span>',
        esc_attr($status),
        esc_html($icon),
        esc_html($label),
        esc_attr($note),
        esc_html($code)
    );
}

function kharbarchi_add_price_control_admin_column($columns)
{
    $new_columns = [];
    foreach ($columns as $key => $label) {
        $new_columns[$key] = $label;
        if ($key === 'price') {
            $new_columns['khb_price_check'] = 'کنترل قیمت';
        }
    }
    return $new_columns;
}

function kharbarchi_render_price_control_admin_column($column, $post_id)
{
    if ($column !== 'khb_price_check') {
        return;
    }

    echo kharbarchi_get_price_control_badge_html($post_id); // phpcs:ignore WordPress.Security.EscapeOutput.OutputNotEscaped
}
