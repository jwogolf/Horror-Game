using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public List<CraftingRecipe> recipes;

    public ItemData TryCraft(ItemData itemA, ItemData itemB)
    {
        foreach (var recipe in recipes)
        {
            if ((recipe.ingredientA == itemA && recipe.ingredientB == itemB) ||
                (recipe.ingredientA == itemB && recipe.ingredientB == itemA))
            {
                return recipe.result;
            }
        }
        return null;
    }
}
