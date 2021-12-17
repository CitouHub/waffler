#run the setup script to create the DB
#do this in a loop because the timing for when the SQL instance is ready is indeterminate
for i in {1..50};
do
    echo "Trying to connect to SQL Server to set up database"
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i exists.sql
    
    if [ $? -eq 0 ]
    then
        EXISTS=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i exists.sql)
        if [ $EXISTS = "FALSE" ];
        then
            echo "Database does not exist, creating..."
            /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i create.sql
            /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterTables.sql
            /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterData.sql
            echo "Database created"
        else 
            echo "Database exists"
        fi
    
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterStoredProcedure.sql

        echo "Database setup complete"
        break
    else
        echo "SQL Server not ready yet..."
        sleep 1
    fi
done