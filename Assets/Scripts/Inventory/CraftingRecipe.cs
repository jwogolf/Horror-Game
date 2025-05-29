using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemData ingredientA;
    public ItemData ingredientB;
    public ItemData result;
}
