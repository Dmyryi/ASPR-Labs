using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Protocols;
using Lab01.Logic.Simplex.Solvers;
using Lab01.Logic.Simplex.Stages;

namespace Lab01.Logic.Assignment;

public static class AssignmentSimplexRunner
{
    public static void Run(double[,] costs, int n, AssignmentTrace trace)
    {
        string objective = AssignmentLpBuilder.BuildObjectiveText(costs, n);
        string dualObjective = AssignmentLpBuilder.BuildDualObjectiveText(costs, n);
        string constraints = AssignmentLpBuilder.BuildConstraintsText(n);
        var program = new LinearProgramParser().Parse(objective, constraints);

        var jordan = new JordanSolver();
        var pivot = new PivotSelector();
        var protocol = new SimplexProtocol();
        var basicFinder = new BasicSolutionFinder(jordan, pivot, SimplexOptions.Default, protocol, logPivotStepNumbers: false);
        var optimalFinder = new OptimalSolutionFinder(jordan, pivot, OptimizationMode.Minimization, SimplexOptions.Default, protocol, logPivotStepNumbers: false);
        var solver = new MinimizationSolver(basicFinder, optimalFinder, protocol);

        protocol.StartTransportation(objective, dualObjective, constraints, program);

        var vectorZ = (double[])program.ObjectiveCoefficients.Clone();
        SolverResult result = solver.Solve(vectorZ, program.ConstraintMatrix, program.RightHandSide);
        protocol.LogResult(result);
        trace.SimplexSolution = result.X;
        trace.SimplexMinCost = result.Z;
        trace.SimplexProtocolText = protocol.GetText();
    }
}
