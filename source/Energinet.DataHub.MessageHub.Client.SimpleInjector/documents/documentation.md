# Documentation

This package supports registration of MessageHub for IServiceCollection and SimpleInjector.

IServiceCollection example:

```csharp
        serviceCollection.AddMessageHub(
            messageHubSendConnectionString,
            new MessageHubConfig(dataAvailableQueue, domainReplyQueue),
            storageServiceConnectionString,
            new StorageConfig(azureBlobStorageContainerName));
```
