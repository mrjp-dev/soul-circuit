using UnityEngine;

public class NonPlayable : MonoBehaviour
{
    [SerializeField] protected NPData baseData;

    protected GameObject gameObj; // non-playable's game object
    protected CustomTag customTagComp; // non-playable's custom tags
    protected Renderer rendererComp;
    protected Collider colliderComp;

    public NPData BaseData { get => baseData; set => baseData = value; }
    public Renderer RendererComp { get => rendererComp; set => rendererComp = value; }
    public Collider ColliderComp { get => colliderComp; set => colliderComp = value; }
    public GameObject GameObj { get => gameObj; set => gameObj = value; }
    public CustomTag CustomTagComp { get => customTagComp; set => customTagComp = value; }

    public void ToggleInteractable(bool state)
    {
        BaseData.IsInteractable = state;
        gameObj.tag = BaseData.IsInteractable ? "Interactable" : "Untagged";

    }

    protected virtual void Awake()
    {

    }
}
