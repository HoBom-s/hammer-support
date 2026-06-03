# Hammer Support

Hammer 경매 플랫폼의 Support Service.

## Stack

- ASP.NET (.NET 10)
- Kafka (이벤트 발행)

## Features

- 온비드(캠코) 공매 데이터 수집 → Kafka 발행 (매일 배치, 실행 시각 설정 가능, stateless)
- Expo Push API 푸시 알림
- 기타 cross-cutting 서포트 기능

## Architecture

```
hammer-support → 온비드 API 수집 → Kafka → hammer-auction (DB 저장, 캐싱, 조회)
hammer-collector ← 각 서비스 로그 수집 → ELK
```

## Data Source

- [캠코공매물건 조회 API](https://www.data.go.kr/data/15000851/openapi.do)
- [이용기관 공매물건 조회 API](https://www.data.go.kr/data/15000849/openapi.do)
- [온비드 코드 조회 API](https://www.data.go.kr/data/15000920/openapi.do)

## Services

| Service | Description |
|---------|-------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway) | API Gateway |
| [hammer-user](https://github.com/HoBom-s/hammer-user) | User & Auth |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction) | BFF, Auction API |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | ELK Logging |
| [hammer-support](https://github.com/HoBom-s/hammer-support) | Data Fetching, Push Notification, Support |

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `Onbid__ServiceKey` | - | 온비드 API 인증키 (필수) |
| `Onbid__CollectionHour` | `9` | 배치 실행 시각 (0-23, KST) |
| `Onbid__PageSize` | `100` | API 페이지당 조회 건수 |
| `Kafka__BootstrapServers` | `localhost:9092` | Kafka 브로커 주소 |

## Getting Started

```bash
cp .env.example .env.local   # ServiceKey 등 환경변수 설정
dotnet restore
dotnet run --project src/Hammer.Support.Api
```

## Branch Strategy

- `main` — Production
- `develop` — Development (default)
