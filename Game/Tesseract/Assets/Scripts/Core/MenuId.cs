// Index into MenuManager's menu list. Inspector buttons pass these as raw ints, because
// UnityEvents cannot pass enums, so this order must match the list in the scene.
public enum MenuId { Options, Inventory, ConfirmReturnToStart, ConfirmExitGame, Dialogue }
