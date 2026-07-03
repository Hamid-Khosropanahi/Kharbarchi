<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('add_meta_boxes', 'kharbarchi_package_add_meta_boxes');

function kharbarchi_package_add_meta_boxes()
{
    add_meta_box('kharbarchi_package_info', 'مشخصات بسته‌بندی', 'kharbarchi_package_meta_box_html', 'kharbarchi_package', 'normal', 'high');
}

function kharbarchi_package_meta_box_html($post)
{
    wp_nonce_field('kharbarchi_save_package_meta', 'kharbarchi_package_nonce');

    $package_code = get_post_meta($post->ID, '_khb_package_code', true);
    $package_group = get_post_meta($post->ID, '_khb_package_group', true);
    $unit_weight = get_post_meta($post->ID, '_khb_unit_weight', true);
    $default_carton_count = get_post_meta($post->ID, '_khb_default_carton_count', true);
    $image_tag = get_post_meta($post->ID, '_khb_image_tag', true);
    ?>
    <div class="kharbarchi-admin-box">
        <p><label>کد بسته‌بندی</label><input type="text" name="_khb_package_code" value="<?php echo esc_attr($package_code); ?>" placeholder="مثلاً 450 یا B30"></p>
        <p><label>گروه بسته‌بندی</label><input type="text" name="_khb_package_group" value="<?php echo esc_attr($package_group); ?>" placeholder="bulk / retail / none"></p>
        <p><label>وزن واحد - فقط برای نمایش</label><input type="number" step="0.001" min="0" name="_khb_unit_weight" value="<?php echo esc_attr($unit_weight); ?>" placeholder="مثلاً 0.45"></p>
        <p><label>تعداد پیش‌فرض در کارتن</label><input type="number" step="1" min="0" name="_khb_default_carton_count" value="<?php echo esc_attr($default_carton_count); ?>" placeholder="مثلاً 12"></p>
        <p><label>متن تگ روی تصویر</label><input type="text" name="_khb_image_tag" value="<?php echo esc_attr($image_tag); ?>" placeholder="مثلاً 450 گرمی"></p>
        <p><strong>توجه:</strong> این اطلاعات قیمت را محاسبه نمی‌کند. قیمت نهایی محصول از SQL روی خود محصول می‌آید.</p>
    </div>
    <?php
}

add_action('save_post_kharbarchi_package', 'kharbarchi_save_package_meta');

function kharbarchi_save_package_meta($post_id)
{
    if (!isset($_POST['kharbarchi_package_nonce']) || !wp_verify_nonce(sanitize_text_field(wp_unslash($_POST['kharbarchi_package_nonce'])), 'kharbarchi_save_package_meta')) {
        return;
    }

    if (defined('DOING_AUTOSAVE') && DOING_AUTOSAVE) {
        return;
    }

    if (wp_is_post_revision($post_id)) {
        return;
    }

    if (!current_user_can('edit_post', $post_id)) {
        return;
    }

    $package_code = isset($_POST['_khb_package_code']) ? sanitize_text_field(wp_unslash($_POST['_khb_package_code'])) : '';
    $package_group = isset($_POST['_khb_package_group']) ? sanitize_text_field(wp_unslash($_POST['_khb_package_group'])) : '';
    $unit_weight = isset($_POST['_khb_unit_weight']) ? wc_format_decimal(wp_unslash($_POST['_khb_unit_weight'])) : '0';
    $default_carton_count = isset($_POST['_khb_default_carton_count']) ? absint(wp_unslash($_POST['_khb_default_carton_count'])) : 0;
    $image_tag = isset($_POST['_khb_image_tag']) ? sanitize_text_field(wp_unslash($_POST['_khb_image_tag'])) : '';

    update_post_meta($post_id, '_khb_package_code', $package_code);
    update_post_meta($post_id, '_khb_package_group', $package_group);
    update_post_meta($post_id, '_khb_unit_weight', $unit_weight);
    update_post_meta($post_id, '_khb_default_carton_count', $default_carton_count);
    update_post_meta($post_id, '_khb_image_tag', $image_tag);

    // Legacy mirrors for backward compatibility only.
    update_post_meta($post_id, '_khb_carton_count', $default_carton_count);
    update_post_meta($post_id, '_khb_apply_packing_cost', 0);

    if (function_exists('kharbarchi_update_products_by_package')) {
        kharbarchi_update_products_by_package($post_id);
    }
}
