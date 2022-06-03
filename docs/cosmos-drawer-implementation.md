# MessageHub DataAvailable Repository

One of the main responsibilities of MessageHub is to manage DataAvailable events from other domains. Each event describes a piece of data from a domain: the size of the data, if it can be bundled and who the recipient of the data is. When an actor sends in a Peek request, these events are used to determine what data should be returned as the response.

In order to reach target performance numbers, MessageHub must efficiently manage these DataAvailable events when reading and writing in CosmosDB. The initial direct solution did not perform within the given time requirements, which led to this algorithm; a trade-off between code complexity and performance.

Because the complexity is purely persistence-related, efforts were made to keep as much of the code out of the domain model as possible. The algorithm is therefore mostly affecting the implementation of IDataAvailableRepository.

## Repository Requirements

### GetNextUnacknowledged(Recipient, Domains)

Given a recipient and a set of domains, find the next ordered sequence of DataAvailable events that have not been acknowledged.

### Acknowledge(Bundle)

Acknowledge all the events that are in the bundle. Once the events are acknowledged, they will no longer be returned from GetNextUnacknowledged.

### Save(DataAvailable Events)

Given an ordered list of received events, store the events so that they can be efficiently returned from GetNextUnacknowledged.

### Other

- The events must be processed as a FIFO queue, per recipient. Events across recipients can be unordered.
- Peek must take at most 30 seconds, including time it takes for domains to return the data.
- Dequeue must take at most 0.5 seconds.
- The file returned from Peek must be no larger than 50 MB. The smallest weight of the data returned from a domain is 1 KB. In the worst case, 51.200 individual events can be bundled into a single file.
- A single CosmosDB partition has a technical limitation of about 10.000 RU/s. Empirical testing showed that it is not possible to place all 51.200 events into a single partition and read them back within our performance targets.

## Drawer Solution

The main idea is to spread the received events over random partitions in CosmosDB. This will allow for parallel reading from several partitions. These partitions are tracked by data structures which allow for fast lookup and update.

## Components

### DataAvailable

Represents a single event. Contains the id of the event, the recipient of the data, the weight of the data and whether the data can be bundled. The partition key of an event points to the id of the drawer that contains it.

### Drawer

Represents an ordered container for DataAvailable events. A drawer contains at least 1 event and at most 10.000 events. The drawer also tracks how many of the events have been acknowledged - these acknowledged events can be skipped. The id of the drawer is a GUID, so the resulting partitions will end up with a random distribution. The drawer's partition key point to the Cabinet (key) that owns it.

### Cabinet (Key)

Represents an ordered set of Drawer containers. There is no limit on how many drawers a cabinet can contain. The key of the cabinet describes the kind of events stored inside the drawers of the cabinet. The key is composite and consists of the recipient, the domain that generated the events and the type of data inside the events.

Note: a Cabinet is not represented by an actual document in CosmosDB, but is a construct representing the key and the drawers. The algorithm does not need an actual cabinet to function, but the construct helps explain the algorithm.

### Catalog

A list of all the Cabinet keys that point to drawers with unacknowledged events. The partition key of each catalog entry is a composite key of recipient and domain. Each catalog entry also specifies the sequence number of the first unacknowledged event in the referenced drawer - this is used to quickly determine which drawer contains the next event.

### Bundle

Represents a bundled set of DataAvailable events.

### Sequence Number

A unique and strictly ascending number assigned to each event. The drawers and the catalog also use this number to determine which entry to pick next.

## Implementation

### GetNextUnacknowledged

A Peek request is made by an actor, asking for data from a specific domain. The first step is to look in the catalog for all entries for this recipient and domain. This is done by constructing the composite partition key Actor-Domain and doing a lookup using the key. If no catalog entry is found, then no new unacknowledged data is available. Otherwise, of all the found catalog entries, the one with the smallest sequence number points to the drawer with the next event. The cabinet key from this catalog entry is used to find the cabinet and all its drawers.

Once the drawers have been found, they are ordered by sequence number, skipping drawers without unacknowledged events. The events for up to 6 next drawers - the maximum number of events bundleable - are then fetched in parallel. A cabinet reader is created from these fetched drawers and their events. The reader facilitates iteration through these events, while simultaneously tracking the position in each drawer.

This reader is then returned to the domain layer, which will iterate through the events based on domain-specific logic. The events chosen in the domain are grouped together in a bundle. The bundle, as well as the changed position inside the drawer, is then persisted to CosmosDB.

Using this approach, it is possible to quickly find which random partition (drawer) contains the next unacknowledged events and allows for parallel reads. The worst running time is linear O(c + d + n) where c is the number of catalog entries affected and d is the number of drawers; n is the number of events retrieved.

### Acknowledge

When acknowledging events, the actual events are not modified. Instead, the bundle contains the list of event ids that were acknowledged. In order to skip acknowledged events on next GetNextUnacknowledged call, the affected drawers are updated with the position of the next event. Etags are used to ensure that concurrent dequeues do not overwrite each others drawer updates.

The catalog entries that were used to find the drawers are deleted. New entries maybe also be added, but the logic differs based on the state of each drawer.

| Empty after Peek | Empty at Dequeue | Catalog Added | Notes                                                                                                                                                                                                                                                            |
|------------------|------------------|---------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| false            | false            | No            | If the drawer has no additional unacknowledged events - all events were returned during previous peek and no new events were added since - then a catalog entry is not created. The catalog entry pointing to the drawer will instead be added during next Save. |
| false            | true             | No            | Impossible scenario.                                                                                                                                                                                                                                             |
| true             | false            | Yes           | If the drawer had no additional unacknowledged events after GetNextUnacknowledged, but new events have been added since, then a new entry is created pointing to the new events.                                                                                 |
| true             | true             | Yes           | If the drawer had additional unacknowledged events left, a new updated catalog entry is added, pointing to the next item in the drawer.                                                                                                                          |

Acknowledging using this approach takes a constant running time O(1).

### Save

Saving an event requires finding the right cabinet and the next drawer. Finding the right cabinet is straightforward, as the cabinet key can be constructed directly from properties of the event: the recipient, the domain and the type of data.

Once the cabinet key is known, it is necessary to find the right drawer. This is done by finding the last drawer in the cabinet, as it is the only drawer that can be partially filled. If the last drawer is completely filled, a new drawer is created instead. Once the drawer is known, the event is saved into it. Furthermore, if the drawer did not already have any unacknowledged events, a new catalog entry is created that points to the newly saved event.

When saving several events at once, the process stays mostly the same. However, several events are bulk-inserted into the drawer, up to the maximum amount of 10.000. Because events must be saved in order, it is not possible to save across drawers in parallel. Saving events with different Cabinet keys can be done in parallel, though.

### Parallel Execution

The data structures used in this algorithm allow for parallel execution when getting and acknowledging the events. Each peek is synchronized using a trigger when committing the bundle, while dequeues are synchronized using etags. Saving the events, however, must not run in parallel, as it is necessary to ensure that the ordering of the events is preserved and sequence numbers are correctly updated.

Empirical testing showed that saving events one-by-one is too slow. It is therefore necessary to bulk insert events. Since the order of events persisted during bulk insert is unknown, there must be a mechanism that ensures that the events are unavailable for peeking until the insertions complete.

The solution is to use sequence numbers to skip events that are not yet ready. When saving events, the maximum usable sequence number is recorded. Calls to GetNextUnacknowledged ignore all data structures (catalog entries, drawers, events) with a sequence number higher than the currently recorded maximum sequence number. Once the events have been bulk-inserted, the maximum sequence number is updated, making the new events available for GetNextUnacknowledged.
