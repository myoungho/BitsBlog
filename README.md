# BitsBlog

ASP.NET Core 8 WebAPI + Entity Framework + React + Bits UI 예제. 클린 아키텍처를 기반으로 한 간단한 게시판(BBS) 구현입니다.

## 프로젝트 구조

- `src/BitsBlog.Domain` - 도메인 엔터티
- `src/BitsBlog.Application` - 서비스 및 인터페이스
- `src/BitsBlog.Infrastructure` - EF Core DbContext 및 리포지토리
- `src/BitsBlog.WebApi` - REST API
- `src/BitsBlog.Web` - ASP.NET Core MVC 클라이언트
- `client-react` - React + Bits UI 클라이언트 샘플

## 실행

```bash
# Web API
cd src/BitsBlog.WebApi
# dotnet run

# MVC 클라이언트
cd src/BitsBlog.Web
# dotnet run

# React 클라이언트 (예: Vite 등으로 빌드)
cd client-react
# cp ../.env.example .env # 환경 변수 설정
# npm install && npm run dev
```

`.env` 파일의 `VITE_API_URL` 값은 백엔드 Web API의 기본 주소를 가리켜야 합니다.

현재 환경에서는 .NET SDK가 설치되어 있지 않으므로 `dotnet` 명령이 실행되지 않을 수 있습니다.
