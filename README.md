# Product Management Microservices

Bu proje, .NET 9 ile geliştirilmiş, modern mikroservis mimarisini kullanan bir ürün yönetim sistemidir. Clean Architecture, CQRS pattern, Event-Driven Architecture ve 12-Factor App prensipleri uygulanmıştır.

## 🏗️ Mimari Genel Bakış

Proje, birbirinden bağımsız çalışabilen 3 ana mikroservisten oluşmaktadır:

### Mikroservisler
- **Auth Service** (Port: 5001) - Kimlik doğrulama ve yetkilendirme
- **Product Service** (Port: 5002) - Ürün yönetimi ve CRUD işlemleri  
- **Log Service** (Port: 5003) - Merkezi loglama sistemi

### Event-Driven Communication
- **RabbitMQ** - Mikroservisler arası asenkron mesajlaşma
- **MassTransit** - .NET için event bus implementasyonu```# 🛠️ Teknoloji Stack

### Backend
- **.NET 9** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API geliştirme
- **Entity Framework Core** - ORM ve database operations
- **PostgreSQL** - İlişkisel veritabanı
- **Redis** - In-memory caching
- **RabbitMQ** - Message broker
- **MassTransit** - Service bus abstraction

### Libraries & Tools
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Swashbuckle** - API documentation
- **BCrypt.Net** - Password hashing

## 📁 Proje Yapısı

```
src/
├── Services/
│   ├── Auth/
│   │   ├── Auth.API/           # Web API Controller'ları
│   │   ├── Auth.Application/   # Business Logic, CQRS
│   │   ├── Auth.Core/          # Domain Entities, Interfaces
│   │   └── Auth.Infrastructure/ # Data Access, External Services
│   ├── Product/
│   │   ├── Product.API/
│   │   ├── Product.Application/
│   │   ├── Product.Core/
│   │   └── Product.Infrastructure/
│   └── Log/
│       └── Log.API/            # Centralized Logging Service
├── Shared/
│   └── Shared/                 # Common models, events, services
```

## 🚀 Kurulum

### Ön Gereksinimler
- .NET 9 SDK
- PostgreSQL
- Redis
- RabbitMQ
- Docker (opsiyonel)

### Veritabanı Kurulumu

```
# PostgreSQL connection string'i güncelle
# appsettings.json dosyalarında connection string'leri düzenle

# Migration'ları çalıştır
cd src/Services/Auth/Auth.Infrastructure
dotnet ef database update --startup-project ../Auth.API

cd src/Services/Product/Product.Infrastructure  
dotnet ef database update --startup-project ../Product.API
```

### Redis ve RabbitMQ (Docker)

```
# Redis
docker run -d --name redis -p 6379:6379 redis:alpine

# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## ▶️ Servisleri Çalıştırma

### Manuel Başlatma

```
# Terminal 1 - Auth Service
cd src/Services/Auth/Auth.API
dotnet run --urls="https://localhost:5001"

# Terminal 2 - Product Service  
cd src/Services/Product/Product.API
dotnet run --urls="https://localhost:5002"

# Terminal 3 - Log Service
cd src/Services/Log/Log.API
dotnet run --urls="https://localhost:5003"
```

### API Endpoints

#### Auth Service (https://localhost:5001)
- `POST /api/v1/auth/register` - Kullanıcı kaydı
- `POST /api/v1/auth/login` - Kullanıcı girişi
- `POST /api/v1/auth/refresh` - Token yenileme

#### Product Service (https://localhost:5002)  
- `GET /api/v1/products` - Tüm ürünler
- `GET /api/v1/products/{id}` - Ürün detayı
- `POST /api/v1/products` - Ürün oluştur (JWT gerekli)
- `PUT /api/v1/products/{id}` - Ürün güncelle (JWT gerekli)
- `DELETE /api/v1/products/{id}` - Ürün sil (JWT gerekli)
- `GET /api/v1/products/my-products` - Kullanıcının ürünleri

#### Log Service (https://localhost:5003)
- `GET /api/v1/log/test` - Servis durumu
- `POST /api/v1/log/information` - Info log
- `POST /api/v1/log/error` - Error log

## 🧪 Test Etme

### 1. Kullanıcı Kaydı
```
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Kullanıcı Girişi
```
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com", 
    "password": "TestPassword123!"
  }'
```

### 3. Ürün Oluşturma (JWT Token Gerekli)
```
curl -X POST https://localhost:5002/api/v1/products \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Ürün",
    "description": "Test açıklaması", 
    "price": 99.99,
    "stock": 10,
    "category": 0
  }'
```

## 📊 Monitoring

### Swagger UI
- Auth Service: https://localhost:5001/swagger
- Product Service: https://localhost:5002/swagger  
- Log Service: https://localhost:5003/swagger

### RabbitMQ Management
- URL: http://localhost:15672
- Username: guest / Password: guest

## 🏛️ Mimari Prensipleri

### Clean Architecture (Onion Architecture)
- **Domain Layer** - Business entities ve kurallar
- **Application Layer** - Use cases ve business logic
- **Infrastructure Layer** - External concerns (DB, cache, messaging)
- **API Layer** - Controllers ve HTTP concerns

### CQRS (Command Query Responsibility Segregation)
- **Commands** - Veri değiştiren işlemler
- **Queries** - Veri okuma işlemleri
- **Handlers** - Business logic implementation

### Event-Driven Architecture
- **Domain Events** - Business olayları
- **Event Handlers** - Olay işleyicileri
- **Message Bus** - Mikroservisler arası iletişim

## 🔒 Güvenlik

- **JWT Authentication** - Stateless token-based auth
- **Password Hashing** - BCrypt ile güvenli şifre saklama
- **CORS** - Cross-origin request handling
- **Input Validation** - FluentValidation ile comprehensive```lidation


## 📈 Performance

### Caching Strategy
- **Redis** - Distributed caching
- **Cache Invalidation** - Smart cache management
- **Query Optimization** - EF Core best practices

### Scalability
- **Stateless Services** - Horizontal scaling ready
- **Database Indexing** - Optimized queries
- **Async Processing** - Non-blocking operations

