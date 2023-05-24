Feature: Parallelism

Parallelism feature:
Multiple consumers, multiple partitions

Scenario: Topic with 2 partitions and 1 consumer that consumes every 2 messages

	Given topic event-loop--parallelism-topic--2-partitions
	Given consumer group event-loop--parallelism-group--1-consumer--fixed-size-2
	
	When wait for 5 second(s)
	Then no product order was consumed

	When partition 0 receives 1 product order
	* wait for 5 second(s)
	Then no product order was consumed

	When partition 1 receives 1 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 1    |
	| 1         | 0    | 1    |
	* controller was invoked 1 time

Scenario: Topic with 3 partitions and 2 consumers that consume every 2 messages

	Given topic event-loop--parallelism-topic--3-partitions
	Given consumer group event-loop--parallelism-group--2-consumers--fixed-size-2
	
	When wait for 5 second(s)
	Then no product order was consumed

	When partition 0 receives 1 product order
	* wait for 5 second(s)
	Then no product order was consumed

	When partition 1 receives 1 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 1    |
	| 1         | 0    | 1    |
	* controller was invoked 1 time

	When partition 2 receives 1 product order
	* wait for 5 second(s)
	Then no product order was consumed

	When partition 2 receives 1 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 2         | 0    | 2    |
	* controller was invoked 1 time

Scenario: Topic with 4 partitions and 4 consumers that consume every 2 messages

	Given topic event-loop--parallelism-topic--4-partitions
	Given consumer group event-loop--parallelism-group--4-consumers--fixed-size-2
	
	When wait for 5 second(s)
	Then no product order was consumed

	When partition 0 receives 2 product orders
	When partition 1 receives 4 product orders
	When partition 2 receives 6 product orders
	When partition 3 receives 8 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 2    |
	| 1         | 0    | 4    |
	| 2         | 0    | 6    |
	| 3         | 0    | 8    |
	* controller was invoked 10 times