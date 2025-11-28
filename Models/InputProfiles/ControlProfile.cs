namespace DS3InputMaster.Models.InputProfiles
{
    /// <summary>
    /// Полный профиль управления для разных ситуаций
    /// </summary>
    public class ControlProfile
    {
        public string Name { get; set; } = "Default";
        public string Description { get; set; } = "Базовый профиль управления";
        
        public MouseSettings Mouse { get; set; } = new();
        public MovementSettings Movement { get; set; } = new();
        public CombatSettings Combat { get; set; } = new();
        public KeyBindings Bindings { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class MouseSettings
    {
        public float CameraSensitivity { get; set; } = 1.0f;
        public float AimingSensitivity { get; set; } = 0.7f;
        public float BowSensitivity { get; set; } = 0.5f;
        public bool InvertY { get; set; } = false;
        public float Smoothing { get; set; } = 0.1f;
    }

    public class MovementSettings
    {
        public float WalkThreshold { get; set; } = 0.3f;
        public float RunThreshold { get; set; } = 0.8f;
        public float AnalogResponseCurve { get; set; } = 1.5f;
        public bool ToggleSprint { get; set; } = true;
    }

    public class CombatSettings
    {
        public float QuickAttackTiming { get; set; } = 0.15f;
        public float ParryWindow { get; set; } = 0.2f;
        public bool AutoWeaponSwap { get; set; } = false;
        public float DodgeAttackDelay { get; set; } = 0.1f;
    }

    public class KeyBindings
    {
        public Dictionary<GameAction, InputBinding> Actions { get; set; } = new();
        
        public InputBinding GetBinding(GameAction action)
        {
            return Actions.TryGetValue(action, out var binding) ? binding : InputBinding.Empty;
        }
    }

    public struct InputBinding
    {
        public VirtualKey PrimaryKey { get; set; }
        public VirtualKey SecondaryKey { get; set; }
        public MouseButton MouseButton { get; set; }
        public InputModifier Modifiers { get; set; }
        
        public static InputBinding Empty => new();
        
        public bool IsEmpty => PrimaryKey == VirtualKey.None && 
                              SecondaryKey == VirtualKey.None && 
                              MouseButton == MouseButton.None;
    }

    [Flags]
    public enum InputModifier
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4
    }

    public enum GameAction
    {
        // Движение
        MoveForward,
        MoveBackward,
        MoveLeft, 
        MoveRight,
        Jump,
        Roll,
        Sprint,
        Walk,
        
        // Комбат
        LightAttack,
        HeavyAttack,
        Parry,
        UseItem,
        SwitchSpell,
        SwitchItem,
        
        // Камера
        LockOn,
        ResetCamera,
        
        // Интерфейс
        Menu,
        Gesture,
        SwitchRightHand,
        SwitchLeftHand
    }
}
