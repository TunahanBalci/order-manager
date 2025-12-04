FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["OrderApi/OrderApi.csproj", "OrderApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Messaging/Messaging.csproj", "Messaging/"]
RUN dotnet restore "OrderApi/OrderApi.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/OrderApi"
RUN dotnet build "OrderApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderApi.dll"]
