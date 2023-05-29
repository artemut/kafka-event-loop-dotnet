Feature: ErrorHandling

ErrorHandling feature:
Different ways to handle errors that occur during message processing

Scenario: Transient error
	
	Given topic event-loop--transient-error-topic--1-partition
	Given consumer group event-loop--transient-error-group--fixed-size-2

	When partition 0 receives 2 product orders
	# controller is configured to throw 2 times and then it's recovered (see TransientErrorController)
	* wait for 10 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 2    |
	| 0         | 0    | 2    |
	| 0         | 0    | 2    |
	* controller was invoked 3 times

	# make sure messages are now gone
	When wait for 10 second(s)
	Then no product order was consumed

Scenario: Critical error, stop consumer
	
	Given topic event-loop--critical-error-stop-topic--1-partition
	Given consumer group event-loop--critical-error-stop-group--fixed-size-2
	
	When partition 0 receives 2 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 2    |
	* controller was invoked 1 time

	# make sure new messages are no longer consumed
	When partition 0 receives 10 product orders
	* wait for 10 second(s)
	Then no product order was consumed
	* topic has no consumers

Scenario: Critical error, dead-lettering
	
	Given topic event-loop--critical-error-dead-lettering-topic--1-partition
	Given consumer group event-loop--critical-error-dead-lettering-group--fixed-size-2
	
	When partition 0 receives 2 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 2    |
	* controller was invoked 1 time

	# make sure consumer is able to receive new messages
	# controller is configured to not throw the second time (see CriticalErrorDeadLetteringController)
	When partition 0 receives 2 product orders
	* wait for 5 second(s)
	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 2    | 2    |
	* controller was invoked 1 time

	# make sure dead-letter messages are processed by a separate consumer group
	Given consumer group event-loop--dead-letters-group--fixed-size-2

	Then product orders were consumed:
	| Partition | Skip | Take |
	| 0         | 0    | 2    |
	* controller was invoked 1 time