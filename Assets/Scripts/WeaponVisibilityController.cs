using UnityEngine;

public class WeaponVisibilityController : MonoBehaviour
{
    [Header("Оружие 1 (калаш)")]
    public GameObject weapon1;    // в руке
    public GameObject weapon1_2;  // за спиной

    [Header("Оружие 2 (шприц)")]
    public GameObject weapon2;    // в руке
    public GameObject weapon2_2;  // за спиной

    private void Start()
    {
        ShowWeapon1();
    }

    // Вызови через Animation Event в середине анимации switchToSyringe
    public void ShowWeapon2()
    {
        if (weapon1) weapon1.SetActive(false);
        if (weapon1_2) weapon1_2.SetActive(false);
        if (weapon2) weapon2.SetActive(true);
        if (weapon2_2) weapon2_2.SetActive(true);
    }

    // Вызови через Animation Event в середине анимации switchToRifle
    public void ShowWeapon1()
    {
        if (weapon1) weapon1.SetActive(true);
        if (weapon1_2) weapon1_2.SetActive(true);
        if (weapon2) weapon2.SetActive(false);
        if (weapon2_2) weapon2_2.SetActive(false);
    }
}