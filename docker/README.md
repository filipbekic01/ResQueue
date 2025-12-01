# Run MSSQL on macos

docker run --platform=linux/amd64 \
  -e "ACCEPT_EULA=Y" \
  -e 'SA_PASSWORD=Abcd1234!' \
  -p 1433:1433 \
  -v sqlserver-data:/var/opt/mssql \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest