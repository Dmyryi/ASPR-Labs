using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Stages;

namespace Lab01.Logic.Gomori;


public sealed class GomorySolver : IGomorySolver
{
    private const string Stage = "пошук цілочислового розв'язку (Гоморі)";

    private readonly IJordan _jordan;
    private readonly IPivotSelector _pivotSelector;

    private int _lpInvocationIndex;
    private int _gomoryCutCount;

    public GomorySolver(IJordan jordan, IPivotSelector pivotSelector)
    {
        _jordan = jordan;
        _pivotSelector = pivotSelector;
    }

    public SolverResult Solve(
        double[] vectorZ,
        double[,] matrixA,
        double[] vectorB,
        OptimizationMode mode,
        GomoryOptions? options = null,
        ISimplexProtocol? protocol = null)
    {
        var gomoryOptions = options ?? GomoryOptions.Default;
        var simplexOptions = SimplexOptions.Default;
        var cutBuilder = new GomoryCutBuilder(gomoryOptions.IntegerEpsilon);

        _lpInvocationIndex = 0;
        _gomoryCutCount = 0;

        double[] preparedZ = PrepareObjective(vectorZ, mode);
        var tableau = new SimplexTableau(matrixA, vectorB, preparedZ);

        SolveLpPhase(tableau, protocol, simplexOptions, mode);

        for (int iteration = 1; iteration <= gomoryOptions.MaxCuts; iteration++)
        {
            int row = cutBuilder.FindMostFractionalRow(tableau);
            if (row == -1)
                return BuildResult(tableau, mode);

            int basisVar = tableau.BasisVariables[row];
            double bValue = tableau.GetB(row);
            double frac = cutBuilder.FractionalComponent(bValue);

            protocol?.LogGomoryFractionalSolution(basisVar, bValue, frac);

            GomoryCut cut = cutBuilder.Build(tableau, row);
            int newBasisId = ComputeNextBasisId(tableau);
            _gomoryCutCount++;

            protocol?.LogGomoryCutEquation(_gomoryCutCount, tableau, cut.Coefficients, cut.Rhs);
            tableau.AppendBasisRow(cut.Coefficients, cut.Rhs, newBasisId);
            protocol?.LogTableau("Симплекс-таблиця з новим обмеженням:", tableau);

            SolveLpPhase(tableau, protocol, simplexOptions, mode);
        }

        throw new IterationLimitExceededException(gomoryOptions.MaxCuts, Stage);
    }


    private void SolveLpPhase(
        SimplexTableau tableau,
        ISimplexProtocol? protocol,
        SimplexOptions simplexOptions,
        OptimizationMode mode)
    {
        bool logInitial = _lpInvocationIndex++ == 0;

        var basicFinder = new BasicSolutionFinder(
            _jordan, _pivotSelector, simplexOptions, protocol,
            logInitialTableau: logInitial,
            logPivotStepNumbers: false);

        var optimalFinder = new OptimalSolutionFinder(
            _jordan, _pivotSelector, mode, simplexOptions, protocol,
            logPivotStepNumbers: false);

        basicFinder.Find(tableau);
        protocol?.LogBasicSolution(tableau);
        optimalFinder.Find(tableau);
        protocol?.LogContinuousOptimalSolution(tableau);
    }

    private static double[] PrepareObjective(double[] vectorZ, OptimizationMode mode)
    {
        if (mode == OptimizationMode.Maximization) return vectorZ;

        var inverted = new double[vectorZ.Length];
        for (int i = 0; i < vectorZ.Length; i++)
            inverted[i] = -vectorZ[i];
        return inverted;
    }

    private static int ComputeNextBasisId(SimplexTableau tableau)
    {
        int max = tableau.ProblemVariableCount - 1;
        foreach (int id in tableau.BasisVariables)
            if (id > max) max = id;
        foreach (int id in tableau.ColumnVariables)
            if (id > max) max = id;
        return max + 1;
    }

    private static SolverResult BuildResult(SimplexTableau tableau, OptimizationMode mode)
    {
        var x = new double[tableau.ProblemVariableCount];
        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int varIndex = tableau.BasisVariables[row];
            if (varIndex >= 0 && varIndex < tableau.ProblemVariableCount)
                x[varIndex] = tableau.GetB(row);
        }

        double[] u = DualMultiplierExtractor.FromFinalTableau(tableau, mode);

        return new SolverResult
        {
            X = x,
            Y = Array.Empty<double>(),
            U = u,
            Z = tableau.Data[tableau.RowsCount, tableau.ColsCount],
            Success = true
        };
    }
}
