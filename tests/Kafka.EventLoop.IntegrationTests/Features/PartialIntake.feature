Feature: PartialIntake

Partial intake feature:
A special case when we have to consume more messages than we actually need
because we have to wait until each partition meets a certain condition
even when some partitions have already met this condition

Scenario: Custom intake strategy with partial intake
	
	Given topic event-loop--partial-intake-strategy-topic--3-partitions
	Given consumer group event-loop--partial-intake-strategy-group--1-consumer

	When partitions receive product orders with given ids:
	| Partition | Id   |
	| 0         | 1    |
	| 1         | 100  |
	| 2         | 1000 |
	* wait for 5 second(s)
	Then no product order was consumed

	When partitions receive product orders with given ids:
	| Partition | Id  | Comment            |
	| 0         | 7   | divisible by 7     |
	| 0         | 8   | should be excluded |
	| 1         | 101 |                    |
	* wait for 5 second(s)
	Then no product order was consumed

	When partitions receive product orders with given ids:
	| Partition | Id  | Comment                              |
	| 1         | 102 |                                      |
	| 1         | 105 | divisible by 7                       |
	| 0         | 14  | divisible by 7 but should be ignored |
	* wait for 5 second(s)
	Then no product order was consumed

	When partitions receive product orders with given ids:
	| Partition | Id   | Comment                              |
	| 1         | 112  | divisible by 7 but should be ignored |
	| 2         | 1001 | divisible by 7                       |
	| 2         | 1002 | should be ignored                    |
	| 0         | 15   | should be ignored                    |
	* wait for 5 second(s)
	Then product orders with given ids were consumed:
	| Partition | Ids             |
	| 0         | 1,7             |
	| 1         | 100,101,102,105 |
	| 2         | 1000,1001       |
	* controller was invoked 1 time

	# verify that ignored messages can be consumed again
	When partitions receive product orders with given ids:
	| Partition | Id   | Comment                                            |
	| 0         | 19   | should be ignored as we have 2nd ID divisible by 7 |
	| 1         | 119  | should be ignored as we have 2nd ID divisible by 7 |
	| 2         | 1008 | divisible by 7                                     |
	* wait for 5 second(s)
	Then product orders with given ids were consumed:
	| Partition | Ids       |
	| 0         | 8,14      |
	| 1         | 112       |
	| 2         | 1002,1008 |
	* controller was invoked 1 time

	# verify that ignored messages can be consumed again
	When partitions receive product orders with given ids:
	| Partition | Id   | Comment        |
	| 0         | 21   | divisible by 7 |
	| 2         | 1009 |                |
	| 2         | 1015 | divisible by 7 |
	* wait for 5 second(s)
	Then product orders with given ids were consumed:
	| Partition | Ids       |
	| 0         | 15,19,21  |
	| 1         | 119       |
	| 2         | 1009,1015 |
	* controller was invoked 1 time