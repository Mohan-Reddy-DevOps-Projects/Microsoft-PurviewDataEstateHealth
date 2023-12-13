#!/bin/bash
set -e

if [ -z ${SUBSCRIPTION_ID+x} ]; then
    echo "SUBSCRIPTION_ID is unset, unable to continue"
    exit 1;
fi

if [ -z ${PYTHON_CATALOG_SPARK_JOB_MAIN+x} ]; then
    echo "PYTHON_CATALOG_SPARK_JOB_MAIN is unset, unable to continue"
    exit 1;
fi

if [ -z ${PYTHON_DATAQUALITY_SPARK_JOB_MAIN+x} ]; then
    echo "PYTHON_DATAQUALITY_SPARK_JOB_MAIN is unset, unable to continue"
    exit 1;
fi

if [ -z ${PYTHON_DATA_ESTATE_HEALTH_LIBRARY+x} ]; then
    echo "PYTHON_DATA_ESTATE_HEALTH_LIBRARY is unset, unable to continue"
    exit 1;
fi

if [ -z ${DESTINATION_CONTAINER_NAME+x} ]; then
    echo "DESTINATION_CONTAINER_NAME is unset, unable to continue"
    exit 1;
fi

if [ -z ${STORAGE_ACCOUNT_NAME+x} ]; then
    echo "STORAGE_ACCOUNT_NAME is unset, unable to continue"
    exit 1;
fi

# Exit when commands complete with non-0 exit codes and display the failing command and its code
trap 'last_command=$current_command; current_command=$BASH_COMMAND' DEBUG
trap 'echo "\"${last_command}\" command completed with exit code $?."' EXIT

# echo "Switch cloud"
# az cloud set -n $Cloud

echo "Login to Azure with the user-assigned MSI"
echo az login --identity
az login --identity

echo "Set current subscription"
echo az account set --subscription $SUBSCRIPTION_ID
az account set --subscription $SUBSCRIPTION_ID

echo "Install extension storage-preview"
echo az extension add --name storage-preview
az extension add --name storage-preview

mkdir pythonfiles

echo "Downloading Catalog.tar file from build output"
echo "wget $PYTHON_CATALOG_SPARK_JOB_MAIN -O Catalog.tar"
wget $PYTHON_CATALOG_SPARK_JOB_MAIN -O Catalog.tar
echo "Untar Catalog.tar to directory pythonfiles"
tar -C pythonfiles -xvf Catalog.tar

echo "Downloading DataQuality.tar file from build output"
echo "wget $PYTHON_DATAQUALITY_SPARK_JOB_MAIN -O DataQuality.tar"
wget $PYTHON_DATAQUALITY_SPARK_JOB_MAIN -O DataQuality.tar
echo "Untar DataQuality.tar to directory pythonfiles"
tar -C pythonfiles -xvf DataQuality.tar

cd ..

mkdir finalpythonfiles

echo "list all files and folders - ls -R"
ls -R

cp ./unarchive/pythonfiles/catalog_spark_job_main.py ./finalpythonfiles/
cp ./unarchive/pythonfiles/dataquality_spark_job_main.py ./finalpythonfiles/

echo "list all files and folders - ls -R"
ls -R

mkdir DataEstateLibraryFolder
cd ./DataEstateLibraryFolder
echo "Downloading DataEstateHealthLibrary.zip file from build output"
echo "wget $STORAGE_ACCOUNT_NAME -O DataEstateHealthLibrary.zip"
wget $PYTHON_DATA_ESTATE_HEALTH_LIBRARY -O DataEstateHealthLibrary.zip
cd ..

echo "Creating container"
echo az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME
az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME

echo "Uploading DataEstateHealthLibrary.zip"
echo az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source DataEstateLibraryFolder --overwrite
az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source DataEstateLibraryFolder --overwrite

echo "Uploading dataquality_spark_job_main.py and catalog_spark_job_main.py"
echo az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source finalpythonfiles --overwrite
az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source finalpythonfiles --overwrite

echo "Python files uploaded to storage container successfully"
