Install Docker
Download and Run Redis Image
docker run --name dist-redis -p  6379:6379 -d redis

Migrations
dotnet ef migrations add "init"
