USG.HttpQueueing
================
Lets you create an HttpClient that defers requests by putting them on an Azure
Storage Queue, to be picked up by a queue triggered worker. The worker will
retry requests a few times.

Components:
 - **USG.HttpQueueuing**, the client library.
 - **USG.HttpQueueuing.Worker**, the worker, an Azure Function project.
 - **USG.HttpQueueuing.Aristocrat**, a console app to generate work for
   testing.

An Azure Storage account is required. With the default names, the layout is:
 - **requests-v1**, the request queue (URLs of blobs)
 - **requests-v1-poison**, repeatedly failed requests.
 - **payloads**, a blob container for the actual serialized requests since
   they may be too large to store directly in the queue.

Usage
-----
After adding the USG.HttpQueueing package, instantiate and use a queueing
HttpClient like so:

    using USG.HttpQueueing;

	// ...

	var storageAccount = CloudStorageAccount.Parse(...);
	var httpHandler = new QueueingHttpHandler(storageAccount);
	var httpClient = new HttpClient(httpHandler);
    
	await httpClient.PostAsync("https://example.com/foo");

A queueuing HTTP client will always return 202 Accepted. The worker will
discard the actual responses.

Local testing
-------------
Install and run Azure Storage Emulator, then run the solution. Both Worker
and Aristocrat are started. Press the return key in Aristocrat to generate
some work.

Shared instance
---------------
A *usghttpqueueing* resource group with storage account and worker has been
set up for general use. Use Azure Storage Explorer to get the connection
string.

Worker set up
-------------
If a separate instance is needed: create an Azure Function resource with its
own storage account and deploy USG.HttpQueueuing.Worker into it.

The queues and blob container are created automatically.

Releasing
---------
Update *CHANGELOG.md* and *Common.props* and push to master. The build
pipeline builds and uploads a new package and deploys the worker to the
shared instance.
