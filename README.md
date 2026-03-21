# Hammer Support

Hammer 경매 플랫폼의 Support Service.

## Stack

- ASP.NET (.NET 10)
- Kafka (이벤트 소비)

## Features

- 요청/이벤트 로그 수집 및 저장
- FCM 푸시 알림
- 기타 cross-cutting 서포트 기능

## Services

| Service | Description |
|---------|-------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway) | API Gateway |
| [hammer-user](https://github.com/HoBom-s/hammer-user) | User & Auth |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction) | Auction API |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | Data Collector |
| [hammer-support](https://github.com/HoBom-s/hammer-support) | Logging, FCM, Support |

## Getting Started

```bash
dotnet restore
dotnet run --project src/Hammer.Support
```

## Branch Strategy

- `main` — Production
- `develop` — Development (default)
