FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Patogh.Domain/Patogh.Domain.csproj", "Patogh.Domain/"]
COPY ["src/Patogh.Application/Patogh.Application.csproj", "Patogh.Application/"]
COPY ["src/Patogh.Infrastructure/Patogh.Infrastructure.csproj", "Patogh.Infrastructure/"]
COPY ["src/Patogh.Persistance/Patogh.Persistance.csproj", "Patogh.Persistance/"]
COPY ["src/Patogh.API/Patogh.API.csproj", "Patogh.API/"]

RUN dotnet restore "src/Patogh.API/Patogh.API.csproj"

COPY .. .

WORKDIR /src/Patogh.API
RUN dotnet publish "Patogh.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

RUN mkdir -p /app/Uploads /app/logs && chown -R appuser:appgroup /app

COPY --from=build /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "Patogh.API.dll"]