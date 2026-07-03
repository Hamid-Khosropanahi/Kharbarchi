# Kharbarchi Price Engine - Auto Control Final

این نسخه افزونه بر اساس اصل طراحی جدید خواربارچی آماده شده است:

- قیمت نهایی محصول از SQL/سیستم مدیریتی وارد WooCommerce می‌شود.
- افزونه وردپرس قیمت را از وزن، تعداد در کارتن یا قیمت کیلویی بازسازی نمی‌کند.
- قیمت فروش شرایطی، قیمت رسمی محصول و مقدار WooCommerce `_regular_price` و `_price` است.
- قیمت نقدی به‌عنوان «تخفیف تسویه نقدی» نگهداری می‌شود و فقط هنگام انتخاب پرداخت نقدی در سبد/تسویه اعمال می‌شود.
- قیمت‌های کیلویی، وزن، تعداد در کارتن، وزن فله و حداقل خرید برای کنترل، گزارش و نمایش استفاده می‌شوند.
- هر محصول وضعیت کنترلی سبز/زرد/قرمز دارد.
- محصولی که وضعیت قرمز بگیرد، به draft منتقل می‌شود تا اشتباهی publish نشود.

## Metaهای اصلی محصول

### قیمت‌های نهایی

- `_kharbarchi_sale_credit_price` قیمت فروش کالا / شرایطی نهایی
- `_kharbarchi_sale_cash_price` قیمت نقدی پس از تخفیف تسویه
- `_kharbarchi_buy_credit_price` قیمت خرید شرایطی نهایی
- `_kharbarchi_buy_cash_price` قیمت خرید نقد نهایی

### قیمت‌های کیلویی برای کنترل/نمایش

- `_kharbarchi_sale_credit_price_per_kg`
- `_kharbarchi_sale_cash_price_per_kg`
- `_kharbarchi_buy_credit_price_per_kg`
- `_kharbarchi_buy_cash_price_per_kg`

### بسته‌بندی و وزن

- `_khb_package_code`
- `_khb_package_title`
- `_khb_package_group` = `bulk` / `retail` / `none`
- `_khb_unit_weight` وزن واحد بسته یا وزن واحد کالا
- `_khb_product_carton_count` تعداد بسته در کارتن همین محصول
- `_khb_bulk_weight_kg` وزن فروش فله
- `_khb_min_purchase_kg` حداقل خرید کیلویی
- `_khb_image_tag`
- `_kharbarchi_package_id`

### کنترل خرید

- `_kharbarchi_min_cartons`
- `_kharbarchi_max_cartons`
- `_kharbarchi_carton_step`

## Metaهای کنترلی خودکار

- `_khb_price_source_mode`
- `_khb_expected_sale_credit_price`
- `_khb_expected_sale_cash_price`
- `_khb_expected_buy_credit_price`
- `_khb_expected_buy_cash_price`
- `_khb_sale_credit_diff`
- `_khb_sale_cash_diff`
- `_khb_buy_credit_diff`
- `_khb_buy_cash_diff`
- `_khb_price_check_status` = `green` / `yellow` / `red`
- `_khb_price_check_code`
- `_khb_price_check_note`
- `_khb_price_check_percent`
- `_khb_price_check_amount`

## کدهای کنترلی

- `OK`
- `ROUNDING_DIFF`
- `MINOR_PRICE_DIFF`
- `MAJOR_PRICE_DIFF`
- `MISSING_WEIGHT`
- `MISSING_UNIT_WEIGHT`
- `MISSING_BULK_WEIGHT`
- `MISSING_CARTON_COUNT`
- `MISSING_MIN_PURCHASE`
- `MISSING_SALE_CREDIT_PRICE`
- `MISSING_SALE_CASH_PRICE`
- `MISSING_BUY_CREDIT_PRICE`
- `MISSING_BUY_CASH_PRICE`
- `CASH_GREATER_THAN_CREDIT`
- `FINAL_PRICE_MISMATCH`
- `PER_KG_PRICE_MISMATCH`
- `MISSING_PACKAGE`
- `MISSING_COMMODITY`
- `INVALID_PACKAGE_CODE`
- `NEED_FIX`
- `DRAFT_REQUIRED`

## منطق رنگ‌ها

- green: داده‌ها کامل است و اختلاف قیمت در محدوده مجاز/گردکردن است.
- yellow: محصول نیازمند بررسی است، ولی خطا بحرانی نیست.
- red: داده ناقص یا ناسازگار است؛ محصول به draft منتقل می‌شود.

## نصب

پوشه افزونه را در مسیر زیر جایگزین کن:

`wp-content/plugins/kharbarchi-price-engine/`

بعد افزونه را deactivate/activate کن.

## نکته مهم برای C#/SQL Sender

در payload محصول، علاوه بر قیمت نهایی، قیمت کیلویی و وزن/تعداد را هم بفرست تا کنترل خودکار کار کند. قیمت نهایی همچنان ملاک فروش است و افزونه آن را دوباره محاسبه نمی‌کند.
