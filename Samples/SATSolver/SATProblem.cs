// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Z3;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.Quantum.Samples.SATSolver
{
    public struct SATProblem
    {
        public SATProblem(BoolExpr[] inputs, BoolExpr formula, Context context)
        {
            Inputs = inputs;
            Formula = formula;
            Context = context;
        }
 
        public BoolExpr[] Inputs { get; }
        public BoolExpr Formula { get; }
        public Context Context { get; }

        public bool Verify(bool[] assignments)
        {
            Contract.Requires(assignments.Length == Inputs.Length);

            var solver = Context.MkSolver();
            solver.Add(Formula);
            var ctx = Context;
            var assumptions = Inputs.Zip(assignments, (x, v) => v ? x : ctx.MkNot(x)).ToArray();
            return solver.Check(assumptions) == Status.SATISFIABLE;
        }

        public override string ToString()
        {
            return Formula.ToString();
        }
    }

    public static class Formulas
    {
        public static SATProblem MAJeqIFF()
        {
            var ctx = new Context();

            /* create three Boolean variables xs[0], xs[1], xs[2] */
            var xs = Enumerable.Range(1, 3).Select(i => (BoolExpr)ctx.MkConst($"x{i}", ctx.MkBoolSort())).ToArray();

            /* create MAJ(xs[0], xs[1], xs[2]) */
            var maj3 = ctx.MkOr(ctx.MkAnd(xs[0], xs[1]), ctx.MkAnd(xs[0], xs[2]), ctx.MkAnd(xs[1], xs[2]));

            /* create XOR(xs[0], xs[1], xs[2]) */
            var xor3 = ctx.MkXor(xs[0], ctx.MkXor(xs[1], xs[2]));

            /* compare MAJ with XOR */
            var problem = ctx.MkIff(maj3, xor3);

            return new SATProblem(xs, problem, ctx);
        }
    }
}
