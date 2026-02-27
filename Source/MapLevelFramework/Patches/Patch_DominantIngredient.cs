using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MapLevelFramework.Patches
{
    /// <summary>
    /// Patch Toils_Recipe.CalculateDominantIngredient 来优先选择非 FixedIngredient 作为产物材质。
    ///
    /// 原版逻辑：按 stackCount 加权随机选择 dominant ingredient
    /// 问题：100精粹 + 60钢铁 → 钢铁有 37.5% 概率被选为 dominant，产物是钢铁材质
    ///
    /// 修复：如果用户在 bill.ingredientFilter 中禁用了某些材料（如钢铁），
    /// 优先从非 FixedIngredient 中选择 dominant ingredient（如精粹）。
    /// </summary>
    [HarmonyPatch(typeof(Toils_Recipe), "CalculateDominantIngredient")]
    public static class Patch_DominantIngredient
    {
        private static bool DebugLog =>
            MapLevelFrameworkMod.Settings?.debugPathfindingAndJob ?? false;

        [HarmonyPostfix]
        public static void Postfix(ref Thing __result, Job job, List<Thing> ingredients)
        {
            if (__result == null || ingredients.NullOrEmpty()) return;
            if (job?.bill == null) return;

            Bill bill = job.bill;
            RecipeDef recipe = job.RecipeDef;
            if (recipe == null) return;

            // 只处理产物需要 stuff 的配方
            if (!recipe.products.Any(x => x.thingDef.MadeFromStuff) &&
                (recipe.unfinishedThingDef == null || !recipe.unfinishedThingDef.MadeFromStuff))
                return;

            // 检查当前 dominant ingredient 是否是 FixedIngredient
            bool currentIsFixed = IsFixedIngredient(__result.def, recipe);
            if (!currentIsFixed) return; // 已经是非 FixedIngredient，不需要调整

            // 检查用户是否在 ingredientFilter 中禁用了当前 dominant ingredient
            if (bill.ingredientFilter.Allows(__result.def)) return; // 用户允许，不需要调整

            // 尝试从非 FixedIngredient 中选择 dominant ingredient
            var nonFixedStuff = ingredients
                .Where(x => x.def.IsStuff && !IsFixedIngredient(x.def, recipe))
                .ToList();

            if (nonFixedStuff.Any())
            {
                // 按 stackCount 加权随机选择（保持原版逻辑）
                Thing newDominant = nonFixedStuff.RandomElementByWeight(x => (float)x.stackCount);
                if (DebugLog)
                {
                    Log.Message($"【MLF】产物材质调整: {__result.def.defName}(FixedIng,用户禁用) → {newDominant.def.defName}(非FixedIng)");
                }
                __result = newDominant;
            }
        }

        private static bool IsFixedIngredient(ThingDef def, RecipeDef recipe)
        {
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                IngredientCount ing = recipe.ingredients[i];
                if (ing.IsFixedIngredient && ing.filter.Allows(def))
                    return true;
            }
            return false;
        }
    }
}
