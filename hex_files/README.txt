How to use:-
------------

Protocol: @R/WXXYYY;
XX  Address register in decimal
YYY Data to be written in decimal

NOTE: For read use @RXX000;

For ATmega16:
REG   |Address
------|--------
PORTA | 59
DDRA  | 58
PINA  | 57
------|----
PORTB | 56
DDRB  | 55
PINB  | 54
------|----
PORTC | 53
DDRC  | 52
PINC  | 51
------|----

For ATmega328p:
REG   |Address
------|--------
PORTB | 37
DDRB  | 36
PINB  | 35
------|----
PORTC | 40
DDRC  | 39
PINC  | 38
------|----