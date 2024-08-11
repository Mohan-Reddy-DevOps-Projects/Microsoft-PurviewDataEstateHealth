#!/bin/bash
set -e

if [ -z ${SUBSCRIPTION_ID+x} ]; then
    echo "SUBSCRIPTION_ID is unset, unable to continue"
    exit 1;
fi

if [ -z ${POWER_BI_ARCHIVE_SAS+x} ]; then
    echo "POWER_BI_ARCHIVE_SAS is unset, unable to continue"
    exit 1;
fi

if [ -z ${DOMAINMODEL_ARCHIVE_SAS+x} ]; then
    echo "DOMAINMODEL_ARCHIVE_SAS is unset, unable to continue"
    exit 1;
fi

if [ -z ${DIMENSIONALMODEL_ARCHIVE_SAS+x} ]; then
    echo "DIMENSIONALMODEL_ARCHIVE_SAS is unset, unable to continue"
    exit 1;
fi

if [ -z ${DEHFABRICSYNC_ARCHIVE_SAS+x} ]; then
    echo "DEHFABRICSYNC_ARCHIVE_SAS is unset, unable to continue"
    exit 1;
fi

if [ -z ${STORAGE_ACCOUNT_NAME+x} ]; then
    echo "STORAGE_ACCOUNT_NAME is unset, unable to continue"
    exit 1;
fi

if [ -z ${DESTINATION_CONTAINER_NAME+x} ]; then
    echo "DESTINATION_CONTAINER_NAME is unset, unable to continue"
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

echo "Downloading powerbi.tar file from build output"
mkdir domain

echo "wget domainmodel.tar to directory domain"
wget $DOMAINMODEL_ARCHIVE_SAS -O domainmodel.tar
wget $DEHFABRICSYNC_ARCHIVE_SAS -O dehfabricsync.tar
wget $DIMENSIONALMODEL_ARCHIVE_SAS -O dimensionalmodel.tar

echo "Untar domainmodel.tar to directory domain"
tar -C domain -xvf domainmodel.tar
tar -C domain -xvf dehfabricsync.tar
tar -C domain -xvf dimensionalmodel.tar

cd ..

mkdir domainfiles

echo "list all files and folders - ls -R"
ls -R

cp ./unarchive/domain/dataestatehealthanalytics-domainmodel-azure-purview-1.1-jar.jar ./domainfiles/
cp ./unarchive/domain/dataestatehealthanalytics-dehfabricsync-azure-purview-1.0-jar.jar ./domainfiles/
cp ./unarchive/domain/dataestatehealthanalytics-dimensionalmodel-azure-purview-1.1-jar.jar ./domainfiles/


echo "Creating container"
echo az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME
az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME

echo "Uploading Domain Model files"
echo az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source domainfiles --overwrite
az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source domainfiles --overwrite

echo "Domain Model files uploaded to storage container successfully"


