# AGENTS.md — Kharbarchi / خواربارچی Project Rules for Codex

> **Purpose**  
> This file defines the binding project rules for Codex or any coding agent working on the Kharbarchi project.  
> Read this file before inspecting, editing, refactoring, migrating, building, deploying, or testing the project.

---

## 0. Agent Operating Rules

### 0.1 Read-first rule
Before making any change, Codex must:

1. Read this file completely.
2. Inspect the relevant project files.
3. Explain the intended plan.
4. Make the smallest safe change.
5. Build/test where possible.
6. Report the exact files changed and why.

### 0.2 No guessing on business rules
If code appears to conflict with these rules, **do not silently choose one**.  
Report the conflict and propose the safest correction.

### 0.3 No destructive actions
Never delete files or folders automatically.

This includes, but is not limited to:

- `bin/`
- `obj/`
- `Migrations/`
- `.git/`
- publish folders
- logs
- uploaded files
- database scripts
- WordPress plugin folders
- generated assets

If cleanup is required, only report the exact paths that the user may delete manually.

### 0.4 No broad/unrelated refactors
Do not reformat, rename, or restructure unrelated files.  
Make changes only where required for the task.

### 0.5 No hardcoded secrets
Never hardcode passwords, API keys, JWT keys, database passwords, WooCommerce keys, gateway credentials, or server secrets.

Use environment variables, user-secrets for local development, or protected server env files.

### 0.6 Persian language and RTL support
The business language is Persian/Farsi. Preserve Persian text correctly.

For Python scripts that print Persian text in terminal, use this approach when needed:

```python
import arabic_reshaper
from bidi.algorithm import get_display
```

Use UTF-8 / utf8mb4 consistently.

---

## 1. Project Identity

### 1.1 Project name
- English: `Kharbarchi`
- Persian: `خواربارچی`

### 1.2 Business domain
Kharbarchi is an online grocery/legume sales system focused on selling legumes to shopkeepers and retailers.

The business model includes:

- sourcing products from farmers
- in-house sorting
- in-house packaging using automated machinery
- wholesale/carton-oriented sales
- WooCommerce storefront
- ERP/backend system as source of operational control

### 1.3 Brand/product description
The core product domain is legumes / حبوبات.

Example positioning:
- از کشاورز می‌گیریم
- خودمان سورت می‌کنیم
- خودمان با ماشین‌آلات تمام اتوماتیک بسته‌بندی می‌کنیم
- هدف: فروش قابل اعتماد برای مغازه‌دارها و خرده‌فروشان

---

## 2. Canonical Rule Set

### 2.1 KHB numbered rules
The project has an approved rule set named `KHB-001` through `KHB-245`.

Current rule status:

- `KHB-001` to `KHB-245` are approved and binding.
- `KHB-045` is approved in revised form.
- `KHB-173` is approved in revised/confirmed form.
- `KHB-214` is approved in revised/confirmed form.
- `KHB-215` is approved in revised/confirmed form.
- `KHB-081` is removed from the main rule set and must not be treated as active unless explicitly re-approved.
- `KHB-246` is added and binding.

Important: The exact full text of all numbered rules is not embedded here.  
If a change depends on an individual numbered rule whose exact text is not present in this file or another project document, Codex must not invent it. It must ask the user or locate the canonical rule document.

### 2.2 KHB-045 revised meaning
`KHB-045` distinguishes **packaging type** from **sale type**.

Important distinction:

- `450g` and `900g` are packaging types used in carton sales.
- `bulk`, `15kg`, `20kg`, `25kg`, and similar large formats are bulk-oriented sale/packaging formats depending on product context.
- “بدون بسته بندی” is only a temporary/disabled fallback state, not a normal final commercial packaging type.

### 2.3 KHB-246
`KHB-246`:

- In **bulk sales**, display and use kilogram price when the product is sold/calculated by kilogram.
- In **carton/package sales**, display package price and use package/count basis.
- Kilogram and package metadata may be stored for reporting or reference.
- Do not use kilogram metadata to calculate carton/package sales unless the active rule for that product explicitly requires it.

### 2.4 Effective rule precedence
When there is conflict, apply this precedence:

1. Latest explicit user decision
2. This `AGENTS.md`
3. Canonical KHB numbered rule document
4. Existing production behavior
5. Existing code comments
6. Agent inference

Do not use inference to override business rules.

---

## 3. Core Design Principle

The main design goal is:

> **Maximum automation and minimum human error.**

No important numeric value should be entered, changed, displayed, calculated, imported, exported, or synchronized without independent system validation.

Important numeric values include:

- prices
- discount percentages
- final sale price
- cash price
- installment/terms price
- purchase price
- carton quantity
- pack count per carton
- unit weight
- bulk weight
- packaging fee
- stock quantity
- order totals
- payment amounts
- gateway amounts
- WooCommerce synchronized prices
- ERP synchronized values

---

## 4. Architecture Rules

### 4.1 Two-server architecture
Kharbarchi production architecture uses two logical servers:

#### Server 1 — Public shop server
- Public access: yes
- Website: `https://www.Kharbarchi.ir/`
- Main software: WordPress + WooCommerce
- Role: storefront, customer checkout, public-facing shop

#### Server 2 — ERP server
- Name in terminal: `barsam`
- Known IP: `2.179.72.80`
- OS: Ubuntu 24.04.4 LTS / Noble
- Public role: must not be treated as a public customer-facing server
- Main software: ERP services + MySQL
- Role: internal operational system, ERP bridge, product/price/order processing

### 4.2 Public access rule
The public browser/customer must not connect directly to ERP.

Allowed pattern:

```text
Customer Browser
    -> WooCommerce / Public Shop
        -> controlled API/sync
            -> ERP / internal service
```

Disallowed pattern:

```text
Customer Browser
    -> ERP API directly
```

### 4.3 ERP exposure rule
ERP services and MySQL must not be publicly exposed.

Preferred access methods:

- SSH tunnel
- VPN
- localhost-only binding
- private/internal IP allowlist

### 4.4 Binding environment separation
Development bindings:

- every website/shop/WooCommerce URL must be localhost, loopback, `*.localhost`, or `*.local`
- database name must be exactly `Kharbarchi_Local`
- Production URLs and database names are forbidden

Production bindings:

- canonical website/shop/WooCommerce URL must be exactly `https://www.Kharbarchi.ir/`
- database name must be exactly `Kharbarchi_erp`
- Local URLs and database names are forbidden

### 4.5 Outbound direction
Where possible, ERP should initiate outbound HTTPS calls to the shop/WooCommerce rather than exposing inbound public APIs.

---

## 5. Security Rules

### 5.1 Secrets
Never commit real secrets to Git.

Forbidden inside source code or repository:

- WooCommerce consumer key
- WooCommerce consumer secret
- JWT signing key
- database username/password
- payment gateway credentials
- application admin passwords
- WordPress application passwords
- SSH credentials
- server passwords

Use placeholders in examples:

```bash
WooCommerce__ConsumerKey="REPLACE_WITH_SECRET"
WooCommerce__ConsumerSecret="REPLACE_WITH_SECRET"
Jwt__Key="REPLACE_WITH_SECRET"
ConnectionStrings__MySqlConnection="REPLACE_WITH_SECRET"
```

### 5.2 ASP.NET Core configuration
Local development may use user-secrets.  
Production must use environment variables or protected env files.

Use double underscore syntax for nested config keys:

```bash
Jwt__Key="..."
WooCommerce__BaseUrl="..."
WooCommerce__ConsumerKey="..."
WooCommerce__ConsumerSecret="..."
Gateway__AllowedUserName="..."
Gateway__SeedGatewayUser="..."
Gateway__Password="..."
ConnectionStrings__MySqlConnection="..."
```

### 5.3 MySQL connection key
The ASP.NET Core application expects:

```text
ConnectionStrings:MySqlConnection
```

Therefore production env variable should be:

```text
ConnectionStrings__MySqlConnection
```

Do not replace it with `DefaultConnection` unless the actual code is intentionally changed everywhere.

### 5.4 Env file syntax
When writing Linux env files, quote values that contain special characters.

Safe example:

```bash
ConnectionStrings__MySqlConnection='Server=127.0.0.1;Port=3307;Database=Kharbarchi;Uid=REPLACE_WITH_USER;Pwd=REPLACE_WITH_PASSWORD;CharSet=utf8mb4;Allow User Variables=True;'
Jwt__Key='REPLACE_WITH_LONG_RANDOM_SECRET'
```

A broken env file can cause shell errors such as:

```text
command not found
```

### 5.5 Database exposure
MySQL must not be opened to the public internet.

For admin/development access from the user’s PC:

- use SSH tunnel or VPN
- local forwarded port such as `3307 -> server:3306` is acceptable
- firewall must restrict direct MySQL access

---

## 6. Deployment Rules

### 6.1 ERP server
ERP deployment target:

```text
Server: barsam
OS: Ubuntu 24.04.4 LTS
Role: ERP/Internal
```

### 6.2 Service binding
Internal ASP.NET Core services should prefer localhost or internal IP binding.

Example:

```bash
ASPNETCORE_URLS=http://127.0.0.1:5100
```

Do not bind ERP APIs to `0.0.0.0` unless explicitly required and protected by firewall/reverse proxy.

### 6.3 systemd
Use a dedicated systemd service for production apps.

Do not change service names, env paths, users, or ports blindly.  
Inspect the existing `.service` file first.

### 6.4 Safe deploy pattern
Preferred production deployment pattern:

```text
/opt/kharbarchi/<service>/releases/<version>
/opt/kharbarchi/<service>/current -> releases/<version>
/etc/kharbarchi/<service>/<service>.env
```

Rollback should be possible by switching the `current` symlink back.

### 6.5 No destructive deploy scripts
Deploy scripts must not delete previous releases automatically unless explicitly approved.

### 6.6 Build-before-restart
Before restarting production services:

1. publish/build successfully
2. confirm env file exists
3. confirm DB connection key exists
4. confirm service user has required permissions
5. restart service
6. check status/logs
7. test health endpoint

---

## 7. Database Rules

### 7.1 Database engines
The project uses both:

- MySQL for ERP/WooCommerce-related production services
- SQL Server in earlier/local ETL/import workflows

Do not assume a single database engine for all modules.

### 7.2 Charset
Use utf8mb4 for Persian text.

Avoid introducing utf8mb3.

### 7.3 Price history
All prices and price registration dates must be stored in a dedicated price history table.

The history must include:

- product
- price type
- price amount
- registration date/time
- active/current flag or validity range
- source of change where applicable
- user/system actor where applicable

When a new price is registered:

- do not overwrite history
- insert a new history record
- mark previous current record as inactive or close its validity range
- use the current active record for calculations

### 7.4 Migrations
Do not bypass EF Core migration errors by disabling checks.

If the application reports:

```text
The model for context 'AppDbContext' has pending changes.
```

Then Codex must:

1. inspect the changed model
2. inspect existing migrations
3. decide whether a new migration is needed
4. generate or recommend the correct migration
5. never drop/recreate production DB
6. never apply destructive migration without explicit approval

### 7.5 Table creation
Do not create duplicate tables.

Before creating tables such as queues/staging tables:

- check if they already exist
- check schema
- create migration or ALTER safely
- preserve data

Known project table names include:

```text
KHB_Product_Update_Queue
Woo_Product_Update_Queue
All_Product_With_Process
Woo_Product_Stage
KHB_Product_Final
```

A new intermediate table before the product update queue may exist or be required. Do not invent a conflicting name without checking current schema.

---

## 8. Product Domain Rules

### 8.1 Product representation
Each sellable WooCommerce product should represent a real purchasable commercial unit.

For carton-based products:

```text
One product = one carton
```

### 8.2 Product categories
Use English category values such as `En_Taxonomic` for slugs/categories where available.

Persian labels may be used for display.

### 8.3 Product slugs
Product slugs should be English and stable.

Avoid Persian slugs for system integration unless explicitly required.

### 8.4 SKU policy
SKU must be concise.

Current preferred SKU policy:

```text
At most two letters from each meaningful English part + underscore + counter
```

General rules:

- avoid long SKUs
- avoid Persian SKUs
- avoid duplicate SKUs
- keep SKU stable after initial creation when possible

### 8.5 Disabled/draft products
If a product has no valid active sales price, or required packaging data is missing:

- create/keep it as disabled/draft
- do not publish it
- mark/display fallback as `بدون بسته بندی` only as temporary/disabled state

### 8.6 Existing products before rebuild
Before rebuilding WooCommerce products, existing products may need to be moved to draft/hidden.

This must be done safely and intentionally.

Do not permanently delete existing products.

### 8.7 Image rules
Product images should prefer existing WordPress/WooCommerce mapped images when available.

Known mapping source:

```text
Woo_Product_Stage
```

Images may be based on commodity/base product image with packaging tag overlays where applicable.

---

## 9. Packaging and Carton Rules

### 9.1 Packaging types
Common packaging/product forms include:

- bulk / فله
- 450g package
- 900g package
- 15kg bulk/carton format
- 20kg bulk/carton format
- 25kg bulk/carton format
- temporary `بدون بسته بندی` fallback

### 9.2 450g package
Default commercial rule:

```text
450g package: 12 packs per carton
```

### 9.3 900g package
Default commercial rule:

```text
900g package: 6 packs per carton
```

### 9.4 Bulk carton weights
Bulk base weights may include:

```text
15kg
20kg
25kg
```

These must be product/context aware.

### 9.5 Pack count may vary
The latest user rule says:

> Each product may have its own direct final price, and pack count per carton may differ per product.

Therefore:

- do not assume the same pack count for every product if product-specific data exists
- do not hardcode pack count globally when product-specific data is available
- default values are only defaults/fallbacks

### 9.6 Carton-only purchase rule
Carton-oriented products must be purchased in allowed carton multiples.

Cart validation must prevent invalid partial quantities.

### 9.7 Packaging fee
Known packaging fee:

```text
30,000 Toman per package
```

This fee applies only where the relevant packaging rule/toggle says it applies.

Do not apply packaging fee globally.

### 9.8 Packaging metadata
Packaging metadata may include:

- unit weight in kg
- packs per carton
- carton weight
- price multiplier
- packaging fee enabled/disabled
- image tag text
- product/package type
- minimum purchase
- carton quantity rule

---

## 10. Pricing Rules

### 10.1 Active pricing approach
The latest direction is:

> The user no longer wants general per-kg pricing for every product.  
> Each product should be able to have a direct product price.

Therefore:

- do not recreate old automatic kg-derived pricing globally
- do not force all products to calculate from base kg price
- use direct final product price where the current data model/rule requires it

### 10.2 KHB-246 pricing display/calculation distinction
For products that are truly bulk/kg-based:

- kg price may be displayed
- kg can be calculation basis

For carton/package sales:

- package/carton price should be displayed
- pack/count should be calculation basis
- kg metadata may exist but should not drive package/carton calculation unless explicitly active

### 10.3 Legacy kg formula
Older design had formulas like:

```text
Final carton price =
(kg_price × cartons × packs_per_carton × unit_weight)
+
(packaging_fee × cartons × packs_per_carton)
```

This is now legacy/global behavior and must not be reintroduced as the universal rule.

Only use it in modules/products where explicitly still required.

### 10.4 Sales price types
The system must support at least:

- cash sale price / فروش نقدی
- installment or terms sale price / فروش شرایطی

Rules:

- installment/terms sale has no cash discount
- cash price can imply a discount compared to terms price
- discount percentage/difference should be shown per product when applicable
- total cash discount should be shown on the order

### 10.5 Purchase price types
Admin/internal purchase pricing may include:

- cash purchase price / خرید نقد
- installment/terms purchase price / خرید شرایطی

These are internal/admin values and must not be exposed publicly unless explicitly required.

### 10.6 WooCommerce official price
WooCommerce official public price should represent the correct active sale price for the selected sale/payment model.

Earlier accepted principle:

- terms/installment price can be the official/base WooCommerce price
- cash discount can be applied as settlement/payment discount when cash payment is selected

Do not mix sale price types without clear rules.

### 10.7 Zero or missing price
If sales price is zero, missing, invalid, or not active:

- product should not be published as purchasable
- create/keep as draft/disabled
- do not silently set random fallback prices

---

## 11. WooCommerce / WordPress Rules

### 11.1 Production shop
Production shop URL:

```text
https://www.Kharbarchi.ir/
```

The `www` host, casing, scheme, and trailing slash above are binding.

### 11.2 Local development shop
Known local WordPress/WooCommerce URL:

```text
https://localhost:4433/Kharbarchi
```

### 11.3 WooCommerce role
WooCommerce is the public storefront and checkout layer.

ERP remains the source of controlled operational logic where applicable.

### 11.4 Custom plugin
Known custom plugin:

```text
wp-content/plugins/kharbarchi-price-engine
```

Known important file from previous errors:

```text
wp-content/plugins/kharbarchi-price-engine/includes/product-meta.php
```

Do not assume plugin structure without inspecting files.

### 11.5 Custom post types
Known custom post types/endpoints may include:

```text
commodity
kharbarchi_package
```

Expected REST style examples:

```text
/wp-json/wp/v2/kharbarchi_package
/wp-json/wc/v3/products
```

### 11.6 REST registration
If adding CPT/meta fields that must be used through REST:

- ensure `show_in_rest = true`
- register meta correctly
- define schema and sanitize callbacks safely

### 11.7 WordPress sanitize callback hazard
Do not register PHP native functions such as `floatval` directly as a sanitize callback if WordPress may call the callback with multiple parameters.

Unsafe example:

```php
'sanitize_callback' => 'floatval'
```

Safer example:

```php
'sanitize_callback' => function ($value) {
    return floatval($value);
}
```

This avoids errors like:

```text
floatval() expects exactly 1 argument, 4 given
```

### 11.8 WooCommerce REST product rebuild
When rebuilding products through REST:

1. Draft/hide existing products if requested.
2. Create/update base commodities.
3. Create/update packaging CPT records.
4. Create/update WooCommerce carton products.
5. Use stable SKU/slug matching.
6. Do not duplicate products.
7. Draft products with invalid/missing price.
8. Preserve images/descriptions when mapped.

### 11.9 Product source data rules
Known source fields/tables include:

```text
All_Product_With_Process
Woo_Product_Stage
Have_Other_Packege
Package_One
En_Taxonomic
```

Rules:

- `Have_Other_Packege = 1` may create additional 450g/900g products where data supports it.
- `Have_Other_Packege = 0` should generally create only the main/Package_One product.
- Items without usable packaging/price should still be represented as disabled/draft when required.

---

## 12. ERP / API Integration Rules

### 12.1 ERP ownership
ERP owns/controls core operational data where applicable:

- products
- prices
- price history
- stock
- purchase data
- internal purchase prices
- sync queues
- operational reports

### 12.2 WooCommerce API integration
ERP may communicate with WooCommerce via WooCommerce API.

Required config keys:

```bash
WooCommerce__BaseUrl
WooCommerce__ConsumerKey
WooCommerce__ConsumerSecret
```

### 12.3 API security
ERP APIs should not be public unless intentionally exposed through a controlled gateway.

Use:

- authentication
- HTTPS
- IP allowlists
- internal binding
- reverse proxy restrictions

### 12.4 Sync queue
Product updates should go through a controlled queue/staging process where applicable.

Known queue name:

```text
KHB_Product_Update_Queue
```

Do not directly overwrite production WooCommerce data without traceability when a queue is expected.

---

## 13. .NET / Blazor Rules

### 13.1 Stack
Known project stack:

- ASP.NET Core server API
- Blazor WebAssembly client
- .NET 8 for current Blazor WASM context
- EF Core / MySQL provider in server API context

Do not upgrade major .NET versions without explicit instruction.

### 13.2 Known local URLs
Known development URLs:

```text
Server API: https://localhost:7100/
Client:     https://localhost:7029/
```

CORS between these origins must be configured intentionally.

### 13.3 Authentication
Known auth components:

```text
JwtAuthStateProvider
AuthTokenHandler
CascadingAuthenticationState
```

Do not bypass authentication checks to make builds pass.

### 13.4 Program.cs
If `Program.cs` becomes too large, refactor carefully into extension methods/services only when needed.

Do not move logic without preserving behavior.

### 13.5 EF Core migrations in production
Never run production migrations blindly.

For migration changes:

- inspect schema
- generate migration
- review generated SQL
- ensure non-destructive
- backup plan must exist
- user approval required for destructive changes

---

## 14. Python / ETL Rules

### 14.1 Python scripts
Python import/sync scripts should have:

- English docstrings
- clear logging
- safe retries where appropriate
- no deletion of remote data unless explicitly requested
- SSL handling that distinguishes local self-signed certificates from production HTTPS

### 14.2 Local SSL
For local WordPress at `https://localhost:4433/Kharbarchi`, self-signed SSL may require `verify=False` in Python requests.

Never use `verify=False` for production `https://www.Kharbarchi.ir/`.

### 14.3 Persian terminal output
When a Python script displays Persian text in terminal, use Arabic reshaper and bidi display if needed.

### 14.4 SQL import safety
When importing numeric data:

- validate precision/scale
- avoid invalid float parameters
- log bad rows
- do not silently truncate or coerce important values

---

## 15. Payment Gateway Rules

### 15.1 WooCommerce payment gateway
The WooCommerce payment gateway integration may require:

- national code field / کد ملی
- customer information mapping
- gateway request API
- callback/verify API
- storage of gateway reference variables
- order status update after successful verification

### 15.2 Payment amount integrity
Payment amount must match the validated order amount.

Never trust only client-side amount.

### 15.3 Gateway credentials
Gateway credentials must be stored only in secure config/env/options, not in source code.

---

## 16. UI / UX Rules

### 16.1 Persian and RTL
UI must support Persian and RTL properly.

### 16.2 Pricing display
Display must clearly distinguish:

- cash price
- installment/terms price
- discount amount/percentage
- carton/package unit
- kg unit where relevant
- number of packs in carton where relevant

### 16.3 Customer confusion prevention
Do not display technical packaging metadata in a way that confuses the buyer.

For carton sales, customer-facing language should emphasize carton/package purchase unit.

---

## 17. Validation Rules

### 17.1 Server-side validation
All important business validation must happen server-side.

Client-side validation is useful but never sufficient.

### 17.2 Cart validation
Cart must reject invalid purchase quantities.

For carton-only products:

- quantity must match carton/multiple rules
- partial/invalid quantity must not proceed to checkout

### 17.3 Price validation
Before order/payment:

- recalculate price server-side
- verify active price history
- verify selected payment/sale type
- verify product is purchasable
- verify discount rules
- verify order total

---

## 18. Logging and Audit Rules

### 18.1 Important events
Log/audit important changes:

- price creation/change
- product publish/draft changes
- WooCommerce sync result
- payment request/verify result
- failed validations
- admin/manual overrides
- migration/deploy actions

### 18.2 No sensitive logs
Do not log secrets, passwords, tokens, full API keys, or full connection strings.

Mask secrets in logs.

---

## 19. Testing Rules

### 19.1 Required checks before reporting success
Before saying a code change is done, Codex should run the relevant checks if environment allows:

```bash
dotnet build
dotnet test
```

For WordPress/PHP:

```bash
php -l <file>
```

For Python:

```bash
python -m py_compile <file>
```

For SQL/migrations:

- generate migration
- inspect SQL
- do not apply destructive changes automatically

### 19.2 Report inability honestly
If Codex cannot run a command because dependencies, database, secrets, or environment are missing, it must say so clearly.

Do not claim tests passed unless they actually ran.

---

## 20. Prohibited Changes

Codex must not do these without explicit user approval:

- delete files or folders
- drop database tables
- truncate tables
- reset migrations
- recreate database
- publish products with zero/missing price
- expose ERP/MySQL publicly
- commit secrets
- disable authentication
- disable validation
- change production domain
- change server architecture
- change pricing model globally
- replace WooCommerce as storefront
- upgrade major framework versions
- rename core tables broadly
- remove price history
- remove audit/logging for critical events

---

## 21. Recommended Codex Response Format

When Codex works on this project, respond in this format:

```text
Task understood:
- ...

Relevant rules from AGENTS.md:
- ...

Files inspected:
- ...

Plan:
1. ...
2. ...

Changes made:
- file: reason

Commands run:
- command: result

Risks / notes:
- ...

Manual actions for user:
- ...
```

---

## 22. Current Known Important Paths / Names

These are known from the project context and must be verified before use:

```text
Kharbarchi.Client
Kharbarchi.Server
wp-content/plugins/kharbarchi-price-engine
wp-content/plugins/kharbarchi-price-engine/includes/product-meta.php
/etc/kharbarchi-api.env
/etc/kharbarchi/
ConnectionStrings__MySqlConnection
KHB_Product_Update_Queue
Woo_Product_Update_Queue
All_Product_With_Process
Woo_Product_Stage
KHB_Product_Final
```

Do not assume these paths are final if the repository shows a different structure.

---

## 23. Human Owner Preferences

The project owner prefers:

- full corrected files when code changes are large
- exact file paths
- exact commands
- step-by-step deployment instructions
- Persian explanations for operational instructions
- no automatic deletion
- no hidden assumptions
- no vague “change this somewhere” instructions

When giving a patch, be precise.

---

## 24. Final Reminder

Kharbarchi is not just a codebase. It is a business system with strict rules around pricing, packaging, WooCommerce synchronization, ERP security, and human-error prevention.

A technically valid change is not acceptable if it violates business rules.

When in doubt:

1. preserve data
2. preserve price history
3. preserve security
4. preserve product/carton logic
5. avoid deletion
6. ask before destructive or business-changing actions
