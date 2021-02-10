## Azure Container Registry events to Azure Storage queues 

The sample below uses event grid to bind ACR events and sends the events to storage queues. 
This enables persisting events and enabling a queue processing. 

> For custom events you can refer this sample  
>  - https://docs.microsoft.com/en-us/azure/event-grid/custom-event-to-queue-storage

For this sample we use an existing registry and a new storage accounts that will be used to receive queue events from the registry. Once the resource are created we finally bind the the ACR System Topic to the storage account queue. 

```bash
# Create Storage account and queue
export STORAGE_ACCOUNT_NAME=acropaeventgrid
export RESOURCE_GROUP=acropa-rg
export QUEUE_NAME=eventqueue

az group create --name $RESOURCE_GROUP --location westus2

az storage account create -n $STORAGE_ACCOUNT_NAME -g $RESOURCE_GROUP -l westus2 --sku Standard_LRS
az storage queue create --name $QUEUE_NAME --account-name $STORAGE_ACCOUNT_NAME

STORAGE_ID=$(az storage account show --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP --query id --output tsv)
QUEUE_ID="$STORAGE_ID/queueservices/default/queues/$QUEUE_NAME"

```

## Bind Event grid to Azure Storage queue

Send the ACR events to the event grid endpoint. Here the topic is a system topic which can directly publish to the storage queue endpoint.

```bash
export ACR_NAME={registryname}
ACR_REGISTRY_ID=$(az acr show --name $ACR_NAME --query id --output tsv)
az eventgrid event-subscription create \
    --name event-sub-acr \
    --source-resource-id $ACR_REGISTRY_ID \
    --endpoint-type storagequeue \
    --endpoint $QUEUE_ID
```

## Running application to subscribe to events

```bash
export CONNECTION_STRING=$(az storage account show-connection-string --name $STORAGE_ACCOUNT_NAME -g $RESOURCE_GROUP -o tsv --query connectionString)

dotnet run -- subscribe \
    --storage-connection-string   "$CONNECTION_STRING" \
    --queue-name $QUEUE_NAME
```

### Test message to handle processing

Post a test message to the Azure storage queue to test the receiver

```json
{
  "id": "0d7964cf-58f8-4f6d-b9b2-03ead7a9e223",
  "topic": "/subscriptions/xx/resourceGroups/xx/providers/Microsoft.ContainerRegistry/registries/myregistry",
  "subject": "empty-test",
  "eventType": "Microsoft.ContainerRegistry.ImageDeleted",
  "data": {
    "id": "0d7964cf-58f8-4f6d-b9b2-03ead7a9e223",
    "timestamp": "2021-02-09T22:46:40.9751806Z",
    "action": "delete",
    "target": {
      "mediaType": "application/vnd.oci.image.manifest.v1+json",
      "digest": "sha256:bc4b23b58840b73e24d7be3ff6baa477a830999023af4c75c83ec4db6c86fc9a",
      "repository": "empty-test"
    },
    "request": {
      "id": "7a1a6686-47e5-480d-bf8e-56c980563b29",
      "host": "myregistry.azurecr.io",
      "method": "DELETE",
      "useragent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36 Edg/88.0.705.63"
    }
  },
  "dataVersion": "1.0",
  "metadataVersion": "1",
  "eventTime": "2021-02-09T22:46:41.3678772Z"
}

```

