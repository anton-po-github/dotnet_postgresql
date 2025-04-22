# Stage 1: билд
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Гарантируем, что Kestrel подхватит PORT=8080
ENV ASPNETCORE_URLS=http://+:8080

ENV PORT=8080

ENTRYPOINT ["dotnet", "dotnet_postgresql.dll"]
