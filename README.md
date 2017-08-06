# MyMapper
.NET light-weight, powerful object mapping framework. Fluent design. Dependency injectable.

Please read 00-SampleMapper.cs code file to learn how to create mappers using MyMapper framework.

Please read 00-SampleMapper-WithExtensions.cs code file to learn how to create mappers using MyMapper framework extensions.

Codefile 00-EntitiesSampleMapper.cs contains the source and destination entities used in the sample mapper.

Features:

1.	Fluent design.
2.	Dependency injectable.
3.  Ability to do auto mapping of source and destination properties (with the same name) using reflection 
	and add maps only for properties with different names. Reflective auto mapping can be turned off too.
4.	Ability to create multiple maps between the same source and destination types.
5.	Ability to harness other maps. Other mappers can be chained as required.
6.	Conditional, Switch mapping support.
7.	Mappings happen dynamically on source instance.
8.	Mappings can be debugged by either setting a breakpoint or using Debugger.Break().
9.	Parallel mapping support.
10.	.NET Object and IEnumerable<T> integration extensions. Async support.
