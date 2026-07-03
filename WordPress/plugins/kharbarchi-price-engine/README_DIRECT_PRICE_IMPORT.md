# Kharbarchi Price Engine - Direct Product Price Import

This version follows the final rule:

- Product prices come directly from SQL/import.
- No product price is calculated from kg weight, package weight, carton count, or packaging cost.
- `_regular_price` and `_price` must equal `_kharbarchi_sale_credit_price`.
- `_sale_price` must stay empty.
- `woodmart_price_unit_of_measure` must be `کارتن`.

## Main product meta keys

Required on WooCommerce product:

- `_kharbarchi_sale_cash_price`
- `_kharbarchi_sale_credit_price`
- `_kharbarchi_buy_cash_price`
- `_kharbarchi_buy_credit_price`
- `_kharbarchi_package_id`
- `_khb_package_code`
- `_khb_package_title`
- `_khb_package_group`
- `_khb_unit_weight`
- `_khb_product_carton_count`
- `_khb_image_tag`
- `_kharbarchi_min_cartons`
- `_kharbarchi_max_cartons`
- `_kharbarchi_carton_step`
- `_kharbarchi_commodity_id`
- `_kharbarchi_brand_name`
- `_kharbarchi_brand_english_name`
- `_khb_source_id`
- `_khb_source_row_number`
- `_khb_need_fix`
- `_khb_fix_note`
- `woodmart_price_unit_of_measure`

## Package CPT meta keys

Post type: `kharbarchi_package`

- `_khb_package_code`
- `_khb_package_group`
- `_khb_unit_weight`
- `_khb_default_carton_count`
- `_khb_image_tag`

Legacy keys `_khb_carton_count` and `_khb_apply_packing_cost` are only mirrors/compatibility fields and must not be used for pricing.

## Commodity taxonomy meta keys

Taxonomy: `commodity`

- `base_image_id`
- `base_gallery_ids`
- `_khb_english_name`
- `_khb_category_slug`

`price_per_kg` is deleted when commodity is saved/imported and is not used.

## Required import order

1. `POST /wp-json/khb/v1/category/upsert`
2. `POST /wp-json/khb/v1/commodity/upsert`
3. `POST /wp-json/khb/v1/package/upsert`
4. `POST /wp-json/wc/v3/products`
5. `POST /wp-json/khb/v1/product/link`

## Product link body example

```json
{
  "product_id": 123,
  "category_slug": "legumes",
  "commodity_slug": "lape-azarshahr",
  "package_code": "lape-azarshahr-450",
  "meta": {
    "_kharbarchi_sale_cash_price": "1200000",
    "_kharbarchi_sale_credit_price": "1250000",
    "_kharbarchi_buy_cash_price": "0",
    "_kharbarchi_buy_credit_price": "0",
    "_khb_package_code": "450",
    "_khb_package_title": "کارتن ۱۲ عددی ۴۵۰ گرمی",
    "_khb_package_group": "retail",
    "_khb_unit_weight": "0.45",
    "_khb_product_carton_count": "12",
    "_khb_image_tag": "۴۵۰ گرمی",
    "_kharbarchi_min_cartons": "1",
    "_kharbarchi_max_cartons": "0",
    "_kharbarchi_carton_step": "1",
    "_kharbarchi_brand_name": "برند",
    "_kharbarchi_brand_english_name": "brand",
    "_khb_source_id": "1",
    "_khb_source_row_number": "1",
    "_khb_need_fix": "0",
    "_khb_fix_note": ""
  }
}
```

## Product WC payload rule

When creating/updating product through WooCommerce REST:

- `regular_price` = SaleCreditPrice
- `sale_price` = empty string
- `status` = publish if valid, draft if missing price/package/need_fix
- `catalog_visibility` = visible if valid, hidden if draft/fix
- `meta_data` must contain all product meta keys above.

## Expected commodity count

For the current CSV/import, there must be 43 base commodities. Each final WooCommerce product is a combination of one commodity and one package.
