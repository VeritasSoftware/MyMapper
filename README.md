# MyMapper
.NET object mapping framework. Fluent design. Dependency injectable.

Please read 00-SampleMapper.cs code file to learn how to create mappers using MyMapper framework.

Codefile 00-EntitiesSampleMapper.cs contains the source and destination entities used in the sample mapper.

Features:

1.	Fluent design.
2.	Dependency injectable.
3.  Ability to do auto mapping of source and destination properties (with the same name) using reflection 
	and add maps only for properties with different names. Reflective auto mapping can be turned off too.
4.	Ability to harness other maps. Other mappers can be chained as required.
5.	Conditional, Switch mapping support.
6.	Mappings happen dynamically on source instance.
7.	Mappings can be debugged using Debugger.Break().
