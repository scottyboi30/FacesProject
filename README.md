# FacesProject


# Docker containers
docker run -p 15672:15672 -p5672:5672 --name rabbit-corona rabbitmq:3-management
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Passw0rd(!)" --name ordermssql -p 1445:1433 mcr.microsoft.com/mssql/server:2017-latest-ubuntu