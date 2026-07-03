<?php

if (!defined('ABSPATH')) {
    exit;
}

/**
 * اعتبارسنجی تعداد هنگام افزودن به سبد خرید
 * در معماری فعلی:
 * quantity = تعداد کارتن
 */
add_filter(
    'woocommerce_add_to_cart_validation',
    'kharbarchi_validate_carton_quantity',
    10,
    3
);

function kharbarchi_validate_carton_quantity($passed, $product_id, $quantity)
{
    return kharbarchi_check_carton_quantity_rules(
        $passed,
        $product_id,
        $quantity
    );
}

/**
 * اعتبارسنجی هنگام بروزرسانی تعداد در صفحه سبد خرید
 */
add_filter(
    'woocommerce_update_cart_validation',
    'kharbarchi_validate_carton_quantity_on_update',
    10,
    4
);

function kharbarchi_validate_carton_quantity_on_update(
    $passed,
    $cart_item_key,
    $values,
    $quantity
) {
    if (empty($values['product_id'])) {
        return $passed;
    }

    return kharbarchi_check_carton_quantity_rules(
        $passed,
        $values['product_id'],
        $quantity
    );
}

/**
 * تابع اصلی کنترل تعداد کارتن
 */
function kharbarchi_check_carton_quantity_rules(
    $passed,
    $product_id,
    $quantity
) {
    $package_id = absint(
        get_post_meta(
            $product_id,
            '_kharbarchi_package_id',
            true
        )
    );

    if ($package_id <= 0) {
        return $passed;
    }

    $quantity = absint($quantity);

    $min_cartons = absint(
        get_post_meta(
            $product_id,
            '_kharbarchi_min_cartons',
            true
        )
    );

    $max_cartons = absint(
        get_post_meta(
            $product_id,
            '_kharbarchi_max_cartons',
            true
        )
    );

    $carton_step = absint(
        get_post_meta(
            $product_id,
            '_kharbarchi_carton_step',
            true
        )
    );

    if ($min_cartons <= 0) {
        $min_cartons = 1;
    }

    if ($carton_step <= 0) {
        $carton_step = 1;
    }

    if ($quantity < $min_cartons) {
        wc_add_notice(
            sprintf(
                'حداقل خرید این کالا %s کارتن است.',
                $min_cartons
            ),
            'error'
        );

        return false;
    }

    if (
        $max_cartons > 0 &&
        $quantity > $max_cartons
    ) {
        wc_add_notice(
            sprintf(
                'حداکثر خرید این کالا %s کارتن است.',
                $max_cartons
            ),
            'error'
        );

        return false;
    }

    if ($quantity % $carton_step !== 0) {
        wc_add_notice(
            sprintf(
                'تعداد خرید این کالا باید مضربی از %s کارتن باشد.',
                $carton_step
            ),
            'error'
        );

        return false;
    }

    return $passed;
}