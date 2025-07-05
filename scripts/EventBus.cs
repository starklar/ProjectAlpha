using Godot;
using Godot.Collections;

public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }

    [Signal]
    public delegate void saveGameEventHandler(string fileName);

    [Signal]
    public delegate void loadGameEventHandler(string fileName);

    [Signal]
    public delegate void loadChapterDataEventHandler(string chapterName);

    [Signal]
    public delegate void initializeBoardEventHandler();

    [Signal]
    public delegate void acceptPressedEventHandler(Vector2I cell);

    [Signal]
    public delegate void movedEventHandler(Vector2I newCell);

    [Signal]
    public delegate void enterUnitMenuEventHandler(Array<string> possibleCommands);

    [Signal]
    public delegate void exitUnitMenuEventHandler();

    [Signal]
    public delegate void enterUseSkillMenuEventHandler(int basicAttackIndex, Array<int> skillIndexes);

    [Signal]
    public delegate void exitUseSkillMenuEventHandler();

    [Signal]
    public delegate void enableCursorEventHandler(bool enableInput);

    [Signal]
    public delegate void unitMenuButtonPressedEventHandler(string option);

    [Signal]
    public delegate void useSkillMenuButtonPressedEventHandler(int option);

    [Signal]
    public delegate void leftCombatValuesEventHandler();

    [Signal]
    public delegate void rightCombatValuesEventHandler();

    [Signal]
    public delegate void enterCombatEventHandler();

    [Signal]
    public delegate void exitCombatEventHandler(int leftHP, int leftMP, int rightHP, int rightMP);

    [Signal]
    public delegate void setUpMapUIEventHandler(string unitName, string unitHP, string unitMP, string resistances, string skillName, string affinityBonus);

    [Signal]
    public delegate void hideMapUIEventHandler();

    [Signal]
    public delegate void setUpLeftNumbersEventHandler(int damage, int accuracy, int criticalRate, int criticalMultiplier);

    [Signal]
    public delegate void setUpRightNumbersEventHandler(int damage, int accuracy, int criticalRate, int criticalMultiplier);


    [Signal]
    public delegate void CombatUnitHealthChangedEventHandler(int health);

    [Signal]
    public delegate void CombatUnitDiedEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }
}
