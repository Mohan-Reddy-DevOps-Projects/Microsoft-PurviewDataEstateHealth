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

if [ -z ${DATADOMAINJARS_ARCHIVE_SAS+x} ]; then
    echo "DATADOMAINJARS_ARCHIVE_SAS is unset, unable to continue"
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

echo "wget datadomainjars.tar to directory domain"
wget $DATADOMAINJARS_ARCHIVE_SAS -O datadomainjars.tar

echo "Untar datadomainjars.tar to directory domain"
tar -C domain -xvf datadomainjars.tar

cd ..

mkdir domainfiles

echo "list all files and folders - ls -R"
ls -R

cp ./unarchive/domain/target/*.jar ./domainfiles/

echo "Creating container"
echo az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME
az storage container create --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --name $DESTINATION_CONTAINER_NAME

echo "Uploading Domain Model files"
echo az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source domainfiles --overwrite
az storage blob upload-batch --auth-mode login --account-name $STORAGE_ACCOUNT_NAME --destination $DESTINATION_CONTAINER_NAME --source domainfiles --overwrite

echo "Domain Model files uploaded to storage container successfully"


