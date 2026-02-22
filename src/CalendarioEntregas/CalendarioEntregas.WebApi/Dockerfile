FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CalendarioEntregas.WebApi/CalendarioEntregas.WebApi.csproj", "CalendarioEntregas.WebApi/"]
COPY ["CalendarioEntregas.Application/CalendarioEntregas.Application.csproj", "CalendarioEntregas.Application/"]
COPY ["CalendarioEntregas.Domain/CalendarioEntregas.Domain.csproj", "CalendarioEntregas.Domain/"]
COPY ["CalendarioEntregas.Infrastructure/CalendarioEntregas.Infrastructure.csproj", "CalendarioEntregas.Infrastructure/"]

RUN dotnet restore "CalendarioEntregas.WebApi/CalendarioEntregas.WebApi.csproj"

COPY [".", "."]

RUN dotnet build "CalendarioEntregas.WebApi/CalendarioEntregas.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CalendarioEntregas.WebApi/CalendarioEntregas.WebApi.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 7020 7021

ENTRYPOINT ["dotnet", "CalendarioEntregas.WebApi.dll"]
