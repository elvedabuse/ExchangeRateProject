# ExchangeRateProject

TCMB (Türkiye Cumhuriyet Merkez Bankası) verilerini kullanarak günlük ve geçmiş tarihli döviz kurlarını sunan, ASP.NET Core Web API ile geliştirilmiş bir backend projesidir.

Proje; döviz kurlarının TCMB üzerinden alınması, Memory Cache üzerinde tutulması, veritabanına kaydedilmesi ve API üzerinden güncel veya geçmiş tarihli olarak sorgulanması amacıyla geliştirilmiştir.

## 🚀 Projenin Amacı

Bu projenin temel amacı, TCMB tarafından yayınlanan döviz kuru verilerini bir backend uygulaması üzerinden yönetmek ve farklı API endpoint'leri aracılığıyla erişilebilir hale getirmektir.

Proje kapsamında:

* Günlük döviz kurları TCMB'den alınır.
* Alınan veriler Memory Cache üzerinde tutulur.
* Döviz kurları SQL Server veritabanına kaydedilir.
* Belirli bir saatte günlük kur güncellemesi otomatik olarak gerçekleştirilir.
* Kullanıcı tarafından yeni para birimleri sisteme eklenebilir.
* Geçmiş tarihlere ait döviz kurları sorgulanabilir.
* Belirli bir tarih aralığındaki döviz kurları sorgulanabilir.
* API işlemleri Swagger ve Postman üzerinden test edilebilir.

## 🛠️ Kullanılan Teknolojiler

* **C#**
* **.NET 10**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server / LocalDB**
* **Memory Cache**
* **Swagger / OpenAPI**
* **Postman**
* **TCMB XML API**
* **Clean Architecture**

## 🏗️ Proje Mimarisi

Proje, sorumlulukların birbirinden ayrılması amacıyla Clean Architecture yaklaşımına uygun olarak katmanlara ayrılmıştır.

```text
ExchangeRateProject
│
├── ExchangeRate.Api
│   └── Controllers
│
├── ExchangeRate.Application
│   ├── Interfaces
│   ├── Services
│   └── DTOs
│
├── ExchangeRate.Domain
│   └── Entities
│
├── ExchangeRate.Infrastructure
│   ├── Data
│   ├── Repositories
│   ├── Services
│   ├── Background Services
│   └── Migrations
│
└── ExchangeRateProject.slnx
```

### Katmanların Görevleri

**ExchangeRate.Api**

HTTP isteklerini karşılayan API katmanıdır. Controller'lar bu katmanda bulunmaktadır.

**ExchangeRate.Application**

Uygulamanın iş kurallarının ve servislerinin bulunduğu katmandır. Interface, servis ve DTO yapıları burada yer almaktadır.

**ExchangeRate.Domain**

Uygulamanın temel entity'lerini içeren katmandır. Veritabanına veya dış servislere bağımlı değildir.

**ExchangeRate.Infrastructure**

Veritabanı işlemleri, repository yapıları, TCMB ile iletişim, Memory Cache ve background service gibi dış sistemlerle ilgili işlemler bu katmanda gerçekleştirilmektedir.

## 📌 Temel Özellikler

### 1. TCMB'den Günlük Kur Alma

TCMB'nin yayınladığı XML verileri kullanılarak günlük döviz kurları alınmaktadır.

Örnek olarak:

* USD
* EUR
* GBP
* JPY

gibi para birimleri sistem tarafından desteklenmektedir.

### 2. Memory Cache

TCMB'den alınan günlük kur verileri tekrar tekrar dış servisten alınmaması için Memory Cache üzerinde tutulmaktadır.

Bu sayede aynı veriye tekrar ihtiyaç duyulduğunda TCMB'ye yeniden istek gönderilmesi engellenerek gereksiz HTTP istekleri azaltılmaktadır.

### 3. Veritabanına Kayıt

Alınan döviz kurları SQL Server LocalDB üzerinde bulunan `ExchangeRateDb` veritabanında saklanmaktadır.

Temel tablolar:

```text
Currencies
CurrencyRates
__EFMigrationsHistory
```

### 4. Otomatik Günlük Güncelleme

`DailyRateJob` isimli Background Service kullanılarak günlük döviz kuru güncellemesi otomatik olarak gerçekleştirilmektedir.

Güncelleme işlemi hafta içi günlerde saat **15:30** civarında çalışacak şekilde yapılandırılmıştır.

### 5. Dinamik Para Birimi Ekleme

Sistemde yalnızca başlangıçta belirlenen para birimlerine bağlı kalmadan, TCMB tarafından desteklenen yeni para birimleri API üzerinden eklenebilmektedir.

Örnek:

```http
POST /api/Currencies?currencyCode=JPY
```

Sisteme daha önce eklenmiş bir para biriminin tekrar eklenmesi engellenmektedir.

### 6. Geçmiş Kur Sorgulama

Belirli bir tarih ve para birimi için geçmiş döviz kuru sorgulanabilmektedir.

Örnek:

```http
GET /api/ExchangeRates/historical?date=2026-09-02&currencyCode=USD
```

### 7. Tarih Aralığında Kur Sorgulama

Belirli bir tarih aralığında seçilen para birimine ait kayıtlar API üzerinden sorgulanabilmektedir.

Örnek:

```http
GET /api/Exchanges/historical?startDate=2026-09-01&endDate=2026-09-08&currencyCode=USD
```

### 8. Güncel Kurları Listeleme

Sisteme kaydedilmiş güncel günlük kurlar aşağıdaki endpoint üzerinden görüntülenebilir:

```http
GET /api/Exchanges
```

Bu endpoint geçmişteki tüm kayıtları değil, sistemdeki güncel günlük kur kayıtlarını döndürmek amacıyla kullanılmaktadır.

## 🔌 API Endpoint'leri

| Method | Endpoint                           | Açıklama                                              |
| ------ | ---------------------------------- | ----------------------------------------------------- |
| GET    | `/api/Exchanges`                   | Güncel günlük kurları getirir                         |
| GET    | `/api/Exchanges/historical`        | Tarih aralığındaki kuru getirir                       |
| GET    | `/api/ExchangeRates/historical`    | Belirli tarih ve para birimi için geçmiş kuru getirir |
| POST   | `/api/ExchangeRates/update`        | Günlük kurları manuel olarak günceller                |
| POST   | `/api/Currencies?currencyCode=USD` | Yeni para birimi ekler                                |

## 🧪 Swagger

API endpoint'lerinin test edilmesi ve API yapısının incelenmesi için Swagger/OpenAPI kullanılmıştır.

Projeyi çalıştırdıktan sonra Swagger arayüzü üzerinden endpoint'ler görüntülenebilir ve test edilebilir.

Örnek:

```text
https://localhost:7188/swagger
```

Port numarası çalıştırılan ortama göre değişebilir.

## 📮 Postman

API endpoint'lerinin manuel olarak test edilmesi amacıyla Postman kullanılmıştır.

Postman üzerinden:

* Günlük kur güncelleme
* Para birimi ekleme
* Geçmiş kur sorgulama
* Tarih aralığında kur sorgulama
* Güncel kurları listeleme

işlemleri test edilmiştir.

## 🗄️ Veritabanı

Proje geliştirme ortamında **SQL Server LocalDB** kullanmaktadır.

Veritabanı adı:

```text
ExchangeRateDb
```

Temel tablolar:

```text
Currencies
CurrencyRates
```

Entity Framework Core Migrations kullanılarak veritabanı şeması yönetilmektedir.

## ▶️ Projeyi Çalıştırma

### 1. Projeyi Klonlama

```bash
git clone https://github.com/elvedabuse/ExchangeRateProject.git
```

### 2. Proje klasörüne geçme

```bash
cd ExchangeRateProject
```

### 3. Projeyi Visual Studio ile açma

Aşağıdaki solution dosyası Visual Studio ile açılabilir:

```text
ExchangeRateProject.slnx
```

### 4. Veritabanını Oluşturma

LocalDB'nin bilgisayarda kurulu olduğundan emin olduktan sonra gerekli migration'lar uygulanabilir.

Package Manager Console üzerinden:

```powershell
Update-Database -Project ExchangeRate.Infrastructure -StartupProject ExchangeRate.Api
```

veya .NET CLI kullanılarak:

```bash
dotnet ef database update --project ExchangeRate.Infrastructure --startup-project ExchangeRate.Api
```

### 5. Projeyi Çalıştırma

`ExchangeRate.Api` projesi çalıştırıldığında API kullanılabilir hale gelir.

Swagger üzerinden endpoint'lere erişilebilir.

## 🔄 Döviz Kuru Veri Akışı

Projedeki temel veri akışı aşağıdaki şekildedir:

```text
TCMB
  │
  ▼
TcmbService
  │
  ▼
Günlük Kur Verileri
  │
  ├──────────────► Memory Cache
  │
  ▼
Application Services
  │
  ▼
Repository
  │
  ▼
SQL Server LocalDB
  │
  ▼
ASP.NET Core API
  │
  ▼
Swagger / Postman / API Client
```

## 🔐 Veri ve Yapı

Projede veritabanı işlemleri Infrastructure katmanında tutulmuş, API katmanının doğrudan veritabanı işlemleri yapmasının önüne geçilmiştir.

Bu yapı sayesinde:

* Katmanlar arasındaki bağımlılık azaltılmıştır.
* İş kuralları Application katmanında tutulmuştur.
* Veritabanı işlemleri Infrastructure katmanında ayrıştırılmıştır.
* API katmanı yalnızca HTTP isteklerinin yönetimine odaklanmıştır.

## 📚 Geliştirme Süreci

Bu proje bir backend geliştirme ve staj projesi kapsamında geliştirilmiştir.

Geliştirme sürecinde aşağıdaki konular üzerinde çalışılmıştır:

* ASP.NET Core Web API
* Clean Architecture
* Entity Framework Core
* Repository Pattern
* Memory Cache
* Background Service
* REST API
* SQL Server
* API validation
* Swagger/OpenAPI
* Postman
* TCMB XML verilerinin işlenmesi
* Entity Framework Core Migrations

## 📌 Proje Durumu

Proje geliştirme aşamasındadır ve yeni özellikler eklenerek geliştirilmeye devam etmektedir.
