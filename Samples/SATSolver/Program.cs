// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Quantum.Primitive;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Quantum.Samples.SATSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new Context();

            /* build SAT problem */
            var xs = Enumerable.Range(1, 3).Select(i => (BoolExpr)ctx.MkConst($"x{i}", ctx.MkBoolSort())).ToArray();
            var maj3 = ctx.MkOr(ctx.MkAnd(xs[0], xs[1]), ctx.MkAnd(xs[0], xs[2]), ctx.MkAnd(xs[1], xs[2]));
            var xor3 = ctx.MkXor(xs[0], ctx.MkXor(xs[1], xs[2]));
            var problem = ctx.MkIff(maj3, xor3);

            using (var sim = new QuantumSimulator()) {
                /* translate SAT problem to into a reversible computation */
                var comp = Translate(sim, xs, problem);

                /* solve SAT problem with Q# and print solution */
                var res = SATSolver.Run(sim, comp, xs.Length).Result;
                Console.WriteLine(res);

                /* verify solution with classical SAT solver */
                var assumptions = xs.Zip(res, (x, v) => v ? x : ctx.MkNot(x)).ToArray();
                var solver = ctx.MkSolver();
                Console.WriteLine(solver.Check(assumptions));

                /* report */
                Console.WriteLine($"SAT solving finished. Used {comp.RequiredAncillae} ancillae qubits.");
            }
        }

        static ReversibleComputation Translate(IOperationFactory sim, BoolExpr[] inputs, BoolExpr problem)
        {
            /* initialize visited structure */
            var visited = new Dictionary<Expr, int>();
            var count = 0;
            foreach ( var inp in inputs )
            {
                visited[inp] = count++;
            }

            /* reversible computation object */
            var comp = sim.Get<ReversibleComputation, ReversibleComputation>();

            int _AddExprRec(Expr e)
            {
                if (visited.ContainsKey(e))
                {
                    return visited[e];
                }

                if (e.IsAnd)
                {
                    var indexes = e.Args.Select(x => _AddExprRec(x)).ToList();
                    return comp.ComputeAnd(indexes);
                }
                else if (e.IsOr)
                {
                    var indexes = e.Args.Select(x => _AddExprRec(x)).ToList();
                    return comp.ComputeOr(indexes);
                }
                else if (e.IsXor)
                {
                    var indexes = e.Args.Select(x => _AddExprRec(x)).ToList();
                    return comp.ComputeXor(indexes);
                }
                else if (e.IsIff)
                {
                    return comp.ComputeIff(_AddExprRec(e.Args[0]), _AddExprRec(e.Args[1]));
                }
                else
                {
                    throw new Exception($"unsupported expression {e}");
                }
            }

            _AddExprRec(problem);
            return comp;
        }
    }
}
