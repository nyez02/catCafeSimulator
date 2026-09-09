using System.Collections.Generic;
using UnityEngine;

public class FoodBowl : MonoBehaviour
{
    public static List<FoodBowl> AllBowls = new List<FoodBowl>();

    [Header("Bowl Settings")]
    public float maxFood = 100f;
    public float currentFood = 50f;
    public GameObject foodVisual; // Mesh thức ăn bên trong bát

    private void OnEnable()
    {
        if (!AllBowls.Contains(this))
        {
            AllBowls.Add(this);
        }
        UpdateVisual();
    }

    private void OnDisable()
    {
        AllBowls.Remove(this);
    }

    public bool HasFood()
    {
        return currentFood > 5f;
    }

    public void FillFood()
    {
        currentFood = maxFood;
        UpdateVisual();
    }

    public float Eat(float amount)
    {
        float eaten = Mathf.Min(currentFood, amount);
        currentFood -= eaten;
        UpdateVisual();
        return eaten;
    }

    private void UpdateVisual()
    {
        if (foodVisual != null)
        {
            foodVisual.SetActive(currentFood > 0f);
        }
    }

    public static FoodBowl GetNearestAvailableBowl(Vector3 position)
    {
        FoodBowl nearest = null;
        float minDist = float.MaxValue;

        foreach (var bowl in AllBowls)
        {
            if (bowl != null && bowl.HasFood())
            {
                float dist = Vector3.Distance(position, bowl.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = bowl;
                }
            }
        }
        return nearest;
    }
}
