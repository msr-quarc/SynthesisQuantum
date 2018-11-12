// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Quantum.Simulation.Core;
using Microsoft.Z3;
using System;
using System.Linq;

namespace Microsoft.Quantum.Samples.SATSolver
{
    public static class Translation
    {
        public static ReversibleComputation FromSATProblem(IOperationFactory sim, SATProblem problem)
        {
            /* initialize visited structure, by mapping each input to an index 0, 1, ... */
            var visited = Enumerable.Range(0, problem.Inputs.Length).ToDictionary(i => (Expr)problem.Inputs[i]);

            /* reversible computation object */
            var comp = sim.Get<ReversibleComputation, ReversibleComputation>();
            comp.CreateInputs(problem.Inputs.Length);

            /* local recursive function to visit the AST */
            int _AddExprRec(Expr e)
            {
                if (visited.ContainsKey(e))
                {
                    return visited[e];
                }

                if (e.IsFalse)
                {
                    return comp.ComputeFalse();
                }
                else if (e.IsTrue)
                {
                    return comp.ComputeTrue();
                }
                else if (e.IsNot)
                {
                    return comp.ComputeNot(_AddExprRec(e.Args[0]));
                }
                else if (e.IsAnd)
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
                else if (e.IsIff || e.IsEq)
                {
                    return comp.ComputeIff(_AddExprRec(e.Args[0]), _AddExprRec(e.Args[1]));
                }
                else if (e.IsImplies)
                {
                    return comp.ComputeImplies(_AddExprRec(e.Args[0]), _AddExprRec(e.Args[1]));
                }
                else
                {
                    throw new Exception($"unsupported expression {e}");
                }
            }

            _AddExprRec(problem.Formula);
            return comp;
        }
    }
}
