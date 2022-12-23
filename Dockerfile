

FROM centos:7 AS base
# Add Microsoft package repository and install ASP.NET Core
RUN rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm \
    && yum install -y aspnetcore-runtime-6.0

# Ensure we listen on any IP Address 
ENV DOTNET_URLS=http://+:5000

WORKDIR /app

# ... remainder of dockerfile as before
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ReverseProxy/ReverseProxy.csproj", "."]
RUN dotnet restore "ReverseProxy/ReverseProxy.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ReverseProxy/ReverseProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReverseProxy/ReverseProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReverseProxy.dll"]