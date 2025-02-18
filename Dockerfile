# Use the official .NET image as the base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the .csproj file into the container and restore dependencies
COPY MyChatApp/MyChatApp.csproj ./MyChatApp/
RUN dotnet restore ./MyChatApp/MyChatApp.csproj

# Copy the rest of the source code into the container
COPY . .

# Build the application
WORKDIR /src/MyChatApp
RUN dotnet build MyChatApp.csproj -c Release -o /app/build

# Publish the application to the /app/publish folder
FROM build AS publish
RUN dotnet publish MyChatApp.csproj -c Release -o /app/publish

# Set up the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define the entry point to run the app
ENTRYPOINT ["dotnet", "MyChatApp.dll"]
