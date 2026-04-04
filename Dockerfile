FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Hammer.Support.slnx ./
COPY src/Hammer.Support.Domain/Hammer.Support.Domain.csproj src/Hammer.Support.Domain/
COPY src/Hammer.Support.Application/Hammer.Support.Application.csproj src/Hammer.Support.Application/
COPY src/Hammer.Support.Infrastructure/Hammer.Support.Infrastructure.csproj src/Hammer.Support.Infrastructure/
COPY src/Hammer.Support.Api/Hammer.Support.Api.csproj src/Hammer.Support.Api/
RUN dotnet restore src/Hammer.Support.Api/Hammer.Support.Api.csproj

COPY src/ src/
RUN dotnet publish src/Hammer.Support.Api/Hammer.Support.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --no-create-home appuser

COPY --from=build /app .

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Hammer.Support.Api.dll"]
