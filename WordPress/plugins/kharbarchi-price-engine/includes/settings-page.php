<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('admin_menu', 'kharbarchi_register_settings_page');
add_action('admin_init', 'kharbarchi_register_settings');

function kharbarchi_register_settings_page()
{
    add_submenu_page(
        'edit.php?post_type=product',
        'تنظیمات خواربارچی',
        'تنظیمات خواربارچی',
        'manage_woocommerce',
        'kharbarchi-settings',
        'kharbarchi_settings_page_html'
    );
}

function kharbarchi_register_settings()
{
    register_setting(
        'kharbarchi_settings',
        'kharbarchi_cash_gateway_ids',
        [
            'type' => 'array',
            'sanitize_callback' => 'kharbarchi_sanitize_gateway_id_array',
            'default' => [],
        ]
    );

    register_setting(
        'kharbarchi_settings',
        'kharbarchi_credit_gateway_ids',
        [
            'type' => 'array',
            'sanitize_callback' => 'kharbarchi_sanitize_gateway_id_array',
            'default' => [],
        ]
    );

    register_setting(
        'kharbarchi_settings',
        'kharbarchi_default_payment_type',
        [
            'type' => 'string',
            'sanitize_callback' => 'kharbarchi_sanitize_default_payment_type',
            'default' => 'credit',
        ]
    );
}

function kharbarchi_sanitize_gateway_id_array($value)
{
    if (!is_array($value)) {
        return [];
    }

    $sanitized = [];

    foreach ($value as $gateway_id) {
        $gateway_id = sanitize_key((string) $gateway_id);

        if ($gateway_id !== '') {
            $sanitized[] = $gateway_id;
        }
    }

    return array_values(array_unique($sanitized));
}

function kharbarchi_sanitize_default_payment_type($value)
{
    $value = sanitize_key((string) $value);

    return in_array($value, ['cash', 'credit'], true) ? $value : 'credit';
}

function kharbarchi_get_available_payment_gateways_for_settings()
{
    if (!function_exists('WC') || !WC()->payment_gateways()) {
        return [];
    }

    $gateways = WC()->payment_gateways()->payment_gateways();

    if (!is_array($gateways)) {
        return [];
    }

    return $gateways;
}

function kharbarchi_settings_page_html()
{
    if (!current_user_can('manage_woocommerce')) {
        wp_die('دسترسی غیرمجاز');
    }

    if (isset($_GET['kharbarchi_updated'])) {
        $updated_count = absint(wp_unslash($_GET['kharbarchi_updated']));
        echo '<div class="notice notice-success"><p>' . esc_html($updated_count) . ' محصول با قیمت مستقیم SQL همسان‌سازی شد.</p></div>';
    }

    $gateways = kharbarchi_get_available_payment_gateways_for_settings();
    $cash_gateways = (array) get_option('kharbarchi_cash_gateway_ids', []);
    $credit_gateways = (array) get_option('kharbarchi_credit_gateway_ids', []);
    $default_payment_type = get_option('kharbarchi_default_payment_type', 'credit');
    ?>
    <div class="wrap">
        <h1>تنظیمات خواربارچی</h1>
        <p><strong>قانون نهایی قیمت:</strong> قیمت شرایطی قیمت رسمی ووکامرس است. نقدی فقط تخفیف تسویه سریع است. قیمت خرید فقط به‌صورت متای مخفی سفارش ذخیره می‌شود و توسط افزونه نمایش داده نمی‌شود.</p>

        <hr>

        <h2>نگاشت روش‌های پرداخت به نقدی / شرایطی</h2>
        <p>در این بخش مشخص کن هر درگاه یا روش پرداخت ووکامرس برای خواربارچی «نقدی» حساب شود یا «شرایطی». اگر روشی در هیچ‌کدام انتخاب نشود، مقدار پیش‌فرض پایین استفاده می‌شود.</p>

        <form method="post" action="options.php">
            <?php settings_fields('kharbarchi_settings'); ?>

            <table class="widefat striped" style="max-width: 980px;">
                <thead>
                    <tr>
                        <th style="width: 160px;">شناسه روش پرداخت</th>
                        <th>عنوان</th>
                        <th style="width: 120px;">وضعیت ووکامرس</th>
                        <th style="width: 120px;">نقدی</th>
                        <th style="width: 120px;">شرایطی</th>
                    </tr>
                </thead>
                <tbody>
                <?php if (empty($gateways)) : ?>
                    <tr>
                        <td colspan="5">هیچ روش پرداختی پیدا نشد.</td>
                    </tr>
                <?php else : ?>
                    <?php foreach ($gateways as $gateway_id => $gateway) : ?>
                        <?php
                        $gateway_id = sanitize_key($gateway_id);
                        $title = isset($gateway->title) ? $gateway->title : $gateway_id;
                        $enabled = isset($gateway->enabled) ? $gateway->enabled : '';
                        ?>
                        <tr>
                            <td><code><?php echo esc_html($gateway_id); ?></code></td>
                            <td><?php echo esc_html($title); ?></td>
                            <td><?php echo $enabled === 'yes' ? 'فعال' : 'غیرفعال'; ?></td>
                            <td>
                                <label>
                                    <input type="checkbox" name="kharbarchi_cash_gateway_ids[]" value="<?php echo esc_attr($gateway_id); ?>" <?php checked(in_array($gateway_id, $cash_gateways, true)); ?>>
                                    نقدی
                                </label>
                            </td>
                            <td>
                                <label>
                                    <input type="checkbox" name="kharbarchi_credit_gateway_ids[]" value="<?php echo esc_attr($gateway_id); ?>" <?php checked(in_array($gateway_id, $credit_gateways, true)); ?>>
                                    شرایطی
                                </label>
                            </td>
                        </tr>
                    <?php endforeach; ?>
                <?php endif; ?>
                </tbody>
            </table>

            <h3>مقدار پیش‌فرض برای روش‌های نامشخص</h3>
            <p>چون سیاست اصلی فروش خواربارچی شرایطی است، پیشنهاد می‌شود مقدار پیش‌فرض روی شرایطی بماند.</p>
            <p>
                <label>
                    <input type="radio" name="kharbarchi_default_payment_type" value="credit" <?php checked($default_payment_type, 'credit'); ?>>
                    شرایطی
                </label>
                &nbsp;&nbsp;
                <label>
                    <input type="radio" name="kharbarchi_default_payment_type" value="cash" <?php checked($default_payment_type, 'cash'); ?>>
                    نقدی
                </label>
            </p>

            <?php submit_button('ذخیره تنظیمات پرداخت'); ?>
        </form>

        <hr>

        <h2>متاهای مخفی خرید در سفارش</h2>
        <p>بعد از ثبت سفارش، مجموع قیمت خرید نقد و مجموع قیمت خرید شرایطی فقط به‌صورت متای مخفی روی سفارش ذخیره می‌شود. افزونه این دو مقدار را نه در پنل سفارش و نه در سایت نمایش نمی‌دهد.</p>
        <p>کلیدهای داخلی، فقط برای سیستم و گزارش‌های مدیریتی:</p>
        <ul style="list-style: disc; margin-right: 22px;">
            <li><code>_khb_o_bct</code></li>
            <li><code>_khb_o_brt</code></li>
        </ul>

        <hr>

        <h2>همسان‌سازی کلی قیمت‌های ووکامرس</h2>
        <p>این دکمه فقط <code>_regular_price</code> و <code>_price</code> را با <code>_kharbarchi_sale_credit_price</code> هماهنگ می‌کند و <code>_sale_price</code> را خالی نگه می‌دارد.</p>
        <?php
        $update_url = wp_nonce_url(
            admin_url('admin-post.php?action=kharbarchi_update_all_prices'),
            'kharbarchi_update_all_prices'
        );
        ?>
        <a href="<?php echo esc_url($update_url); ?>" class="button button-primary">همسان‌سازی همه قیمت‌ها</a>
    </div>
    <?php
}
