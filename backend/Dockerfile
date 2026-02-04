
# .NET 10 (LTS) SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src/Orders.Domain/Orders.Domain.csproj ./src/Orders.Domain/
COPY ./src/Orders.Application/Orders.Application.csproj ./src/Orders.Application/
COPY ./src/Orders.Infrastructure/Orders.Infrastructure.csproj ./src/Orders.Infrastructure/
COPY ./src/Orders.Api/Orders.Api.csproj ./src/Orders.Api/

RUN dotnet restore ./src/Orders.Api/Orders.Api.csproj

COPY . .
RUN dotnet publish ./src/Orders.Api/Orders.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
