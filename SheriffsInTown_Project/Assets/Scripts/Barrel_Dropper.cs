using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Barrel_Dropper : Barrel
{
    /// <summary>
    /// La probabilità è una barra da 0 a 100
    /// Gli item droppabili vengono "distribuiti" su questa barra in ordine di importanza
    /// Gli item più vicini allo 0 sono meno importanti, quelli più vicini a 100 sono più preziosi
    /// In ordine di importanza dal meno al più importante:
    /// dropNothing / dropStar / dropHeal
    /// </summary>
    [Tooltip("Probabilità che droppi una cura alla sua distruzione")]
    [SerializeField] [Range(0f, 100f)] float dropHeal;

    [Tooltip("Probabilità che droppi la stella dello sceriffo alla sua distruzione")]
    [SerializeField] [Range(0f, 100f)] float dropStar;

    [Tooltip("Prefab della cura da spawnare")]
    [SerializeField] GameObject healPickupPrefab;

    [Tooltip("Prefab della stella da spawnare")]
    [SerializeField] GameObject starPickupPrefab;


    private void Start()
    {
        dropHeal = 100f - dropHeal; //Stabilisco il range dropHeal(%) - 100%
        dropStar = dropHeal - dropStar; //Stabilisco il range dropStar(%) - dropHeal(%)

        //Da 0% a dropStar% non droppa nulla
    }
    public override void Destroy()
    {
        DropItem();
        base.Destroy();
    }

    private void DropItem()
    {
        float drawn = Random.Range(0f, 100f);
        if(drawn >= dropHeal)
        {
            //Droppa cura
            //Da far vedere a marco
            Instantiate(healPickupPrefab, transform.position, Quaternion.identity);
        }
        else if(drawn >= dropStar)
        {
            //Droppa la stella
            //Dar far vedere a marco
            Instantiate(starPickupPrefab, transform.position, Quaternion.identity);
        }
        //else non droppare niente
    }
}
