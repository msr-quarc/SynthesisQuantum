// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


namespace Microsoft.Quantum.Samples.SATSolver {
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

    operation MultipleControlledZ(qubits : Qubit[]) : () {
        body {
            (Controlled Z)(qubits[1..(Length(qubits) - 1)], qubits[0]);
        }
    }

    operation SATSolver(oracle : ((Qubit[], Qubit) => ()), numInputs : Int) : Bool[] {
        body {
            mutable result = 0;

            using (qubits = Qubit[numInputs + 1]) {
                // function is applied to first 3 qubits
                let inputs = qubits[0..(numInputs - 1)];

                // ancillae is initialized to 1
                X(qubits[numInputs]);

                // superposition
                (ApplyToEach(H, _))(qubits);

                // apply oracle
                oracle(inputs, qubits[numInputs]);

                // diffusion operator
                With(ApplyToEachA(BindA([H; X]), _), MultipleControlledZ, inputs);

                // uncompute ancillae
                H(qubits[numInputs]);
                X(qubits[numInputs]);

                set result = MeasureInteger(LittleEndian(inputs));
            }

            return BoolArrFromPositiveInt(result, numInputs);
        }
    }
}
