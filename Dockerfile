# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire solution file and restore dependencies for all projects
COPY *.sln ./
COPY Application/Application.csproj ./Application/
COPY DataModel/DataModel.csproj ./DataModel/
COPY Domain/Domain.csproj ./Domain/
COPY Gateway/Gateway.csproj ./Gateway/
COPY WebApi/WebApi.csproj ./WebApi/

# Restore dependencies for all projects
RUN dotnet restore

# Copy the rest of the files and build the application
COPY . ./
RUN dotnet publish -c Release -o /app

# Use the official ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Expose the port the application runs on
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "WebApi.dll"]