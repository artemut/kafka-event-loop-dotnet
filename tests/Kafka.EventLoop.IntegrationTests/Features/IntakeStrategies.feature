Feature: IntakeStrategies

Intake strategies feature:
Different ways to decide when it is time to start message processing

Scenario: Fixed size
	
	Given topic event-loop--fixed-size-topic--2-partitions
	Given consumer group event-loop--fixed-size-group--2-consumers--fixed-size-6

	When partition 0 receives 5 product orders
	When partition 1 receives 1 product orders
	* wait for 5 second(s)
	Then no product order was consumed

	When partition 0 receives 1 product order
	When partition 1 receives 5 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 6    |
	| 1         | 0    | 6    |
	* controller was invoked 2 times

	When partition 0 receives 36 product order
	When partition 1 receives 36 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 6    | 36   |
	| 1         | 6    | 36   |
	* controller was invoked 12 times

Scenario: Fixed interval
	
	Given topic event-loop--fixed-interval-topic--2-partitions
	Given consumer group event-loop--fixed-interval-group--2-consumers--fixed-interval-5s

	When partitions gradually receive product orders for the duration of 20 seconds:
	| Partition | Messages per second | Expected total |
	| 0         | 2                   | 40 (4 intakes) |
	| 1         | 3                   | 60 (4 intakes) |
	* wait for 10 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 40   |
	| 1         | 0    | 60   |
	* controller was invoked at least 8 times, at most 10 times

Scenario: Max size with timeout
	
	Given topic event-loop--max-size-timeout-topic--3-partitions
	Given consumer group event-loop--max-size-timeout-group--3-consumers--size-10-5s

	When partitions gradually receive product orders for the duration of 20 seconds:
	| Partition | Messages per second | Comment                               | Expected total |
	| 0         | 1                   | 5 per 5s -> timeout happens first     | 20 (4 intakes) |
	| 1         | 2                   | 10 per 5s -> both conditions apply    | 40 (4 intakes) |
	| 2         | 3                   | 15 per 5s -> size limit happens first | 60 (6 intakes) |
	* wait for 10 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 20   |
	| 1         | 0    | 40   |
	| 2         | 0    | 60   |
	* controller was invoked at least 14 times, at most 17 times

Scenario: Custom strategy
	
	Given topic event-loop--custom-strategy-topic--2-partitions
	Given consumer group event-loop--custom-strategy-group--2-consumers

	When partitions receive product orders with given ids:
	| Partition | Id   |
	| 0         | 1    |
	| 0         | 2    |
	| 1         | 1003 |
	| 0         | 6    |
	| 1         | 1009 |
	* wait for 5 second(s)
	Then no product order was consumed

	When partitions receive product orders with given ids:
	| Partition | Id | Comment        |
	| 0         | 5  |                |
	| 0         | 7  | divisible by 7 |
	* wait for 5 second(s)
	Then product orders with given ids were consumed:
	| Partition | Ids       |
	| 0         | 1,2,6,5,7 |
	* controller was invoked 1 time

	When partitions receive product orders with given ids:
	| Partition | Id   | Comment        |
	| 0         | 9    |                |
	| 1         | 1008 | divisible by 7 |
	| 0         | 21   | divisible by 7 |
	* wait for 5 second(s)
	Then product orders with given ids were consumed:
	| Partition | Ids            |
	| 0         | 9,21           |
	| 1         | 1003,1009,1008 |
	* controller was invoked 2 times