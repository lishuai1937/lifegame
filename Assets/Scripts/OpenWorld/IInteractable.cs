/// <summary>
/// 可交互物体接口
/// 所有NPC、物品、触发器都实现此接口
/// </summary>
public interface IInteractable
{
    string InteractionPrompt { get; } // 交互提示文本（如"按E对话"）
    void Interact();
}
