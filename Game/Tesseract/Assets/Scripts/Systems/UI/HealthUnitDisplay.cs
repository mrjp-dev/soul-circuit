using UnityEngine;
using UnityEngine.UI;

// One heart of the health bar. A heart holds four quarters of health, so its sprite is
// chosen by how many quarters are filled: sprites[0] is empty and sprites[4] is full.
// An array rather than one field per state, so a finer-grained heart is only more sprites.
public class HealthUnitDisplay : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    public void SetQuarters(int filled) => GetComponent<Image>().sprite = sprites[filled];
}
