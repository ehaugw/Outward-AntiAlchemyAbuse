namespace AntiAlchemyAbuse
{
    using System.Collections.Generic;
    using UnityEngine;
    using BepInEx;
    using HarmonyLib;
    using System;

    [BepInPlugin(GUID, NAME, VERSION)]
    public class AntiAlchemyAbuse : BaseUnityPlugin
    {
        public const string GUID = "com.ehaugw.antialchemyabuse";
        public const string VERSION = "4.0.0";
        public const string NAME = "Anti Alchemy Abuse";

        internal void Awake()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            //SL.OnPacksLoaded += RebalanceRecipeOutcomes;
        }
        
        [HarmonyPatch(typeof(RecipeManager), "LoadCraftingRecipe")]
        public class RecipeManager_LoadCraftingRecipe
        {
            [HarmonyPostfix]
            public static void Postfix(RecipeManager __instance, ref Dictionary<string, Recipe> ___m_recipes)
            {
                RebalanceRecipeOutcomes(___m_recipes ?? new Dictionary<string, Recipe>());
            }
        }

        private static void RebalanceRecipeOutcomes(Dictionary<string, Recipe> recipeDictionary)
        {
            foreach (Recipe recipe in recipeDictionary.Values)
            {
                if (recipe.CraftingStationType != Recipe.CraftingType.Alchemy) continue;
                
                bool hasAdvancedIngredient = false;
                float totalCraftCost = 0;

                foreach (RecipeIngredient ingredient in recipe.Ingredients)
                {
                    switch(ingredient.ActionType)
                    {
                        case RecipeIngredient.ActionTypes.AddGenericIngredient:
                            if (ingredient.AddedIngredient is WaterItem)
                            {
                                hasAdvancedIngredient = true;
                            }
                            break;
                        case RecipeIngredient.ActionTypes.AddSpecificIngredient:
                            totalCraftCost += ingredient.AddedIngredient.RawCurrentValue;
                                break;
                        default:
                            break;
                    }
                }
                if (hasAdvancedIngredient) continue;

                float totalSellCost = 0;

                foreach(ItemReferenceQuantity itemQuantity in recipe.Results)
                {
                    totalSellCost += ResourcesPrefabManager.Instance.GetItemPrefab(itemQuantity.ItemID).RawCurrentValue * 0.3f * itemQuantity.Quantity;
                }
                if (totalSellCost > 0)
                {
                    foreach (ItemReferenceQuantity itemQuantity in recipe.Results)
                    {
                        int oldquant = itemQuantity.Quantity;

                        itemQuantity.Quantity = Mathf.Clamp(Mathf.RoundToInt(itemQuantity.Quantity / totalSellCost * totalCraftCost), 1, itemQuantity.Quantity);
                        if (itemQuantity.Quantity < oldquant)
                            Debug.Log("AntiAlchemyAbuse: " + recipe.Name + " - changed from " +oldquant + "x to " + itemQuantity.Quantity + "x " + ResourcesPrefabManager.Instance.GetItemPrefab(itemQuantity.ItemID).Name + "(s).");
                    }
                }
            }
        }
    }
}