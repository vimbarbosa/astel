FROM mcr.microsoft.com/mssql/server:2022-latest

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=stel@123
ENV MSSQL_PID=Developer

USER root

# Instala mssql-tools18
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl apt-transport-https ca-certificates gnupg && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    echo "deb [arch=amd64] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18 mssql-tools18 unixodbc-dev && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/mssql-tools18/bin:/opt/mssql-tools/bin:${PATH}"

# Copia todos os arquivos SQL e de inicialização
COPY ./sql /sql
RUN chmod +x /sql/init.sh

USER mssql
ENTRYPOINT ["/bin/bash", "/sql/init.sh"]
