# Docker

## Unit testing with `Testcontainers`

The tests in this project use [testcontainers](https://testcontainers.com/guides/getting-started-with-test-containers-for-dotnet).

## Local/Manual testing

To play around locally with a variety of databases to test SQL statements on, you can use Docker.

Here are some shortcut one-liners to get some databases up and running for your favorite IDE (e.g., DBeaver, DataGrip, etc...).

### PostgreSQL

Start a container, and persist the volume:

```sh
docker run --rm --name test_postgres15 \
    -e PGDATA=/var/lib/postgresql/data/pgdata \
    -e POSTGRES_PASSWORD=Pa33w0rd! \
    -p 2432:5432 \
    -d \
    -v test_postgres_data:/var/lib/postgresql/data \
    postgis/postgis:15-3.4
```

Stop the container (also deletes it if it was started with `--rm`):

```sh
docker stop test_postgres
```

### MySQL

Start a container, and persist the volume:

```sh
docker run --rm --name test_mysql84 \
    -e MYSQL_DATABASE=testdb \
    -e MYSQL_ROOT_PASSWORD=Pa33w0rd! \
    -p 2306:3306 \
    -d \
    -v test_mysql_data:/var/lib/mysql \
    mysql:8.4
```

Stop the container (also deletes it if it was started with `--rm`):

```sh
docker stop test_mysql84
```

### SQL Server

Start a container, and persist the volume:

```sh
docker run --rm --name test_mssql19 \
    --user root \
    -e ACCEPT_EULA=Y \
    -e MSSQL_SA_PASSWORD=Pa33w0rd! \
    -p 2433:1433 \
    -d \
    -v test_mssql_data:/var/opt/mssql/data \
    mcr.microsoft.com/mssql/server:2019-latest
```

Stop it with

```sh
docker stop test_mssql19
```

### SQLite

No containers necessary for SQLite, just connect to a file on your local filesystem.

### Sample Data

You can use the SQL scripts in the [cristiscu/employees-test-database](https://github.com/cristiscu/employees-test-database/tree/master/scripts) repository to setup a common database in each provider.

    Copies of these scripts are also provided in this `ref` folder.
