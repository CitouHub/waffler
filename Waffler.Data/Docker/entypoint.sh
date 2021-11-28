# Start script and SQL Server

# From https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-configure?view=sql-server-ver15&pivots=cs1-bash
# If you do create your own Dockerfile, be aware of the foreground process, 
# because this process controls the life of the container. If it exits, 
# the container will shut down. For example, if you want to run a script 
# and start SQL Server, make sure that the SQL Server process is the 
# right-most command. All other commands are run in the background. The 
# following command illustrates this inside a Dockerfile:

echo "Running entrypoint.sh"

./setup.sh & /opt/mssql/bin/sqlservr

echo "Ended entrypoint.sh"
