using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.System.Scripts;
using Assets.UI.Colors;
using UnityEditor;
using TMPro;
using UnityEngine;

namespace UIKit
{
	public class FontIconLibrary : SingletonResourcesAsset<FontIconLibrary> 
	{
		[Serializable]
		public class Icon
		{
			[SerializeField/*, HideLabel*/]
			private string _code;
			public string Code => _code;
			
			[SerializeField/*, ValueDropdown("GetSpriteAssetList", AppendNextDrawer = true), HideLabel*/]
			private TMP_SpriteAsset _spriteAsset;
			public TMP_SpriteAsset SpriteAsset => _spriteAsset;

			[SerializeField/*, ValueDropdown("GetSpritesFromAsset", AppendNextDrawer = true), HideLabel*/]
			private int _id;
			public int Id => _id;
			
			[SerializeField]
			private ColorLibraryItem _color;
			public ColorLibraryItem Color => _color;
			/*
#if UNITY_EDITOR
			private IEnumerable GetSpriteAssetList()
			{
				return UnityEditor.AssetDatabase.FindAssets("t:TMP_SpriteAsset")
				                  .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
				                  .Select(x => new ValueDropdownItem(Path.GetFileNameWithoutExtension(x), UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(x)));
			}
			
			private IEnumerable GetSpritesFromAsset()
			{
				if (_spriteAsset == null)
					return null;

				return _spriteAsset.spriteInfoList.Select(x => new ValueDropdownItem($"{x.id}: {x.name}", x.id));
			}
#endif
*/
		}
		
		[SerializeField/*, ListDrawerSettings(OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement")*/]
		private List<Icon> _iconList = new List<Icon>();

        public static bool FindIconByCode(string code, out Icon icon)
        {
            int count = Instance._iconList.Count;
            for (var i = 0; i < count; i++)
            {
                icon = Instance._iconList[i];
                if (icon.SpriteAsset != null && icon.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            icon = null;
            return false;
        }
        
        public static string GetTag(string code)
		{
            return GetTag(code, Color.white);
		}
		
        public static string GetTag(string code, Color color)
        {
            if (FindIconByCode(code, out var icon))
                return $"<sprite=\"{icon.SpriteAsset.name}\" index=\"{icon.Id}\" color=#{ColorUtility.ToHtmlStringRGBA(icon.Color)}>";
            
            var arr = code.Split('/');
            if (arr.Length == 2)
                return $"<sprite{(string.IsNullOrEmpty(arr[0]) ? "" : $"=\"{arr[0]}\"")} name=\"{arr[1]}\" color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
            
            if (arr.Length == 1)
                return $"<sprite name=\"{arr[0]}\" color=#{ColorUtility.ToHtmlStringRGBA(color)}>";

            return "";
        }
		
		/*
#if UNITY_EDITOR
		
		private void BeginDrawListElement(int index)
		{
			GUILayout.BeginHorizontal();

			if (_iconList[index].SpriteAsset != null)
			{
				TMP_Sprite sprite = _iconList[index].SpriteAsset.spriteInfoList.FirstOrDefault(e => e.id == _iconList[index].Id);
				if (sprite != null)
				{
					Texture texture = _iconList[index].SpriteAsset.spriteSheet;
					Vector2 fullSize = new Vector2(texture.width, texture.height);
					float size = (EditorGUIUtility.singleLineHeight + 2) * 4;
					Rect position = GUILayoutUtility.GetRect(size, size);
					Rect coords = new Rect(
						sprite.x / fullSize.x, 
						sprite.y / fullSize.y, 
						sprite.width / fullSize.x,
						sprite.height / fullSize.y
					);
					GUI.DrawTextureWithTexCoords(position, texture, coords);
				}
			}
			
			GUILayout.BeginVertical();
		} 
		
		private void EndDrawListElement(int index)
		{
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
			
			
			//[UnityEditor.MenuItem("Assets/Create/UI/FontIconLibrary")]
			//public static void CreateCommandRoseElementsLibrary()
			//{
			//	ScriptableObjectAsset.CreateAsset<FontIconLibrary>();
			//}
			
#endif
		*/
	}
}

