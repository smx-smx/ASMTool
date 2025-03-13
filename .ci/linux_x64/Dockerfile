FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && \
    apt-get install -y build-essential gcc g++ cmake pkg-config p7zip-full libpci-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && dotnet nuget locals all --clear
    # reserved for cache eviction: 0
