FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN apt-get update \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && dotnet nuget locals all --clear
    # reserved for cache eviction: 0
