<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * Registers the commodity taxonomy on WooCommerce products and exposes it through wp/v2.
 * Python creates commodity terms through /wp-json/wp/v2/commodity.
 */
function kharbarchi_register_commodity_taxonomy()
{
    register_taxonomy(
        'commodity',
        ['product'],
        [
            'labels' => [
                'name'          => 'کالاهای پایه',
                'singular_name' => 'کالای پایه',
                'menu_name'     => 'کالاهای پایه',
                'add_new_item'  => 'افزودن کالای پایه',
                'edit_item'     => 'ویرایش کالای پایه',
            ],
            'hierarchical'      => true,
            'public'            => true,
            'show_ui'           => true,
            'show_admin_column' => true,
            'show_in_rest'      => true,
            'rest_base'         => 'commodity',
            'rewrite'           => [
                'slug' => 'commodity',
            ],
        ]
    );
}
add_action('init', 'kharbarchi_register_commodity_taxonomy');
