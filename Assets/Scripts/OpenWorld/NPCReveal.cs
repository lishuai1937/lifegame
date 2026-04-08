using UnityEngine;

/// <summary>
/// NPC Reveal System - "Shadow People" mechanic
/// 
/// DESIGN RULE:
/// - All NPCs start as black silhouettes (shadow material)
/// - Only after the player talks to them, their true appearance is revealed
/// - Important/story NPCs can be set to show their true form immediately
/// - Once revealed, stays revealed for the rest of that grid world visit
/// 
/// This creates a sense of mystery and rewards player exploration/interaction
/// </summary>
[RequireComponent(typeof(NPC))]
public class NPCReveal : MonoBehaviour
{
    [Header("Reveal Settings")]
    public bool IsImportantNPC = false;     // Important NPCs show true form immediately
    public bool IsRevealed = false;         // Runtime state

    [Header("Appearance")]
    public Material ShadowMaterial;         // Black silhouette material (shared)
    public Material TrueMaterial;           // This NPC's real material
    public GameObject ShadowModel;          // Black silhouette model (default)
    public GameObject TrueModel;            // Real model (hidden until revealed)

    [Header("Reveal Effect")]
    public float RevealDuration = 0.8f;     // How long the transition takes
    public Color ShadowColor = Color.black;
    public ParticleSystem RevealParticles;  // Optional particle effect on reveal

    private Renderer[] renderers;
    private float revealProgress = 0f;
    private bool isRevealing = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        if (IsImportantNPC)
        {
            // Important NPCs show true form from the start
            ShowTrueForm(instant: true);
        }
        else
        {
            // Regular NPCs start as shadows
            ApplyShadowAppearance();
        }
    }

    /// <summary>
    /// Called when player first talks to this NPC
    /// Triggers the reveal transition
    /// </summary>
    public void Reveal()
    {
        if (IsRevealed || isRevealing) return;
        isRevealing = true;
        Debug.Log($"[NPCReveal] Revealing: {gameObject.name}");

        if (RevealParticles != null)
            RevealParticles.Play();

        // Start reveal coroutine
        StartCoroutine(RevealCoroutine());
    }

    System.Collections.IEnumerator RevealCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < RevealDuration)
        {
            elapsed += Time.deltaTime;
            revealProgress = Mathf.Clamp01(elapsed / RevealDuration);

            // Lerp shadow color to white during transition
            Color current = Color.Lerp(ShadowColor, Color.white, revealProgress);
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                    r.material.color = current;
            }

            yield return null;
        }

        ShowTrueForm(instant: false);
        isRevealing = false;
        IsRevealed = true;
    }

    void ApplyShadowAppearance()
    {
        // Apply shadow material to all renderers
        if (ShadowMaterial != null)
        {
            foreach (var r in renderers)
            {
                if (r != null) r.material = ShadowMaterial;
            }
        }
        else
        {
            // No shadow material assigned - just make everything black
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                    r.material.color = ShadowColor;
            }
        }

        // Model swap: show shadow, hide true
        if (ShadowModel != null) ShadowModel.SetActive(true);
        if (TrueModel != null) TrueModel.SetActive(false);
    }

    void ShowTrueForm(bool instant)
    {
        IsRevealed = true;

        // Apply true material
        if (TrueMaterial != null)
        {
            foreach (var r in renderers)
            {
                if (r != null) r.material = TrueMaterial;
            }
        }
        else
        {
            // No true material - just reset to white
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                    r.material.color = Color.white;
            }
        }

        // Model swap: hide shadow, show true
        if (ShadowModel != null) ShadowModel.SetActive(false);
        if (TrueModel != null) TrueModel.SetActive(true);

        if (!instant)
            Debug.Log($"[NPCReveal] {gameObject.name} revealed!");
    }
}