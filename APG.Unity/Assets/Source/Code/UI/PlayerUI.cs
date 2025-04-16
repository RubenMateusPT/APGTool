using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lifesText;

    public void UpdateLifes(int value)
    {
        lifesText.text = $"x {value}";
    }
}
