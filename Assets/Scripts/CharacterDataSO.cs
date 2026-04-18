using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Identity")]
    public int characterId;
    public string characterName;

    [Header("Stats")]
    public int maxHP;
    public float moveSpeed;
    public int startAmmo;

    [Header("Weapon")]
    public WeaponData weapon;
}
