FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY ../LhDev.ShoppingLister/bin/Release/net8.0/publish/* /app
USER app
WORKDIR /app
ENTRYPOINT ["dotnet", "LhDev.ShoppingLister.dll"]