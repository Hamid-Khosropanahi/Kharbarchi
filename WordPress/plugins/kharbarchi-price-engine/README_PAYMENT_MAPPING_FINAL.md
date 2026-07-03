# Kharbarchi Price Engine - Payment Mapping Final

## هدف این نسخه

این نسخه بر اساس دو اصل جدید اصلاح شده است:

1. نوع پرداخت خواربارچی از روی روش پرداخت WooCommerce تشخیص داده می‌شود.
   - مدیر در تنظیمات افزونه مشخص می‌کند کدام gateway نقدی است و کدام gateway شرایطی.
   - اگر gateway در هیچ لیستی نبود، مقدار پیش‌فرض استفاده می‌شود. پیشنهاد پیش‌فرض: شرایطی.

2. بعد از ثبت سفارش، مجموع قیمت خرید نقد و مجموع قیمت خرید شرایطی فقط به‌صورت متای مخفی سفارش ذخیره می‌شوند.
   - این دو مقدار در frontend نمایش داده نمی‌شوند.
   - در پنل سفارش هم افزونه آن‌ها را نمایش نمی‌دهد.
   - کلیدهای داخلی:
     - `_khb_o_bct` = مجموع قیمت خرید نقد سفارش
     - `_khb_o_brt` = مجموع قیمت خرید شرایطی سفارش

## فایل‌های تغییرکرده

- `includes/payment-pricing.php`
- `includes/settings-page.php`
- `kharbarchi-price-engine.php`

## تنظیم روش پرداخت

بعد از فعال‌سازی افزونه برو به:

WooCommerce Products → تنظیمات خواربارچی

در بخش «نگاشت روش‌های پرداخت به نقدی / شرایطی» برای هر gateway مشخص کن:

- نقدی
- شرایطی

اگر روش پرداختی انتخاب نشده باشد یا در هیچ لیستی نباشد، مقدار پیش‌فرض استفاده می‌شود.

## منطق قیمت

- `credit` یعنی قیمت رسمی/شرایطی محصول.
- `cash` یعنی قیمت نقدی به عنوان تخفیف تسویه سریع.

متاهای محصول:

- `_kharbarchi_sale_credit_price` = قیمت رسمی/شرایطی
- `_kharbarchi_sale_cash_price` = قیمت نقدی
- `_kharbarchi_buy_credit_price` = قیمت خرید شرایطی، داخلی
- `_kharbarchi_buy_cash_price` = قیمت خرید نقد، داخلی

قیمت WooCommerce:

- `_regular_price = _kharbarchi_sale_credit_price`
- `_price = _kharbarchi_sale_credit_price`
- `_sale_price = empty`

## متاهای سفارش

متاهای عمومی/قابل نمایش توسط افزونه:

- `_kharbarchi_payment_type`
- `_kharbarchi_payment_gateway_id`
- `_kharbarchi_cash_discount_total`

متاهای مخفی داخلی خرید:

- `_khb_o_bct`
- `_khb_o_brt`

افزونه فقط متاهای پرداخت و تخفیف را در سفارش نمایش می‌دهد و متاهای خرید را نمایش نمی‌دهد.

## تست syntax

روی همه فایل‌های PHP این نسخه `php -l` اجرا شده و خطای syntax وجود نداشت.
