Feature: Streaming

Streaming feature:
Sending messages to another topic

Scenario: One-to-one streaming
	
	Given topic event-loop--streamer-topic--2-partitions
	Given consumer group event-loop--streamer-group--2-consumers--fixed-size-10

	When partition 0 receives 10 product orders
	When partition 1 receives 10 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 10   |
	| 1         | 0    | 10   |
	* extended product orders were streamed:
	| Partition | Skip | Take |
	| 0         | 0    | 10   |
	| 1         | 0    | 10   |
	* controller was invoked 2 times

	# make sure consumer is able to receive new messages
	When partition 0 receives 10 product orders
	When partition 1 receives 10 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 10   | 10   |
	| 1         | 10   | 10   |
	* extended product orders were streamed:
	| Partition | Skip | Take |
	| 0         | 10   | 10   |
	| 1         | 10   | 10   |
	* controller was invoked 2 times

	# make sure streamed messages are processed by a separate consumer group
	Given consumer group event-loop--stream-consumer-group--2-consumers--fixed-size-5
	
	When wait for 10 second(s)

	Then extended product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 20   |
	| 1         | 0    | 20   |
	* controller was invoked 8 times
