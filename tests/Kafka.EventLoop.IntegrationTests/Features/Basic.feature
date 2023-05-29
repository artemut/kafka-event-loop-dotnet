Feature: Basic

Basic feature:
1 consumer
Consume, Process, Commit

Scenario: Single consumer and topic with single partition

	Given topic event-loop--basic-topic--1-partition
	Given consumer group event-loop--basic-group
	
	When wait for 5 second(s)
	Then no product order was consumed
	
	When partition 0 receives 1 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 1    |
	* controller was invoked 1 time
	
	When partition 0 receives 1 product order
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 1    | 1    |
	* controller was invoked 1 time