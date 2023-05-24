Feature: Throttling

Throttling feature:
Controlling the message processing rate

Scenario: Maximum consumption speed
	
	Given topic event-loop--throttling-topic--4-partitions
	Given consumer group event-loop--throttling-group--2-consumers--fixed-size-50--speed-50
	
	# with the speed of 50 messages per partition per second
	# it is expected to consume 200 messages per second from 4 partitions
	When partitions receive product orders in parallel:
	| Partition | Count |
	| 0         | 500   |
	| 1         | 500   |
	| 2         | 500   |
	| 3         | 500   |
	Then consumption happens at maximum rate 200 tps during the period of 10 seconds
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 500  |
	| 1         | 0    | 500  |
	| 2         | 0    | 500  |
	| 3         | 0    | 500  |
	* controller was invoked 40 times