using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Stats")]
    public int damage;
    public float range;
    public float fireRate;
    public float shootAngle;

    [Header("Shotgun")]
    public int pellets = 6;
    public float spreadAngle = 8f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    [Header("Sounds")]
    public AudioClip[] shootSounds;
    public AudioClip reloadSound;

    [Header("Magazine")]
    public int magazineSize = 30;
    public float reloadTime = 1.5f;

    public AudioClip GetRandomShootSound()
    {
        if (shootSounds == null || shootSounds.Length == 0) return null;
        return shootSounds[Random.Range(0, shootSounds.Length)];
    }
}