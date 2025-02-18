# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MyChatApp/MyChatApp.csproj", "MyChatApp/"]
RUN dotnet restore "MyChatApp/MyChatApp.csproj"
COPY . .
WORKDIR "/src/MyChatApp"
RUN dotnet build "MyChatApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyChatApp.csproj" -c Release -o /app/publish

# Copy the published app to the base image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyChatApp.dll"]
