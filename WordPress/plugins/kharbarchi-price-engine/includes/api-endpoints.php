<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * Checks whether the current Application Password user may edit WooCommerce products.
 * This permission callback protects the commodity attachment endpoint from public writes.
 */
function kharbarchi_api_can_edit_products()
{
    return current_user_can('edit_products')
        || current_user_can('manage_woocommerce');
}


/**
 * Registers a public health route and authenticated product-commodity routes in khb/v1.
 * The GET product route supports diagnostics and the POST route performs taxonomy attachment.
 */
function kharbarchi_register_custom_api_endpoints()
{
    register_rest_route(
        'khb/v1',
        '/ping',
        [
            'methods'             => WP_REST_Server::READABLE,
            'callback'            => 'kharbarchi_api_ping',
            'permission_callback' => '__return_true',
        ]
    );

    register_rest_route(
        'khb/v1',
        '/product/(?P<product_id>\d+)/commodity',
        [
            [
                'methods'             => WP_REST_Server::READABLE,
                'callback'            => 'kharbarchi_api_get_product_commodity',
                'permission_callback' => '__return_true',
                'args'                => [
                    'product_id' => [
                        'required'          => true,
                        'sanitize_callback' => 'absint',
                    ],
                ],
            ],
            [
                'methods'             => WP_REST_Server::CREATABLE,
                'callback'            => 'kharbarchi_api_attach_commodity_to_product',
                'permission_callback' => 'kharbarchi_api_can_edit_products',
                'args'                => [
                    'product_id' => [
                        'required'          => true,
                        'sanitize_callback' => 'absint',
                    ],
                    'commodity_id' => [
                        'required'          => true,
                        'sanitize_callback' => 'absint',
                    ],
                ],
            ],
        ]
    );
}
add_action('rest_api_init', 'kharbarchi_register_custom_api_endpoints');


/**
 * Returns a minimal response proving that the custom endpoint file has loaded.
 * This route can be tested directly in a browser without authentication.
 */
function kharbarchi_api_ping()
{
    return rest_ensure_response(
        [
            'success' => true,
            'message' => 'Kharbarchi API is active.',
        ]
    );
}


/**
 * Validates a product ID and returns all attached commodity taxonomy terms.
 * The route distinguishes a missing product from an unregistered REST route.
 */
function kharbarchi_api_get_product_commodity(WP_REST_Request $request)
{
    $product_id = absint($request->get_param('product_id'));

    if (!$product_id || get_post_type($product_id) !== 'product') {
        return new WP_Error(
            'invalid_product',
            'Invalid WooCommerce product ID.',
            ['status' => 400]
        );
    }

    $terms = wp_get_object_terms($product_id, 'commodity');

    if (is_wp_error($terms)) {
        return new WP_Error(
            'commodity_read_failed',
            $terms->get_error_message(),
            ['status' => 500]
        );
    }

    $items = array_map(
        static function ($term) {
            return [
                'id'   => (int) $term->term_id,
                'name' => $term->name,
                'slug' => $term->slug,
            ];
        },
        $terms
    );

    return rest_ensure_response(
        [
            'success'    => true,
            'product_id' => $product_id,
            'commodity'  => $items,
        ]
    );
}


/**
 * Replaces the product's commodity relationship and stores the same term ID in product metadata.
 * Both taxonomy and metadata are updated so WordPress queries and the pricing plugin remain consistent.
 */
function kharbarchi_api_attach_commodity_to_product(WP_REST_Request $request)
{
    $product_id   = absint($request->get_param('product_id'));
    $commodity_id = absint($request->get_param('commodity_id'));

    if (!$product_id || get_post_type($product_id) !== 'product') {
        return new WP_Error(
            'invalid_product',
            'Invalid WooCommerce product ID.',
            ['status' => 400]
        );
    }

    $term = term_exists($commodity_id, 'commodity');

    if (!$commodity_id || !$term) {
        return new WP_Error(
            'invalid_commodity',
            'Invalid commodity term ID.',
            ['status' => 400]
        );
    }

    $result = wp_set_object_terms(
        $product_id,
        [$commodity_id],
        'commodity',
        false
    );

    if (is_wp_error($result)) {
        return new WP_Error(
            'commodity_attach_failed',
            $result->get_error_message(),
            ['status' => 500]
        );
    }

    update_post_meta(
        $product_id,
        '_kharbarchi_commodity_id',
        $commodity_id
    );

    return rest_ensure_response(
        [
            'success'           => true,
            'product_id'        => $product_id,
            'commodity_id'      => $commodity_id,
            'term_taxonomy_ids' => array_map('intval', $result),
        ]
    );
}
