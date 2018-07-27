// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Quantum.Canon;
using Microsoft.Quantum.Primitive;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;

namespace Microsoft.Quantum.Samples.SATSolver
{
    /// <summary>
    /// This is a class to perform a reversible computation.
    /// </summary>
    public class ReversibleComputation : Operation<(QArray<Qubit>, Qubit), QVoid>
    {
        public ReversibleComputation(IOperationFactory m) : base(m)
        {
        }

        public override void Init()
        {
            _simbase = Factory as SimulatorBase;
            if (_simbase == null)
            {
                throw new ExecutionFailException("simulator for reversible computation does not allow to allocate qubits");
            }
        }

        public int ComputeAnd(List<int> qubits)
        {
            _steps.Add((OpCode.AND, qubits));
            return -_steps.Count;
        }

        public int ComputeOr(List<int> qubits)
        {
            _steps.Add((OpCode.OR, qubits));
            return -_steps.Count;
        }

        public int ComputeXor(List<int> qubits)
        {
            _steps.Add((OpCode.XOR, qubits));
            return -_steps.Count;
        }

        public int ComputeIff(int qubit1, int qubit2)
        {
            _steps.Add((OpCode.IFF, new List<int>{qubit1, qubit2}));
            return -_steps.Count;
        }

        public override Func<(QArray<Qubit>, Qubit), QVoid> Body
        {
            get
            {
                return delegate ((QArray<Qubit>, Qubit) args)
                {
                    /* save qubits for internal use */
                    (_inputs, _output) = args;

                    /* allocate possible ancillae qubits */
                    AllocateQubits();

                    /* compute intermediate steps on ancillae + final step on output */
                    for (int i = 0; i < _steps.Count; ++i)
                    {
                        ApplyStep(i);
                    }

                    /* uncompute intermediate steps */
                    for (int i = _steps.Count - 2; i >= 0; --i)
                    {
                        ApplyStep(i);
                    }

                    /* release ancillae qubits */
                    ReleaseQubits();

                    return QVoid.Instance;
                };
            }
        }

        private void AllocateQubits()
        {
            if (RequiredAncillae > 0)
            {
                _ancillae = _simbase.QubitManager.Allocate(RequiredAncillae);
            }
        }

        private void ReleaseQubits()
        {
            if (_ancillae != null)
            {
                _simbase.QubitManager.Release(_ancillae);
            }
        }

        private (QArray<Qubit>, Qubit) GetQubits(int stepIndex)
        {
            var children = new QArray<Qubit>();
            foreach (var child in _steps[stepIndex].Item2)
            {
                if (child < 0)
                {
                    children.Add(_ancillae[-child - 1]);
                }
                else
                {
                    if (child >= _inputs.Count)
                    {
                        throw new ExecutionFailException($"input index out of range in step {stepIndex}");
                    }
                    children.Add(_inputs[child]);
                }
            }
            var target = (stepIndex == _steps.Count - 1) ? _output : _ancillae[stepIndex];
            return (children, target);
        }

        private void ApplyStep(int stepIndex)
        {
            var (controls, target) = GetQubits(stepIndex);

            switch (_steps[stepIndex].Item1)
            {
                /* Multiple-controlled Toffoli gate */
                case OpCode.AND:
                {
                    var cx = new ControlledOperation<Qubit, QVoid>(Factory.Get<X, X>());
                    cx.Body.Invoke((controls, target));
                } break;

                /* Multiple-controlled Toffoli gate with negated controls + NOT on target */
                case OpCode.OR:
                {
                    var xgate = Factory.Get<X, X>();                                    /* X - gate */
                    var cx = new ControlledOperation<Qubit, QVoid>(xgate);              /* controlled X */
                    var toeach = Factory.Get<ApplyToEach<Qubit>, ApplyToEach<Qubit>>(); /* apply to each */
                    toeach.Body.Invoke((xgate, controls));
                    cx.Body.Invoke((controls, target));
                    xgate.Body.Invoke(target);
                    toeach.Body.Invoke((xgate, controls));
                } break;

                /* CNOTs for each input on target */
                case OpCode.XOR:
                {
                    var cnot = Factory.Get<CNOT, CNOT>();
                    foreach (var qb in controls)
                    {
                        cnot.Body.Invoke((qb, target));
                    }
                } break;

                /* CNOTs for each input on target + NOT on target */
                case OpCode.IFF:
                {
                    var cnot = Factory.Get<CNOT, CNOT>();
                    var xgate = Factory.Get<X, X>();
                    foreach (var qb in controls)
                    {
                        cnot.Body.Invoke((qb, target));
                    }
                    xgate.Body.Invoke(target);
                } break;
            } 
        }

        public int RequiredAncillae { get => _steps.Count - 1; }

        private enum OpCode { AND, OR, XOR, IFF };
        private List<(OpCode, List<int>)> _steps = new List<(OpCode, List<int>)>();
        private SimulatorBase _simbase;
        private QArray<Qubit> _inputs;
        private Qubit _output;
        private QArray<Qubit> _ancillae;
    }
}
