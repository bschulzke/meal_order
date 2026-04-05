FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MealOrder.sln ./
COPY MealOrder.Api/MealOrder.Api.csproj MealOrder.Api/
RUN dotnet restore

COPY . .
RUN dotnet publish MealOrder.Api/MealOrder.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MealOrder.Api.dll"]
