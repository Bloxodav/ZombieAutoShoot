using UnityEngine;
using TMPro;

public class MoneyDisplay : MonoBehaviour
{
    public PlayerProgressSO progress;
    public TextMeshProUGUI moneyText;

    private int _lastCash = -1;

    private void OnEnable()
    {
        if (progress != null)
            progress.OnCashChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (progress != null)
            progress.OnCashChanged -= Refresh;
    }

    public void Refresh()
    {
        if (progress == null || moneyText == null) return;
        if (progress.cash == _lastCash) return;

        _lastCash = progress.cash;
        moneyText.text = _lastCash.ToString();
    }
}
