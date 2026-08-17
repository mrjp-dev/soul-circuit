using UnityEngine;
using UnityEngine.UI;

public enum HealthUnitStatus
{
    Empty = 0,
    QuarterFull = 1,
    Half = 2,
    QuarterEmpty = 3,
    Full = 4
}

public class HealthUnitDisplay : MonoBehaviour
{
    [SerializeField] private Sprite empty, quarterFull, half, quarterEmpty, full;
    [SerializeField] private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetUnitImage(HealthUnitStatus unitStatus)
    {
        switch (unitStatus)
        {
            case (HealthUnitStatus.Empty):
                image.sprite = empty;
                break;

            case (HealthUnitStatus.QuarterFull):
                image.sprite = quarterFull;
                break;

            case (HealthUnitStatus.Half):
                image.sprite = half;
                break;

            case (HealthUnitStatus.QuarterEmpty):
                image.sprite = quarterEmpty;
                break;

            case (HealthUnitStatus.Full):
                image.sprite = full;
                break;
        }
    }
}
