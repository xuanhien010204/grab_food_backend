# 🍔 GrabFood Clone - Food Ordering Backend API

> A full-featured food ordering platform backend built with **ASP.NET Core (.NET 10)**, inspired by GrabFood. Supports multi-tenant stores, wallet payments via MoMo, order lifecycle management, reviews, vouchers, favorites, notifications, and more.

---

## 📋 Table of Contents

- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Authentication](#-authentication)
- [API Reference](#-api-reference)
- [Enums](#-enums)
- [Error Handling](#-error-handling)
- [MoMo Payment Integration](#-momo-payment-integration)
- [Getting Started](#-getting-started)
- [Deployment](#-deployment)

---

## 🏗 Architecture

```
┌──────────────────────────────────────────────────────┐
│                    Mobile App (Client)                │
└──────────────────────┬───────────────────────────────┘
                       │ HTTP REST
┌──────────────────────▼───────────────────────────────┐
│               FoodOrderingPRM392 (API Layer)         │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐ │
│  │ Controllers  │  │ Middlewares   │  │ Extensions  │ │
│  │ (13 total)   │  │ (Global Ex.) │  │ (DI, Auth)  │ │
│  └──────┬──────┘  └──────────────┘  └─────────────┘ │
└─────────┼────────────────────────────────────────────┘
          │ Interfaces
┌─────────▼────────────────────────────────────────────┐
│           FoodOrderingRepository (Service Layer)      │
│  ┌──────────────────┐  ┌───────────────────────────┐ │
│  │ Interface/ (14)   │  │ Implement/ (13)           │ │
│  └──────────────────┘  └───────────────────────────┘ │
└─────────┬────────────────────────────────────────────┘
          │ EF Core + Dapper
┌─────────▼────────────────────────────────────────────┐
│              FoodOrderingCore (Domain Layer)          │
│  ┌────────┐ ┌──────┐ ┌──────────┐ ┌───────────────┐ │
│  │ Data/  │ │ Dto/ │ │ Request/ │ │ Response/     │ │
│  │ Enum/  │ │Const/│ │ Helpers/ │ │ Exceptions/   │ │
│  └────────┘ └──────┘ └──────────┘ └───────────────┘ │
└──────────────────────────────────────────────────────┘
          │
┌─────────▼────────────────────────────────────────────┐
│              SQL Server Database                      │
│              (grab-food.mssql.somee.com)              │
└──────────────────────────────────────────────────────┘
```

---

## 🛠 Tech Stack

| Component | Technology |
|-----------|------------|
| **Framework** | .NET 10, ASP.NET Core |
| **ORM** | Entity Framework Core + Dapper |
| **Database** | SQL Server (Somee.com hosting) |
| **Authentication** | Cookie-based (ASP.NET Core Identity Cookies) |
| **Payment** | MoMo Payment Gateway (Test Environment) |
| **Serialization** | Newtonsoft.Json |
| **Architecture** | 3-Layer (API → Repository → Core) |
| **Patterns** | Repository Pattern, Global Exception Middleware |

---

## 📁 Project Structure

```
FoodOrderingPRM392/                          # Solution Root
│
├── FoodOrderingPRM392/                      # 🌐 API Layer
│   ├── Controllers/
│   │   ├── UserController.cs                # Auth & Profile
│   │   ├── StoreController.cs               # Store listing & detail
│   │   ├── FoodController.cs                # Food CRUD (Admin/Manager)
│   │   ├── FoodTypeController.cs            # Food categories
│   │   ├── FoodStoreController.cs           # Menu (Food+Store+Size)
│   │   ├── TenantController.cs              # Multi-tenant management
│   │   ├── OrderController.cs               # Order lifecycle
│   │   ├── WalletController.cs              # Wallet & MoMo payment
│   │   ├── DeliveryAddressController.cs     # User addresses
│   │   ├── ReviewController.cs              # Reviews & ratings
│   │   ├── VoucherController.cs             # Voucher management
│   │   ├── FavoriteController.cs            # Favorites
│   │   └── NotificationController.cs        # Notifications
│   ├── Extension/
│   │   └── ServiceCollectionExtension.cs    # DI registration
│   ├── Helps/
│   │   └── ClaimsPrincipalExtensions.cs     # User.GetUserId()
│   ├── Middlewares/
│   │   ├── GlobalExceptionMiddleware.cs     # Global error handler
│   │   └── MiddlewareExtensions.cs
│   ├── Filters/
│   │   └── ExceptionFilter.cs
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
│
├── FoodOrderingRepository/                  # 📦 Service Layer
│   ├── Interface/                           # 14 interfaces
│   └── Implement/                           # 13 implementations
│
└── FoodOrderingCore/                        # 🎯 Domain Layer
    ├── Data/              # 15 entities
    ├── Dto/               # DTOs
    ├── Request/           # Request models
    ├── Response/          # Response models
    ├── Enum/              # 6 enum types
    ├── Constants/         # Message constants
    ├── Helpers/           # MomoPaymentHelper
    ├── Exceptions/        # Custom exceptions
    ├── Context/           # FoodOrderingContext
    ├── Extensions/        # JsonConvertExtension
    └── ConfigurationOptions/
```

---

## 🗄 Database Schema

### Entities Overview

| Entity | PK Type | Description |
|--------|---------|-------------|
| `User` | `long` | User accounts with wallet |
| `Role` | `int` | Roles (User, Manager, Admin) |
| `Tenant` | `int` | Multi-tenant groups |
| `Store` | `long` | Food stores with location, rating |
| `Food` | `long` | Food items with type |
| `FoodSize` | `int` | Sizes (S, M, L, XL) |
| `FoodType` | `int` | Categories (Appetizer, Main, Dessert, Beverage) |
| `FoodStore` | `Guid` | Menu: Food + Store + Size + Price |
| `Order` | `Guid` | Orders with status, payment, delivery |
| `OrderDetail` | Composite | Order items (OrderId + FoodStoreId) |
| `WalletTransaction` | `Guid` | Wallet deposit/payment/refund records |
| `DeliveryAddress` | `long` | Saved delivery addresses |
| `Review` | `Guid` | Reviews with rating (1-5), images, reply |
| `Voucher` | `Guid` | Discount vouchers (percent/fixed/free shipping) |
| `VoucherUsage` | `Guid` | Voucher usage tracking per user per order |
| `Favorite` | `long` | Favorite stores and foods |
| `Notification` | `Guid` | In-app notifications |

### Key Relationships

| Relationship | Type | Delete Behavior |
|-------------|------|-----------------|
| Tenant → Store | 1:N | Cascade |
| Store → FoodStore | 1:N | — |
| Food → FoodStore | 1:N | — |
| FoodSize → FoodStore | 1:N | SetNull |
| FoodStore → OrderDetail | 1:N | — |
| User → Order | 1:N | — |
| Store → Order | 1:N | NoAction |
| Order → Review | 1:1 | NoAction |
| User → WalletTransaction | 1:N | NoAction |
| User → DeliveryAddress | 1:N | Cascade |
| User → Favorite | 1:N | Cascade |
| Store → Favorite | 1:N | Cascade |
| User → Notification | 1:N | Cascade |
| Store → Voucher | 1:N | Cascade |
| Voucher → VoucherUsage | 1:N | Cascade |
| User → Review | 1:N | Cascade |
| Store → Review | 1:N | NoAction |

### Unique Constraints

| Entity | Unique Columns |
|--------|---------------|
| `User` | `Email` |
| `FoodStore` | `(StoreId, FoodId, SizeId)` |
| `Voucher` | `Code` |
| `Favorite` | `(UserId, StoreId)` and `(UserId, FoodId)` |

### Seed Data

| Entity | Values |
|--------|--------|
| **Roles** | `1: User`, `2: Manager`, `3: Admin` |
| **FoodTypes** | `1: Appetizer`, `2: Main Course`, `3: Dessert`, `4: Beverage` |
| **FoodSizes** | `1: S (Nhỏ)`, `2: M (Vừa)`, `3: L (Lớn)`, `4: XL (Siêu lớn)` |
| **Tenants** | `1: Default Tenant` |

---

## 🔐 Authentication

Cookie-based authentication using ASP.NET Core Identity Cookies.

| Setting | Value |
|---------|-------|
| Cookie Name | `auth_cookie` |
| Expiration | 168 hours (7 days) |
| HttpOnly | `true` |
| SameSite | `Lax` |
| Secure Policy | `SameAsRequest` |
| Sliding Expiration | `false` |

### Claims in Cookie

| Claim | ClaimType |
|-------|-----------|
| User ID | `ClaimTypes.NameIdentifier` |
| Email | `ClaimTypes.Email` |
| Phone | `ClaimTypes.MobilePhone` |
| Role Name | `ClaimTypes.Role` |
| Role ID | `"RoleId"` |

### Role Permissions

| Role | ID | Access |
|------|----|--------|
| **User** | 1 | Profile, orders, wallet, addresses, reviews, favorites, notifications |
| **Manager** | 2 | + Food, store, voucher management |
| **Admin** | 3 | + Full system access, tenant CRUD, delete operations |

---

## 📡 API Reference

> **Base URL:** `http://grab-food.somee.com`
>
> **Response Format:**
> ```json
> { "message": "string", "result": { ... } }
> ```

---

### 1. User API

**Base:** `/api/users`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/login` | ❌ | Login with email & password |
| `POST` | `/register` | ❌ | Register new account |
| `GET` | `/profile` | ✅ | Get current user profile |
| `GET` | `/sign-out` | ✅ | Sign out (clear cookie) |
| `PATCH` | `/temp-data` | ✅ | Save temporary cart data |
| `DELETE` | `/temp-data` | ✅ | Clear temporary cart data |

**Login:**
```json
// POST /api/users/login
{ "email": "user@example.com", "password": "string" }

// Response
{
  "message": "Success",
  "result": {
    "id": 1, "name": "John", "email": "user@example.com",
    "phone": "0901234567", "walletAmount": 500000,
    "roleName": "User", "roleId": 1
  }
}
```

**Register:**
```json
// POST /api/users/register
{ "name": "John", "email": "user@example.com", "phone": "0901234567", "password": "Pass123" }
// Response: { "message": "Register success" }
```

**Save Cart:**
```json
// PATCH /api/users/temp-data
{
  "orderList": {
    "guid-1": { "quantity": 2, "foodStore": { ... } },
    "guid-2": { "quantity": 1, "foodStore": { ... } }
  }
}
```

---

### 2. Store API

**Base:** `/api/stores`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/` | ❌ | Get all stores |
| `GET` | `/tenant/{id}` | ❌ | Get stores by tenant |
| `GET` | `/{id}` | ❌ | Get store detail |

**Response:**
```json
{
  "id": 1, "name": "Bún Bò Huế Cô Ba",
  "description": "Traditional Vietnamese noodles",
  "address": "123 Lê Lợi, Q1",
  "latitude": "10.7769", "longitude": "106.7009",
  "imageSrc": "https://...", "phone": "0901234567",
  "openTime": "07:00", "closeTime": "22:00",
  "isOpen": true, "rating": 4.50, "reviewCount": 128,
  "minOrderAmount": 30000, "deliveryFee": 15000,
  "estimatedDeliveryTime": 30, "tenantId": 1
}
```

---

### 3. Food API

**Base:** `/api/foods` — **Auth:** Admin, Manager

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/` | Get all foods |
| `GET` | `/{id}` | Get food by ID |
| `POST` | `/` | Create food |
| `PUT` | `/` | Update food |

```json
// POST /api/foods
{ "name": "Phở Bò", "imageSrc": "https://...", "foodTypeId": 2 }

// PUT /api/foods
{ "name": "Phở Bò Updated", "imageSrc": "https://...", "foodTypeId": 2, "isAvaiable": true }
```

---

### 4. Food Type API

**Base:** `/api/food-types`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/` | ❌ | Get all types |
| `GET` | `/{id}` | ❌ | Get type by ID |
| `POST` | `/` | ✅ Admin/Manager | Create type |
| `PUT` | `/` | ✅ Admin/Manager | Update type |
| `DELETE` | `/{id}` | ✅ Admin/Manager | Delete type |

```json
// POST
{ "name": "Seafood", "imgSrc": "https://..." }
// PUT
{ "id": 5, "name": "Updated", "imgSrc": "https://..." }
```

---

### 5. Food Store (Menu) API

**Base:** `/api/food-stores`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/?foodName=&foodTypeId=` | ❌ | Get menu items (filtered) |

```json
// GET /api/food-stores?foodName=Phở&foodTypeId=2
{
  "result": [{
    "id": "guid", "storeId": 1, "storeName": "Store A",
    "foodId": 5, "foodName": "Phở Bò",
    "sizeId": 2, "sizeName": "M",
    "price": 55000, "isAvailable": true
  }]
}
```

---

### 6. Tenant API

**Base:** `/api/tenants`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/` | ❌ | Get all tenants |
| `GET` | `/{id}` | ❌ | Get tenant by ID |
| `POST` | `/` | ✅ Admin/Manager | Create tenant |
| `PUT` | `/` | ✅ Admin/Manager | Update tenant |
| `DELETE` | `/{id}` | ✅ Admin | Delete tenant |

```json
// POST
{ "name": "Restaurant Group" }
// PUT
{ "id": 1, "name": "Updated Group" }
```

---

### 7. Order API

**Base:** `/api/orders` — **Auth:** ✅ Required

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/` | Create order |
| `GET` | `/{id}` | Get order detail |
| `GET` | `/history?status=` | User order history |
| `GET` | `/store/{storeId}?status=` | Store orders |
| `PUT` | `/{id}/status` | Update status |
| `POST` | `/{id}/cancel` | Cancel order |
| `POST` | `/legacy` | Create order (legacy dict) |
| `GET` | `/{id}/legacy` | Get detail (legacy format) |

**Create Order:**
```json
{
  "storeId": 1,
  "paymentMethod": 1,
  "deliveryAddress": "123 Nguyễn Huệ, Q1",
  "recipientPhone": "0901234567",
  "recipientName": "Nguyễn Văn A",
  "note": "Không hành",
  "deliveryFee": 15000,
  "discount": 10000,
  "items": [
    { "foodStoreId": "guid-1", "quantity": 2 },
    { "foodStoreId": "guid-2", "quantity": 1 }
  ]
}
```

**Update Status:**
```json
{ "status": 1, "reason": "optional" }
```

**Cancel Order:**
```json
{ "reason": "Changed my mind" }
```

**Order Status Flow:**
```
Pending(0) → Confirmed(1) → Preparing(2) → Ready(3) → Delivering(4) → Completed(5)
     └──────────────────────────────────────────────────────────────→ Cancelled(6)
```

---

### 8. Wallet & Payment API

**Base:** `/api/wallet`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/balance` | ✅ | Get wallet balance |
| `POST` | `/deposit` | ✅ | Deposit via MoMo |
| `POST` | `/momo/ipn` | ❌ | MoMo IPN webhook |
| `GET` | `/momo/return` | ❌ | MoMo return URL |
| `GET` | `/transactions?pageNumber=&pageSize=` | ✅ | Transaction history |
| `GET` | `/check-balance/{amount}` | ✅ | Check balance |

**Get Balance:**
```json
{
  "result": {
    "userId": 1, "userName": "John",
    "balance": 500000, "formattedBalance": "500,000 VND",
    "lastUpdated": "2025-01-31T14:00:00Z"
  }
}
```

**Deposit:**
```json
// POST /api/wallet/deposit
{ "amount": 100000, "note": "Top up" }

// Response
{
  "result": {
    "orderId": "DEPOSIT_1_abc123",
    "amount": 100000,
    "payUrl": "https://test-payment.momo.vn/...",
    "deepLink": "momo://...",
    "qrCodeUrl": "https://...",
    "success": true
  }
}
```

**Check Balance:**
```json
// GET /api/wallet/check-balance/50000
{ "result": { "amount": 50000, "hasSufficientBalance": true } }
```

---

### 9. Delivery Address API

**Base:** `/api/addresses` — **Auth:** ✅ Required

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/` | Get all addresses |
| `GET` | `/{id}` | Get by ID |
| `GET` | `/default` | Get default address |
| `POST` | `/` | Create address |
| `PUT` | `/{id}` | Update address |
| `DELETE` | `/{id}` | Delete address |
| `PUT` | `/{id}/default` | Set as default |

```json
// POST /api/addresses
{
  "label": "Home",
  "recipientName": "Nguyễn Văn A",
  "phone": "0901234567",
  "address": "123 Nguyễn Huệ, Q1, TP.HCM",
  "addressDetail": "Tầng 3, căn 301",
  "latitude": "10.7769",
  "longitude": "106.7009",
  "isDefault": true
}
```

---

### 10. Review API

**Base:** `/api/reviews`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/` | ✅ | Create review |
| `GET` | `/{id}` | ❌ | Get review |
| `GET` | `/store/{storeId}?pageNumber=&pageSize=` | ❌ | Store reviews + stats |
| `GET` | `/food/{foodId}?pageNumber=&pageSize=` | ❌ | Food reviews |
| `GET` | `/my-reviews?pageNumber=&pageSize=` | ✅ | My reviews |
| `POST` | `/{id}/reply` | ✅ | Store reply |
| `DELETE` | `/{id}` | ✅ | Delete review |
| `GET` | `/can-review/{orderId}` | ✅ | Check eligibility |

**Create:**
```json
{
  "orderId": "guid", "storeId": 1, "foodId": 5,
  "rating": 5, "comment": "Rất ngon!",
  "images": ["https://img1.jpg"]
}
```

**Reply:**
```json
// POST /api/reviews/{id}/reply
{ "reply": "Cảm ơn bạn!" }
```

**Store Reviews Response:**
```json
{
  "result": {
    "stats": { "averageRating": 4.3, "totalReviews": 128 },
    "reviews": [...],
    "pageNumber": 1, "pageSize": 20
  }
}
```

---

### 11. Voucher API

**Base:** `/api/vouchers`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/` | ✅ Admin/Manager | Create voucher |
| `GET` | `/{id}` | ❌ | Get by ID |
| `GET` | `/code/{code}` | ❌ | Get by code |
| `GET` | `/active?storeId=` | ❌ | Active vouchers |
| `GET` | `/available?orderAmount=&storeId=` | ✅ | Available for user |
| `POST` | `/apply` | ✅ | Apply voucher |
| `PUT` | `/{id}` | ✅ Admin/Manager | Update voucher |
| `DELETE` | `/{id}` | ✅ Admin/Manager | Deactivate |

**Create:**
```json
{
  "code": "SAVE20", "name": "Giảm 20%",
  "description": "Giảm 20% đơn từ 50k",
  "type": 1, "value": 20,
  "minOrderAmount": 50000, "maxDiscount": 30000,
  "startDate": "2025-02-01", "endDate": "2025-03-01",
  "usageLimit": 100, "usageLimitPerUser": 1,
  "storeId": null
}
```

**Apply:**
```json
// POST /api/vouchers/apply
{ "code": "SAVE20", "orderAmount": 100000, "storeId": 1 }

// Response
{ "result": { "discountAmount": 20000, "finalAmount": 80000 } }
```

**Update:**
```json
{
  "name": "Updated", "minOrderAmount": 60000,
  "endDate": "2025-04-01", "isActive": true
}
```

---

### 12. Favorite API

**Base:** `/api/favorites` — **Auth:** ✅ Required

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/stores` | Get favorite stores |
| `GET` | `/foods` | Get favorite foods |
| `POST` | `/stores/{storeId}` | Add store favorite |
| `POST` | `/foods/{foodId}` | Add food favorite |
| `DELETE` | `/stores/{storeId}` | Remove store favorite |
| `DELETE` | `/foods/{foodId}` | Remove food favorite |
| `GET` | `/stores/{storeId}/check` | Is store favorited? |
| `GET` | `/foods/{foodId}/check` | Is food favorited? |

```json
// GET /api/favorites/stores/1/check
{ "result": { "isFavorited": true } }
```

---

### 13. Notification API

**Base:** `/api/notifications` — **Auth:** ✅ Required

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/?pageNumber=&pageSize=&isRead=` | Get notifications |
| `GET` | `/unread-count` | Unread count |
| `PUT` | `/{id}/read` | Mark as read |
| `PUT` | `/read-all` | Mark all read |
| `DELETE` | `/{id}` | Delete notification |

**Response:**
```json
{
  "result": {
    "notifications": [{
      "id": "guid", "title": "Order Confirmed",
      "content": "Your order has been confirmed",
      "type": 1, "referenceId": "order-guid",
      "deepLink": "grabfood://orders/guid",
      "isRead": false, "createdAt": "2025-01-31T14:00:00Z"
    }],
    "unreadCount": 5,
    "pageNumber": 1, "pageSize": 20
  }
}
```

---

## 📊 Enums

### OrderStatus
| Value | Name | Description |
|-------|------|-------------|
| 0 | `Pending` | Waiting for confirmation |
| 1 | `Confirmed` | Store confirmed |
| 2 | `Preparing` | Being prepared |
| 3 | `Ready` | Ready for delivery |
| 4 | `Delivering` | Being delivered |
| 5 | `Completed` | Delivered |
| 6 | `Cancelled` | Cancelled |

### PaymentMethod
| Value | Name |
|-------|------|
| 1 | `Wallet` |
| 2 | `CashOnDelivery` |
| 3 | `MoMo` |

### PaymentStatus
| Value | Name |
|-------|------|
| 0 | `Unpaid` |
| 1 | `Paid` |
| 2 | `Refunded` |
| 3 | `Failed` |

### TransactionType
| Value | Name |
|-------|------|
| 1 | `Deposit` |
| 2 | `Payment` |
| 3 | `Refund` |
| 4 | `Withdrawal` |
| 5 | `Bonus` |

### TransactionStatus
| Value | Name |
|-------|------|
| 0 | `Pending` |
| 1 | `Completed` |
| 2 | `Failed` |
| 3 | `Cancelled` |

### VoucherType
| Value | Name | Description |
|-------|------|-------------|
| 1 | `Percent` | Discount % (e.g., 20%) |
| 2 | `FixedAmount` | Fixed amount (e.g., 10,000 VND) |
| 3 | `FreeShipping` | Free delivery |

### NotificationType
| Value | Name |
|-------|------|
| 0 | `System` |
| 1 | `Order` |
| 2 | `Promotion` |
| 3 | `Wallet` |
| 4 | `Review` |
| 5 | `Feature` |

### RoleEnum
| Value | Name |
|-------|------|
| 1 | `User` |
| 2 | `Manager` |
| 3 | `Admin` |

---

## ⚠️ Error Handling

### Global Exception Middleware

All unhandled exceptions are caught by `GlobalExceptionMiddleware` and returned as consistent JSON.

| Exception | HTTP Status | Use Case |
|-----------|-------------|----------|
| `BadRequestException` | 400 | Business validation |
| `OutOfWalletAmountException` | 400 | Insufficient balance |
| `DbUpdateException` | 400 | DB constraint violation |
| `DbException` | 400 | Database errors |
| `ArgumentNullException` | 400 | Missing parameters |
| `ArgumentException` | 400 | Invalid arguments |
| `InvalidOperationException` | 400 | Invalid operations |
| `KeyNotFoundException` | 404 | Not found |
| `Exception` (default) | 500 | Unexpected errors |

**Error Response:** `{ "message": "Error description" }`

**Pattern:**
```
Client → Controller → Repository (throws exception) → GlobalExceptionMiddleware → HTTP Response
```

---

## 💳 MoMo Payment Integration

### Configuration

```json
{
  "MoMo": {
    "PartnerCode": "MOMOBKUN20180529",
    "AccessKey": "klm05TvNBzhg7h7j",
    "SecretKey": "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa",
    "ApiEndpoint": "https://test-payment.momo.vn",
    "NotifyUrl": "http://grab-food.somee.com/api/wallet/momo/ipn",
    "ReturnUrl": "grabfood://payment/callback"
  }
}
```

### Payment Flow

```
App → POST /wallet/deposit → Backend creates pending tx → POST MoMo API
                                                            ↓
App ← payUrl/deepLink ← Backend ← MoMo returns URLs

App → User pays on MoMo app

       Backend ← POST /wallet/momo/ipn ← MoMo IPN webhook
       (verify signature, update wallet, return 204)

App ← MoMo redirects via deep link (grabfood://payment/callback)
```

### Key Details

| Aspect | Value |
|--------|-------|
| OrderId Format | `DEPOSIT_{userId}_{requestId}` |
| Signature | HMAC-SHA256 |
| IPN Endpoint | `POST /api/wallet/momo/ipn` (no auth) |
| Return URL | Deep link to app |
| Idempotency | Checks `ExternalReference` |
| Amount Range | 10,000 - 50,000,000 VND |
| Environment | Test (`test-payment.momo.vn`) |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server
- Git

### Run Locally

```bash
git clone https://github.com/xuanhien010204/grab_food_backend.git
cd grab_food_backend
dotnet restore
dotnet ef database update --project FoodOrderingPRM392
dotnet run --project FoodOrderingPRM392
```

### Local URLs

- `https://localhost:7163` — HTTPS
- `http://localhost:5190` — HTTP
- `/swagger` — Swagger UI

---

## 🌐 Deployment

| Component | URL |
|-----------|-----|
| **API** | `http://grab-food.somee.com` |
| **Swagger** | `http://grab-food.somee.com/swagger` |
| **Database** | `grab-food.mssql.somee.com` |
| **GitHub** | `https://github.com/xuanhien010204/grab_food_backend` |

### Hosting Notes

- **HTTP only** (Somee.com free tier)
- Cookie `SecurePolicy = SameAsRequest` for HTTP compatibility
- Cookie `SameSite = Lax` for cross-site requests
- HTTPS redirection disabled
- MoMo IPN URL must be publicly accessible

---

## 📄 License

Developed for educational purposes — **PRM392 Course Project**.