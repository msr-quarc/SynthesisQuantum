// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Quantum.Canon;
using Microsoft.Quantum.Pebbling;
using Microsoft.Quantum.Primitive;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Quantum.Samples.SATSolver
{
    /// <summary>
    /// This is a class to perform a reversible computation.
    /// 
    /// The computation is applied to a list of control qubits and a target qubit.
    /// The computation computes a single output value, based on several computation steps.
    /// All but the last steps are computed on clean ancillae qubits, which are allocated by the operation itself.
    ///
    /// Steps are combinational operations such as AND, OR, XOR, etc. Inputs to the steps can be control
    /// qubits, indicated by a nonnegative integer starting from 0, or a previous step using a negative integer
    /// starting from -1 and decreasing.
    /// 
    /// <example>
    /// The following example illustrates how to prepare a reversible computation for a simulator that computes the
    /// if-then-else operation, with condition on qubit 0, then case on qubit 1, and else case on qubit 2:
    /// <code>
    /// using (var sim = new QuantumSimulator()) {
    ///     var comp = sim.Get<ReversibleComputation, ReversibleComputation>();
    ///     var then_case = comp.ComputeAnd(new List<int>{0, 1});
    ///     var cond_neg = comp.ComputeNot(0);
    ///     var else_case = comp.ComputeAnd(new List<int>{cond_neg, 2});
    ///     var ite = comp.ComputeOr(new List<int>{then_case, else_case});
    ///     // use comp as oracle in computation
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class ReversibleComputation : Operation<(QArray<Qubit>, Qubit), QVoid>
    {
        public ReversibleComputation(IOperationFactory m) : base(m)
        {
        }

        /// <summary>
        /// When initializing a reversible computation, we cast the internal factory into a SimulatorBase, if possible.
        /// This is needed to allocate and release ancillae qubits.
        /// Not all simulator implementations derive from it.  If this is the case, an exception is raised.
        /// </summary>
        public override void Init()
        {
            _simbase = Factory as SimulatorBase;
            if (_simbase == null)
            {
                throw new ExecutionFailException("simulator for reversible computation does not allow to allocate qubits");
            }
        }

        public void CreateInputs(int numInputs)
        {
            _graph = new PebbleGraph(numInputs);
        }

        /// <summary>
        /// Computes FALSE
        /// </summary>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeFalse()
        {
            //_steps.Add((OpCode.FALSE, new List<int> { }));
            //return -_steps.Count;

            var n = _graph.AddNode();
            _nodeTypes[n] = OpCode.FALSE;
            return n;
        }

        /// <summary>
        /// Computes TRUE
        /// </summary>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeTrue()
        {
            //_steps.Add((OpCode.TRUE, new List<int> { }));
            //return -_steps.Count;

            var n = _graph.AddNode();
            _nodeTypes[n] = OpCode.TRUE;
            return n;
        }

        /// <summary>
        /// Perform the NOT operation of a previous step or a primary input.
        /// </summary>
        /// <param name="qubit">Index or primary input or previous step</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeNot(int qubit)
        {
            //_steps.Add((OpCode.NOT, new List<int> { qubit }));
            //return -_steps.Count;

            var n = _graph.AddNode(qubit);
            _nodeTypes[n] = OpCode.NOT;
            return n;
        }

        /// <summary>
        /// Perform the AND operation of previous steps or primary inputs.
        /// </summary>
        /// <param name="qubits">List of indexes of primary inputs or previous steps</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeAnd(List<int> qubits)
        {
            //_steps.Add((OpCode.AND, qubits));
            //return -_steps.Count;

            var n = _graph.AddNode(qubits);
            _nodeTypes[n] = OpCode.AND;
            return n;
        }

        /// <summary>
        /// Perform the OR operation of previous steps or primary inputs.
        /// </summary>
        /// <param name="qubits">List of indexes of primary inputs or previous steps</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeOr(List<int> qubits)
        {
            //_steps.Add((OpCode.OR, qubits));
            //return -_steps.Count;

            var n = _graph.AddNode(qubits);
            _nodeTypes[n] = OpCode.OR;
            return n;
        }

        /// <summary>
        /// Perform the XOR operation of previous steps or primary inputs.
        /// </summary>
        /// <param name="qubits">List of indexes of primary inputs or previous steps</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeXor(List<int> qubits)
        {
            //_steps.Add((OpCode.XOR, qubits));
            //return -_steps.Count;

            var n = _graph.AddNode(qubits);
            _nodeTypes[n] = OpCode.XOR;
            return n;
        }

        /// <summary>
        /// Perform the IFF (XNOR, equals) operation of previous steps or primary inputs.
        /// </summary>
        /// <param name="qubit1">Index of primary input or previous step</param>
        /// <param name="qubit2">Index of primary input or previous step</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeIff(int qubit1, int qubit2)
        {
            //_steps.Add((OpCode.IFF, new List<int> { qubit1, qubit2 }));
            //return -_steps.Count;

            var n = _graph.AddNode(qubit1, qubit2);
            _nodeTypes[n] = OpCode.IFF;
            return n;
        }

        /// <summary>
        /// Perform the IMPLIES operation of previous steps or primary inputs.
        /// </summary>
        /// <param name="qubit1">Index of primary input or previous step</param>
        /// <param name="qubit2">Index of primary input or previous step</param>
        /// <returns>Next free step index holding the result of the computation</returns>
        public int ComputeImplies(int qubit1, int qubit2)
        {
            //_steps.Add((OpCode.IMPLIES, new List<int> { qubit1, qubit2 }));
            //return -_steps.Count;

            var n = _graph.AddNode(qubit1, qubit2);
            _nodeTypes[n] = OpCode.IMPLIES;
            return n;
        }

        /// <summary>
        /// Applies the operation. In here the expression list is traversed and gates are applied
        /// for each step, depending on the operation.  This function also allocates and releases
        /// the required clean ancillae.
        /// </summary>
        public override Func<(QArray<Qubit>, Qubit), QVoid> Body
        {
            get
            {
                return delegate ((QArray<Qubit>, Qubit) args)
                {
                    /* save qubits for internal use */
                    (_inputs, _output) = args;

                    /* set the output of the computation graph to the last step */
                    _graph.Outputs.Clear();
                    _graph.Outputs.Add(_graph.Size - 1);

                    /* reset gate count */
                    _gateCount = 0;

                    /* assign nodeToQubit map */
                    for (var i = 0; i < _graph.NumInputs; ++i) {
                        _nodeToQubit[i] = _inputs[i];
                    }

                    /* find solution */
                    var solution = PebbleSolver.SolveWithBennett(_graph);

                    foreach (var op in solution.GetOperations()) {
                        switch (op.action) {
                            case PebbleSolution.Action.Compute:
                                _nodeToQubit[op.index] = _graph.IsOutput(op.index) ? _output : _simbase.QubitManager.Allocate(1).First();
                                ApplyStep(op.index);
                            break;
                            case PebbleSolution.Action.Uncompute:
                                ApplyStep(op.index);
                                _simbase.QubitManager.Release(new QArray<Qubit> {_nodeToQubit[op.index]});
                            break;
                        }
                    }

                    return QVoid.Instance;
                };
            }
        }

        /// <summary>
        /// Get control qubits and target qubit for a step.
        /// 
        /// This method maps step and input indexes to ancillae qubits, control qubits and target qubits.
        /// </summary>
        /// <param name="node">Index of the step</param>
        /// <returns></returns>
        private (QArray<Qubit>, Qubit) GetQubits(int node)
        {
            var children = new QArray<Qubit>();
            foreach (var child in _graph[node].Children) {
                children.Add(_nodeToQubit[child]);
            }
            return (children, _nodeToQubit[node]);
        }

        /// <summary>
        /// Applies step.  This function applies the necessary quantum operations to perform a step operation.
        /// </summary>
        /// <param name="node">Index of the step</param>
        private void ApplyStep(int node)
        {
            var (controls, target) = GetQubits(node);

            switch (_nodeTypes[node])
            {
                case OpCode.FALSE:
                    {
                        /* do nothing */
                    } break;

                case OpCode.TRUE:
                    {
                        ApplyNOT(target);
                    } break;

                case OpCode.NOT:
                    {
                        ApplyCNOT(controls[0], target);
                        ApplyNOT(target);
                    } break;

                case OpCode.AND:
                    {
                        ApplyToffoli(controls, target);
                    } break;

                case OpCode.OR:
                    {
                        ApplyToffoli(controls, target, controls, true);
                    } break;

                case OpCode.XOR:
                    {
                        foreach (var qb in controls)
                        {
                            ApplyCNOT(qb, target);
                        }
                    } break;

                case OpCode.IFF:
                    {
                        foreach (var qb in controls)
                        {
                            ApplyCNOT(qb, target);
                        }
                        ApplyNOT(target);
                    } break;

                case OpCode.IMPLIES:
                    {
                        ApplyToffoli(controls, target, new QArray<Qubit>{controls[1]}, true);
                    } break;
            }
        }

        private void ApplyNOT(Qubit target)
        {
            ApplyToffoli(new QArray<Qubit> { }, target);
        }

        private void ApplyCNOT(Qubit control, Qubit target)
        {
            ApplyToffoli(new QArray<Qubit> { control }, target);
        }

        private void ApplyToffoli(QArray<Qubit> controls, Qubit target, QArray<Qubit> invertedControls = null, bool invertOutput = false)
        {
            var xgate = Factory.Get<X, X>();                                    /* X - gate */
            var cx = new ControlledOperation<Qubit, QVoid>(xgate);              /* controlled X */
            var toeach = Factory.Get<ApplyToEach<Qubit>, ApplyToEach<Qubit>>(); /* apply to each */

            if (invertedControls != null)
            {
                _gateCount += invertedControls.Count;
                toeach.Body.Invoke((xgate, invertedControls));
            }
            _gateCount++;
            cx.Body.Invoke((controls, target));
            if (invertedControls != null)
            {
                _gateCount += invertedControls.Count;
                toeach.Body.Invoke((xgate, invertedControls));
            }

            if (invertOutput)
            {
                _gateCount++;
                xgate.Body.Invoke(target);
            }
        }

        /// <summary>
        /// Returns number of required ancillae.
        /// </summary>
        public int RequiredAncillae { get => _graph.Size - _graph.NumInputs - 1; }

        /// <summary>
        /// Returns number of reversible gates in computation (available after execution).
        /// </summary>
        public int GateCount { get => _gateCount; }

        private enum OpCode { FALSE, TRUE, NOT, AND, OR, XOR, IFF, IMPLIES };
        private PebbleGraph _graph;
        private Dictionary<int, OpCode> _nodeTypes = new Dictionary<int, OpCode>();
        private Dictionary<int, Qubit> _nodeToQubit = new Dictionary<int, Qubit>();
        private SimulatorBase _simbase;
        private QArray<Qubit> _inputs;
        private Qubit _output;
        private int _gateCount = 0;
    }
}
