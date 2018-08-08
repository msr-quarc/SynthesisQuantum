# SAT Solver Sample #

This sample contains C# and Q# code to implement a SAT solver.  Input is a SAT problem
specified using the API for Boolean logic from the Z3 SMT solver.  The Q# SAT solver
is implemented using a Grover search.  A hierarchical reversible logic synthesis algorithm
is used to translate the SAT problem into a quantum oracle.

The SAT problem can have an arbitrary number of input variables.  However, the iteration
count in the Grover search is currently hard-coded and effectively only supports 2-input
formulas with a single solution or 3-input formulas with two solutions.  The supported set
of Boolean operators is TRUE, FALSE, NOT, AND, OR, XOR, IFF, and IMPLIES.

## Manifest ##

- [SATSolver.qs](./SATSolver.qs): Q# implementation of a SAT solver.
- [Program.cs](./Program.cs): C# console application to translate the SAT problem into a reversible computation
- [ReversibleComputation.cs](./ReversibleComputation.cs): C# class that for reversible computations.  The class
  implements a Q# operation and can be passed to a Q# program.
- [SATProblem.cs](./SATproblem.cs) Abstraction for a SAT problem using Z3 API and sample problem.
- [Translation.cs](./Translation.cs) Routine to translate a SAT problem into a reversible computation.
- [SATSolver.csproj](./SATSolver.csproj): Main C# project for the sample.
