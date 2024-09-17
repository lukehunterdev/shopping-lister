FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_ENVIRONMENT=Prod
RUN mkdir -p /app/data
RUN chown app:users /app
COPY LhDev.ShoppingLister/bin/Release/net8.0/publish /app
RUN mkdir -p /app/data
RUN chown app:users /app/data
USER app
WORKDIR /app
ENTRYPOINT ["dotnet", "LhDev.ShoppingLister.dll"]