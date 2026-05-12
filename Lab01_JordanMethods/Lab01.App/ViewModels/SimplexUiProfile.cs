namespace Lab01.App.ViewModels;

public sealed class SimplexUiProfile
{
    public static SimplexUiProfile Primal { get; } = new(
        pageTitle: "Simplex Optimization",
        pageBadge: "Z • первинна задача",
        objectiveSectionTitle: "Objective Function Z",
        objectiveHint: "приклад: x1 + 2x2 - x3 - x4",
        constraintsHint:
            "одне на рядок, оператори: <=, >=, =. Приклад: x1 + x2 - x3 - 2x4 <= 6",
        resultSectionTitle: "Simplex Result",
        resultObjectiveSymbol: "Z",
        protocolFilePreamble: "=== Симплекс-метод (первинна задача) ===",
        isDual: false,
        contextBanner: null,
        defaultObjective: null,
        defaultConstraints: null);

    public static SimplexUiProfile Dual { get; } = new(
        pageTitle: "Двоїста задача лінійного програмування",
        pageBadge: "W • двоїста задача",
        objectiveSectionTitle: "Цільова функція W",
        objectiveHint:
            "приклад: 4x1 + 3x2 (двоїсті множники теж як x1, x2, …)",
        constraintsHint:
            "одне на рядок, оператори: <=, >=, =. Введіть уже побудовану двоїсту задачу W.",
        resultSectionTitle: "Результат симплексу (W)",
        resultObjectiveSymbol: "W",
        protocolFilePreamble: "=== Двоїста задача W (симплекс-метод) ===",
        isDual: true,
        contextBanner:
            "Це вікно призначене лише для двоїстої задачі W. Побудуйте W за правилами двоїстності до Z, введіть її нижче й запустіть симплекс (мінімізація або максималізація — згідно з напрямком W).",
        defaultObjective: "4x1 + 3x2",
        defaultConstraints: "x1 + x2 >= 2\n2x1 + x2 >= 1");

    private SimplexUiProfile(
        string pageTitle,
        string pageBadge,
        string objectiveSectionTitle,
        string objectiveHint,
        string constraintsHint,
        string resultSectionTitle,
        string resultObjectiveSymbol,
        string protocolFilePreamble,
        bool isDual,
        string? contextBanner,
        string? defaultObjective,
        string? defaultConstraints)
    {
        PageTitle = pageTitle;
        PageBadge = pageBadge;
        ObjectiveSectionTitle = objectiveSectionTitle;
        ObjectiveHint = objectiveHint;
        ConstraintsHint = constraintsHint;
        ResultSectionTitle = resultSectionTitle;
        ResultObjectiveSymbol = resultObjectiveSymbol;
        ProtocolFilePreamble = protocolFilePreamble;
        IsDual = isDual;
        ContextBanner = contextBanner;
        DefaultObjectiveText = defaultObjective;
        DefaultConstraintsText = defaultConstraints;
    }

    public string PageTitle { get; }
    public string PageBadge { get; }
    public string ObjectiveSectionTitle { get; }
    public string ObjectiveHint { get; }
    public string ConstraintsHint { get; }
    public string ResultSectionTitle { get; }
    public string ResultObjectiveSymbol { get; }
    public string ProtocolFilePreamble { get; }
    public bool IsDual { get; }
    public string? ContextBanner { get; }
    public string? DefaultObjectiveText { get; }
    public string? DefaultConstraintsText { get; }
}
