# 🍔 GrabFood Clone - Food Ordering API

A comprehensive food ordering backend API built with ASP.NET Core, featuring wallet management, MoMo payment integration, order management, reviews, favorites, and notifications.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Features](#features)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Authentication](#authentication)
- [Payment Integration](#payment-integration)

---

## 🎯 Overview

**GrabFood Clone** is a full-featured food ordering platform backend that supports:
- Multi-tenant restaurant management
- User authentication with cookie-based sessions
- Digital wallet with MoMo payment integration
- Complete order lifecycle management
- Reviews and ratings system
- Favorites/Wishlist functionality
- Push notification system
- Voucher/Promotion management

---

## 🏗️ Architecture

The solution follows a **Clean Architecture** pattern with 3 layers:

```
┌─────────────────────────────────────────────────────────────────┐
│                    FoodOrderingPRM392 (API)                      │
│  - Controllers (HTTP endpoints)                                  │
│  - Middlewares (Exception handling)                              │
│  - Extensions (DI configuration)                                 │
│  - Filters (Exception filters)                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                FoodOrderingRepository (Business Logic)           │
│  - Interfaces (Repository contracts)                             │
│  - Implementations (Business logic + Data access)                │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    FoodOrderingCore (Domain)                     │
│  - Entities (Data models)                                        │
│  - DTOs (Data transfer objects)                                  │
│  - Enums (Status codes, types)                                   │
│  - Requests/Responses (API contracts)                            │
│  - Exceptions (Custom exceptions)                                │
│  - Helpers (Utilities)                                           │
│  - Constants (Messages)                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🛠️ Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 10, ASP.NET Core |
| **Database** | SQL Server |
| **ORM** | Entity Framework Core + Dapper |
| **Authentication** | Cookie Authentication |
| **Payment** | MoMo Payment Gateway |
| **API Documentation** | Swagger/OpenAPI |
| **Hosting** | Somee.com |

---

## 🗄️ Database Schema

### Core Entities

```
┌─────────────────────────────────────────────────────────────────┐
│                        ENTITY RELATIONSHIPS                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  User ─────┬──── Orders ────── OrderDetails ──── FoodStore      │
│            │                                          │          │
│            ├──── WalletTransactions                   │          │
│            │                                          │          │
│            ├──── DeliveryAddresses                    ▼          │
│            │                                       Food          │
│            ├──── Reviews ◄───── Store ◄───── Tenant             │
│            │                      │                              │
│            ├──── Favorites        ├──── FoodStore               │
│            │                      │                              │
│            ├──── Notifications    └──── Vouchers                │
│            │                                                     │
│            └──── VoucherUsages                                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Entity Details

#### **User**
```csharp
- Id: long (PK)
- Name: string
- Email: string (unique)
- Phone: string
- Password: string (hashed)
- WalletAmount: decimal
- AvatarUrl: string
- IsActive: bool
- RoleId: int (FK → Role)
- CreatedAt: DateTime
- LastLoginAt: DateTime?
```

#### **Store**
```csharp
- Id: long (PK)
- Name: string
- Description: string
- Address: string
- Latitude/Longitude: string
- ImageSrc: string
- Phone: string
- OpenTime/CloseTime: string
- IsOpen: bool
- IsActive: bool
- Rating: decimal (1-5)
- ReviewCount: int
- MinOrderAmount: decimal
- DeliveryFee: decimal
- EstimatedDeliveryTime: int (minutes)
- TenantId: int (FK → Tenant)
```

#### **Food**
```csharp
- Id: long (PK)
- Name: string
- Description: string
- ImageSrc: string
- FoodTypeId: int (FK → FoodType)
- IsAvailable: bool
- HasSize: bool
- Rating: decimal
- ReviewCount: int
```

#### **FoodStore** (Store-specific pricing)
```csharp
- Id: Guid (PK)
- StoreId: long (FK → Store)
- FoodId: long (FK → Food)
- SizeId: int? (FK → FoodSize)
- Price: decimal
- IsAvailable: bool
```

#### **Order**
```csharp
- Id: Guid (PK)
- UserId: long (FK → User)
- StoreId: long (FK → Store)
- PurchaseDate: DateTime
- Status: OrderStatus (enum)
- PaymentMethod: PaymentMethod (enum)
- PaymentStatus: PaymentStatus (enum)
- SubTotal: decimal
- DeliveryFee: decimal
- Discount: decimal
- Total: decimal
- DeliveryAddress: string
- RecipientPhone: string
- RecipientName: string
- Note: string
- VoucherCode: string
- CancelReason: string
- ConfirmedAt/CompletedAt/CancelledAt: DateTime?
```

#### **WalletTransaction**
```csharp
- Id: Guid (PK)
- UserId: long (FK → User)
- TransactionType: TransactionType (enum)
- Amount: decimal
- BalanceBefore: decimal
- BalanceAfter: decimal
- Status: TransactionStatus (enum)
- Description: string
- ExternalReference: string
- PaymentMethod: string
- CreatedAt: DateTime
- CompletedAt: DateTime?
```

### Enums

#### **OrderStatus**
| Value | Name | Description |
|-------|------|-------------|
| 0 | Pending | Order placed, awaiting confirmation |
| 1 | Confirmed | Store confirmed the order |
| 2 | Preparing | Food is being prepared |
| 3 | Ready | Ready for pickup/delivery |
| 4 | Delivering | Out for delivery |
| 5 | Completed | Order delivered |
| 6 | Cancelled | Order cancelled |

#### **PaymentMethod**
| Value | Name | Description |
|-------|------|-------------|
| 1 | Wallet | Pay from digital wallet |
| 2 | CashOnDelivery | Pay on delivery |
| 3 | MoMo | Pay via MoMo |

#### **PaymentStatus**
| Value | Name | Description |
|-------|------|-------------|
| 0 | Unpaid | Not yet paid |
| 1 | Paid | Payment completed |
| 2 | Refunded | Payment refunded |
| 3 | Failed | Payment failed |

#### **TransactionType**
| Value | Name | Description |
|-------|------|-------------|
| 1 | Deposit | Add money to wallet |
| 2 | Payment | Pay for order |
| 3 | Refund | Refund from cancelled order |

---

## 🔌 API Endpoints

### 👤 Authentication (`/api/users`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/login` | User login | ❌ |
| POST | `/register` | User registration | ❌ |
| GET | `/profile` | Get user profile | ✅ |
| GET | `/sign-out` | Logout | ✅ |
| PUT | `/top-up` | Add money (legacy) | ✅ |
| PATCH | `/temp-data` | Save cart data | ✅ |
| DELETE | `/temp-data` | Clear cart data | ✅ |

### 💰 Wallet (`/api/wallet`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/balance` | Get wallet balance | ✅ |
| POST | `/deposit` | Create MoMo deposit | ✅ |
| POST | `/momo/ipn` | MoMo IPN webhook | ❌ |
| GET | `/momo/return` | MoMo return URL | ❌ |
| GET | `/transactions` | Transaction history | ✅ |
| GET | `/check-balance/{amount}` | Check balance | ✅ |

### 📦 Orders (`/api/orders`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/` | Create new order | ✅ |
| GET | `/{id}` | Get order details | ✅ |
| GET | `/history` | Get order history | ✅ |
| GET | `/store/{storeId}` | Get store orders | ✅ |
| PUT | `/{id}/status` | Update order status | ✅ |
| POST | `/{id}/cancel` | Cancel order | ✅ |

### 🏪 Stores (`/api/stores`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get all stores | ❌ |
| GET | `/{id}` | Get store by ID | ❌ |
| GET | `/filter` | Filter stores | ❌ |
| POST | `/` | Create store | ✅ |
| PUT | `/{id}` | Update store | ✅ |

### 🍕 Foods (`/api/foods`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get all foods | ❌ |
| GET | `/{id}` | Get food by ID | ❌ |
| POST | `/` | Create food | ✅ |
| PUT | `/{id}` | Update food | ✅ |

### 🛒 Food Store Items (`/api/food-stores`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/store/{storeId}` | Get store menu | ❌ |
| GET | `/{id}` | Get item details | ❌ |
| POST | `/` | Add menu item | ✅ |
| PUT | `/{id}` | Update menu item | ✅ |

### 📍 Delivery Addresses (`/api/addresses`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get all addresses | ✅ |
| GET | `/{id}` | Get address by ID | ✅ |
| GET | `/default` | Get default address | ✅ |
| POST | `/` | Create address | ✅ |
| PUT | `/{id}` | Update address | ✅ |
| PUT | `/{id}/default` | Set as default | ✅ |
| DELETE | `/{id}` | Delete address | ✅ |

### ⭐ Reviews (`/api/reviews`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get review by ID | ❌ |
| GET | `/store/{storeId}` | Get store reviews | ❌ |
| GET | `/food/{foodId}` | Get food reviews | ❌ |
| GET | `/my-reviews` | Get my reviews | ✅ |
| GET | `/can-review/{orderId}` | Check can review | ✅ |
| POST | `/` | Create review | ✅ |
| POST | `/{id}/reply` | Store reply | ✅ |
| DELETE | `/{id}` | Delete review | ✅ |

### 🎫 Vouchers (`/api/vouchers`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get voucher by ID | ❌ |
| GET | `/code/{code}` | Get by code | ❌ |
| GET | `/active` | Get active vouchers | ❌ |
| GET | `/available` | Get available for user | ✅ |
| POST | `/` | Create voucher | 🔐 Admin |
| POST | `/apply` | Apply voucher | ✅ |
| PUT | `/{id}` | Update voucher | 🔐 Admin |
| DELETE | `/{id}` | Deactivate voucher | 🔐 Admin |

### ❤️ Favorites (`/api/favorites`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/stores` | Get favorite stores | ✅ |
| GET | `/foods` | Get favorite foods | ✅ |
| GET | `/stores/{id}/check` | Is store favorited | ✅ |
| GET | `/foods/{id}/check` | Is food favorited | ✅ |
| POST | `/stores/{id}` | Add store favorite | ✅ |
| POST | `/foods/{id}` | Add food favorite | ✅ |
| DELETE | `/stores/{id}` | Remove store favorite | ✅ |
| DELETE | `/foods/{id}` | Remove food favorite | ✅ |

### 🔔 Notifications (`/api/notifications`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get notifications | ✅ |
| GET | `/unread-count` | Get unread count | ✅ |
| PUT | `/{id}/read` | Mark as read | ✅ |
| PUT | `/read-all` | Mark all read | ✅ |
| DELETE | `/{id}` | Delete notification | ✅ |

---

## 🌟 Features

### 1. 💳 Digital Wallet System
- **Balance Management**: Track user wallet balance
- **MoMo Integration**: Deposit via MoMo payment gateway
- **Transaction History**: Full audit trail of all transactions
- **Real-time Updates**: IPN webhook for instant balance updates

### 2. 📦 Order Management
- **Order Lifecycle**: Pending → Confirmed → Preparing → Ready → Delivering → Completed
- **Multiple Payment Methods**: Wallet, COD, MoMo
- **Order Cancellation**: With refund support
- **Order History**: Full order history with filtering

### 3. 🏪 Multi-Tenant Store System
- **Tenant Management**: Multiple brands/tenants
- **Store Management**: Each tenant can have multiple stores
- **Menu Management**: Store-specific pricing and availability
- **Operating Hours**: Open/close time tracking

### 4. ⭐ Reviews & Ratings
- **Order Reviews**: Review after order completion
- **Star Ratings**: 1-5 star ratings
- **Image Support**: Attach images to reviews
- **Store Reply**: Stores can reply to reviews
- **Statistics**: Average rating and review count

### 5. 🎫 Voucher System
- **Voucher Types**: Percentage, Fixed Amount, Free Shipping
- **Usage Limits**: Total and per-user limits
- **Store-Specific**: Platform-wide or store-specific vouchers
- **Validity Period**: Start and end dates

### 6. ❤️ Favorites
- **Store Favorites**: Save favorite stores
- **Food Favorites**: Save favorite food items
- **Quick Access**: Easy reordering from favorites

### 7. 🔔 Notifications
- **Order Updates**: Status change notifications
- **Wallet Updates**: Deposit/payment notifications
- **Promotions**: Voucher and promotion alerts
- **Read Status**: Mark as read functionality

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server
- Visual Studio 2022 / VS Code

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/xuanhien010204/grab_food_backend.git
cd FoodOrderingPRM392
```

2. **Update connection string** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "FOOD": "Your_Connection_String_Here"
  }
}
```

3. **Run database migrations**:
```bash
dotnet ef database update --project FoodOrderingPRM392
```

4. **Or run SQL scripts manually**:
```
FoodOrderingPRM392/Migrations/Scripts/AddOrderStatusAndPayment.sql
FoodOrderingPRM392/Migrations/Scripts/AddAllNewFeatures.sql
```

5. **Run the application**:
```bash
dotnet run --project FoodOrderingPRM392
```

6. **Access Swagger UI**:
```
https://localhost:5001/swagger
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "FOOD": "Server=...;Database=grab-food;..."
  },
  "MoMo": {
    "PartnerCode": "MOMOBKUN20180529",
    "AccessKey": "your_access_key",
    "SecretKey": "your_secret_key",
    "ApiEndpoint": "https://test-payment.momo.vn",
    "NotifyUrl": "https://your-domain/api/wallet/momo/ipn",
    "ReturnUrl": "https://your-domain/api/wallet/momo/return"
  }
}
```

---

## 📁 Project Structure

```
FoodOrderingPRM392/
├── FoodOrderingCore/              # Domain Layer
│   ├── ConfigurationOptions/      # App settings classes
│   │   ├── ConnectionOption.cs
│   │   └── MomoOption.cs
│   ├── Constants/                 # Message constants
│   │   ├── ResponseMessages.cs
│   │   ├── OrderMessages.cs
│   │   ├── WalletMessages.cs
│   │   └── FeatureMessages.cs
│   ├── Context/                   # EF Core DbContext
│   │   └── FoodOrderingContext.cs
│   ├── Data/                      # Entity models
│   │   ├── User.cs
│   │   ├── Store.cs
│   │   ├── Food.cs
│   │   ├── FoodStore.cs
│   │   ├── Order.cs
│   │   ├── OrderDetail.cs
│   │   ├── WalletTransaction.cs
│   │   ├── DeliveryAddress.cs
│   │   ├── Review.cs
│   │   ├── Voucher.cs
│   │   ├── Favorite.cs
│   │   └── Notification.cs
│   ├── Dto/                       # Data Transfer Objects
│   ├── Enum/                      # Enumerations
│   ├── Exceptions/                # Custom exceptions
│   ├── Helpers/                   # Utility classes
│   ├── Request/                   # API request models
│   └── Response/                  # API response models
│
├── FoodOrderingRepository/        # Business Logic Layer
│   ├── Interface/                 # Repository contracts
│   │   ├── IUserRepository.cs
│   │   ├── IStoreRepository.cs
│   │   ├── IOrderRepository.cs
│   │   ├── IWalletService.cs
│   │   ├── IDeliveryAddressRepository.cs
│   │   ├── IReviewRepository.cs
│   │   ├── IVoucherRepository.cs
│   │   ├── IFavoriteRepository.cs
│   │   └── INotificationRepository.cs
│   └── Implement/                 # Repository implementations
│
├── FoodOrderingPRM392/            # API Layer
│   ├── Controllers/               # API controllers
│   │   ├── UserController.cs
│   │   ├── WalletController.cs
│   │   ├── OrderController.cs
│   │   ├── StoreController.cs
│   │   ├── FoodController.cs
│   │   ├── FoodStoreController.cs
│   │   ├── DeliveryAddressController.cs
│   │   ├── ReviewController.cs
│   │   ├── VoucherController.cs
│   │   ├── FavoriteController.cs
│   │   └── NotificationController.cs
│   ├── Extension/                 # DI extensions
│   ├── Extensions/                # Helper extensions
│   ├── Middlewares/               # Global middlewares
│   ├── Filters/                   # Exception filters
│   ├── Migrations/                # EF Core migrations
│   │   └── Scripts/               # SQL scripts
│   ├── Program.cs                 # App entry point
│   └── appsettings.json          # Configuration
```

---

## 🔐 Authentication

The API uses **Cookie-based Authentication** with the following setup:

### Login Flow
1. User calls `POST /api/users/login` with email/password
2. Server validates credentials
3. Server creates authentication cookie with claims
4. Cookie is sent with subsequent requests

### Claims in Cookie
```csharp
- ClaimTypes.NameIdentifier  → User ID
- ClaimTypes.Email           → Email
- ClaimTypes.MobilePhone     → Phone
- ClaimTypes.Role            → Role name
- "WalletAmount"             → Wallet balance
- "RoleId"                   → Role ID
```

### Cookie Settings
```csharp
- HttpOnly: true
- SameSite: Lax
- ExpireTimeSpan: 168 hours (7 days)
- SlidingExpiration: false
```

---

## 💳 Payment Integration

### MoMo Payment Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      MoMo Payment Flow                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. User requests deposit                                        │
│     POST /api/wallet/deposit { amount: 100000 }                 │
│                          │                                       │
│                          ▼                                       │
│  2. Server creates MoMo payment request                          │
│     - Generate unique OrderId                                    │
│     - Calculate HMAC signature                                   │
│     - Call MoMo API                                              │
│                          │                                       │
│                          ▼                                       │
│  3. Return payment URL to user                                   │
│     { payUrl: "https://test-payment.momo.vn/..." }              │
│                          │                                       │
│                          ▼                                       │
│  4. User completes payment on MoMo                               │
│                          │                                       │
│            ┌─────────────┴─────────────┐                        │
│            ▼                           ▼                        │
│  5a. MoMo sends IPN              5b. User redirected            │
│      POST /api/wallet/momo/ipn       GET /api/wallet/momo/return│
│            │                                                     │
│            ▼                                                     │
│  6. Verify signature & process                                   │
│     - Validate HMAC signature                                    │
│     - Update wallet balance                                      │
│     - Create transaction record                                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### OrderId Format
```
DEPOSIT_{UserId}_{Timestamp}_{Random}
Example: DEPOSIT_123_1706789012345_A1B2
```

---

## 📝 API Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "message": "Operation completed successfully",
  "result": { ... }
}
```

### Error Response
```json
{
  "message": "Error description"
}
```

### Response Classes
```csharp
// Basic response
public class ParentResponse
{
    public string Message { get; set; }
}

// Response with data
public class ParentResultResponse : ParentResponse
{
    public object Result { get; set; }
}
```

---

## 🧪 Testing

### Postman Collection
Import `Wallet_MoMo_Payment_Testing.postman_collection.json` for ready-to-use API tests.

### Test Endpoints
```bash
# Health check
GET /swagger

# Login
POST /api/users/login
{
  "email": "test@example.com",
  "password": "password123"
}

# Get wallet balance
GET /api/wallet/balance

# Create deposit
POST /api/wallet/deposit
{
  "amount": 100000,
  "orderInfo": "Test deposit"
}
```

---

## 📄 License

This project is for educational purposes.

---

## 👥 Contributors

- **Xuan Hien** - Backend Developer

---

## 📞 Support

For support, email xuanhien010204@gmail.com or create an issue on GitHub.

---

**Last Updated:** January 2025
