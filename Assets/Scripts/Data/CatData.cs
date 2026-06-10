using UnityEngine;

[CreateAssetMenu(fileName = "NewCatData", menuName = "Cat Cafe/Cat Data")]
public class CatData : ScriptableObject
{
    public string catName;
    public string breed;
    public int purchaseCost;
    
    [Header("Base Stats")]
    public float maxHappiness = 100f;
    public float maxHunger = 100f;
    public float baseMovementSpeed = 2f;
    
    [Header("Visuals")]
    public GameObject catPrefab;
    public Sprite icon;
}
