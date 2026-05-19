# --- Stage 1: Base Runtime ---
# Usamos a imagem base com as bibliotecas necessárias para evitar o erro de libgssapi
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Instala a dependência que faltava para o Npgsql (driver do PostgreSQL) funcionar no Linux
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

# --- Stage 2: Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Inkdrop.Api/Inkdrop.Api.csproj", "Inkdrop.Api/"]
RUN dotnet restore "Inkdrop.Api/Inkdrop.Api.csproj"

COPY . .
WORKDIR "/src/Inkdrop.Api"
RUN dotnet build "Inkdrop.Api.csproj" -c Release -o /app/build

# --- Stage 3: Publish ---
FROM build AS publish
RUN dotnet publish "Inkdrop.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Stage 4: Final Image ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Inkdrop.Api.dll"]