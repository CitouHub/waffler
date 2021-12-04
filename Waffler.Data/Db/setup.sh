#run the setup script to create the DB
#do this in a loop because the timing for when the SQL instance is ready is indeterminate
for i in {1..50};
do
    echo "Trying to connect to SQL Server so that setup.sql can be executed ..."
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i setup.sql
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterTables.sql
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterData.sql
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -v RS_DATABASE_NAME=$RS_DATABASE_NAME -i DBMasterStoredProcedure.sql
    if [ $? -eq 0 ]
    then
        echo "setup.sql completed"
        break
    else
        echo "SQL Server not ready yet..."
        sleep 1
    fi
done