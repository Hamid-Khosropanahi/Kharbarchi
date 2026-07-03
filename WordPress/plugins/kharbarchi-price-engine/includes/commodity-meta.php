<?php

if (!defined('ABSPATH')) {
    exit;
}

add_action('commodity_add_form_fields', 'kharbarchi_add_commodity_fields');
add_action('commodity_edit_form_fields', 'kharbarchi_edit_commodity_fields');
add_action('created_commodity', 'kharbarchi_save_commodity_fields');
add_action('edited_commodity', 'kharbarchi_save_commodity_fields');
add_action('rest_api_init', 'kharbarchi_register_commodity_rest_meta', 99);

function kharbarchi_register_commodity_rest_meta()
{
    $string_keys = ['base_gallery_ids', '_khb_english_name', '_khb_category_slug'];

    foreach ($string_keys as $key) {
        register_term_meta('commodity', $key, [
            'type'              => 'string',
            'single'            => true,
            'show_in_rest'      => true,
            'sanitize_callback' => 'sanitize_text_field',
            'auth_callback'     => 'kharbarchi_import_api_can_manage',
        ]);
    }

    register_term_meta('commodity', 'base_image_id', [
        'type'              => 'integer',
        'single'            => true,
        'show_in_rest'      => true,
        'sanitize_callback' => 'absint',
        'auth_callback'     => 'kharbarchi_import_api_can_manage',
    ]);
}

function kharbarchi_add_commodity_fields()
{
    ?>
    <div class="form-field term-base-image-wrap">
        <label for="base_image_id">تصویر اصلی کالای پایه</label>
        <input type="number" name="base_image_id" id="base_image_id" min="0" step="1">
        <p>شناسه رسانه وردپرس. قیمت روی کالای پایه نگهداری نمی‌شود.</p>
    </div>
    <div class="form-field term-base-gallery-wrap">
        <label for="base_gallery_ids">گالری کالای پایه</label>
        <input type="text" name="base_gallery_ids" id="base_gallery_ids" placeholder="12,13,14">
    </div>
    <?php
}

function kharbarchi_edit_commodity_fields($term)
{
    $base_image_id = absint(get_term_meta($term->term_id, 'base_image_id', true));
    $base_gallery_ids = get_term_meta($term->term_id, 'base_gallery_ids', true);
    ?>
    <tr class="form-field term-base-image-wrap">
        <th scope="row"><label for="base_image_id">تصویر اصلی کالای پایه</label></th>
        <td><input type="number" name="base_image_id" id="base_image_id" min="0" step="1" value="<?php echo esc_attr($base_image_id); ?>"><p class="description">قیمت محصول مستقیم از SQL می‌آید و روی محصول ذخیره می‌شود.</p></td>
    </tr>
    <tr class="form-field term-base-gallery-wrap">
        <th scope="row"><label for="base_gallery_ids">گالری کالای پایه</label></th>
        <td><input type="text" name="base_gallery_ids" id="base_gallery_ids" value="<?php echo esc_attr($base_gallery_ids); ?>" placeholder="12,13,14"></td>
    </tr>
    <?php
}

function kharbarchi_save_commodity_fields($term_id)
{
    $base_image_id = isset($_POST['base_image_id']) ? absint(wp_unslash($_POST['base_image_id'])) : 0;
    $base_gallery_ids = isset($_POST['base_gallery_ids']) ? sanitize_text_field(wp_unslash($_POST['base_gallery_ids'])) : '';

    update_term_meta($term_id, 'base_image_id', $base_image_id);
    update_term_meta($term_id, 'base_gallery_ids', $base_gallery_ids);

    // Remove old kg pricing source if it exists. It must not drive final prices.
    delete_term_meta($term_id, 'price_per_kg');
}
