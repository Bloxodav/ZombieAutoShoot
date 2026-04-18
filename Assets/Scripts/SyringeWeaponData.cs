using UnityEngine;

[CreateAssetMenu(fileName = "SyringeWeaponData", menuName = "Weapons/Syringe Weapon Data")]
public class SyringeWeaponData : ScriptableObject
{
    [Header("Stats")]
    public float projectileSpeed = 25f;
    public float range = 20f;
    public float fireRate = 0.6f;
    public float alliedDuration = 8f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    public AudioClip hitSound;

    [Header("Trail")]
    public GameObject trailPrefab;

    [Header("Magazine")]
    public int magazineSize = 10;
    public float reloadTime = 2f;
    public AudioClip reloadSound;
}