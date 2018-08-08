// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Linq;

namespace Microsoft.Quantum.Samples.SATSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var problem = Formulas.MAJeqIFF();

            Console.WriteLine($"Solving SAT formula: {problem}");

            using (var sim = new QuantumSimulator()) {
                /* translate SAT problem to into a reversible computation */
                var comp = Translation.FromSATProblem(sim, problem);

                /* solve SAT problem with Q# and print solution */
                var res = SATSolver.Run(sim, comp, problem.Inputs.Length).Result;
                Console.WriteLine(String.Join(" ", problem.Inputs.Zip(res, (x, v) => $"{x.ToString()} = {v}")));

                /* verify the solution using a classical SAT solver */
                Console.WriteLine($"Verify result with SAT solver: {problem.Verify(res.ToArray())}");

                /* report */
                Console.WriteLine($"SAT solving finished. Used {comp.RequiredAncillae} ancillae qubits.");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
