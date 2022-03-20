using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun shooting type", menuName = "New gun shooting type")]
public class ShootingMode : ScriptableObject
{
    [Tooltip("Identifica la modalita' di sparo")]
    public bool isDoubleGunType;

    [Header("Stats")]
    [Tooltip("Quanti proiettili al massimo contiene questa modalita'")]
    [Min(1)] public int maxCapacity;

    [Tooltip("Cadenza di fuoco in questa modalita'")]
    [Min(.01f)] public float reloadTime;

    [Tooltip("Danni in questa modalita'")]
    [Min(0)] public int damage;

    [Tooltip("Raggio di portata in questa modalita'")]
    [Min(.1f)] public float attackRange;
}
