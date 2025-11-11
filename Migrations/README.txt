Para generar las migraciones y la base de datos:

dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
