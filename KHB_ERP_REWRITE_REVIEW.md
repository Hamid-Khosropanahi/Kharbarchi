# گزارش بازنویسی ERP خواربارچی

این نسخه روی کپی پروژه ساخته شده و هیچ فایل/پوشه‌ای حذف نشده است.

## اصلاحات اصلی

1. رفتارهای destructive در مسیر Import/Process غیرفعال شد.
   - `truncate=true` در import نادیده گرفته می‌شود و warning برمی‌گرداند.
   - `clearGeneratedBeforeProcess` و `clearTargets` نادیده گرفته می‌شوند.
   - `ResetGeneratedProductTablesAsync` و `TruncateTableIfExistsAsync` به no-op ایمن تبدیل شدند.

2. migrationهای دارای `DROP TABLE IF EXISTS` ایمن شدند.
   - عبارت‌های drop به کامنت SQL تبدیل شده‌اند.
   - `Down()` migrationها به rollback غیرمخرب تبدیل شد.
   - migration جدید `20260628001000_KharbarchiErpRulesSafeReconcile` اضافه شد.

3. قانون KHB-247 اضافه شد.
   - جدول `KHB_Product_Price_History` ساخته می‌شود.
   - هنگام پردازش محصول، قیمت‌های جدید ثبت می‌شوند.
   - اگر قیمت جاری تغییر کرده باشد، رکورد قبلی با `ValidToUtc` بسته می‌شود.

4. منطق قیمت‌گذاری KHB-246 اعمال شد.
   - کارتن وزنی: قیمت نهایی = قیمت کیلویی × وزن کارتن.
   - کارتن بسته‌ای: قیمت نهایی = قیمت بسته × تعداد بسته در کارتن.
   - هزینه بسته‌بندی و قیمت کیلویی در کارتن بسته‌ای فقط informational/control است و در محاسبه قیمت نهایی لحاظ نمی‌شود.

5. افزونه نهایی در مسیر زیر داخل پروژه قرار گرفت:

```text
WordPress/plugins/kharbarchi-price-engine
```

## محدودیت بررسی

در این محیط `dotnet` نصب نبود؛ بنابراین build واقعی پروژه قابل اجرا نبود. تغییرات با بازبینی ایستا و رعایت قوانین پروژه انجام شد.
