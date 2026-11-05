using System.Collections.Generic;
using UnityEngine;

namespace Code.Craftings
{
    /// <summary>
    /// 제작대 하나가 보여 줄 레시피 목록입니다. 대장간 · 연금대처럼 제작대마다 다른 목록을 줄 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeList", menuName = "SO/Crafting/Recipe list", order = 1)]
    public class RecipeListSO : ScriptableObject
    {
        [field: SerializeField] public string ListName { get; private set; }
        [SerializeField] private List<RecipeSO> recipes = new();

        /// <summary> 레시피 목록입니다. </summary>
        public IReadOnlyList<RecipeSO> Recipes => recipes;
    }
}
