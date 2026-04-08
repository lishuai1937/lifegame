using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dialogue UI - shows dialogue text and choices
/// Listens to DialogueSystem events
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject DialoguePanel;
    public Text SpeakerText;
    public Text DialogueText;
    public Button[] ChoiceButtons;  // up to 4 choice buttons
    public Text[] ChoiceTexts;
    public Button ContinueButton;

    void Start()
    {
        if (DialoguePanel != null) DialoguePanel.SetActive(false);

        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueStarted += ShowNode;
            DialogueSystem.Instance.OnNodeChanged += ShowNode;
            DialogueSystem.Instance.OnDialogueEnded += Hide;
        }

        if (ContinueButton != null)
            ContinueButton.onClick.AddListener(OnContinue);

        // Bind choice buttons
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            int idx = i; // capture for closure
            if (ChoiceButtons[i] != null)
                ChoiceButtons[i].onClick.AddListener(() => OnChoice(idx));
        }
    }

    void ShowNode(DialogueNode node)
    {
        if (DialoguePanel != null) DialoguePanel.SetActive(true);
        if (SpeakerText != null) SpeakerText.text = node.Speaker ?? "";
        if (DialogueText != null) DialogueText.text = node.Text ?? "";

        bool hasChoices = node.Choices != null && node.Choices.Length > 0;

        // Show/hide choice buttons
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (ChoiceButtons[i] != null)
            {
                bool show = hasChoices && i < node.Choices.Length;
                ChoiceButtons[i].gameObject.SetActive(show);
                if (show && ChoiceTexts[i] != null)
                    ChoiceTexts[i].text = node.Choices[i].Text;
            }
        }

        // Continue button only when no choices
        if (ContinueButton != null)
            ContinueButton.gameObject.SetActive(!hasChoices);
    }

    void Hide()
    {
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
    }

    void OnChoice(int index)
    {
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.PickChoice(index);
    }

    void OnContinue()
    {
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.Continue();
    }

    void OnDestroy()
    {
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueStarted -= ShowNode;
            DialogueSystem.Instance.OnNodeChanged -= ShowNode;
            DialogueSystem.Instance.OnDialogueEnded -= Hide;
        }
    }
}