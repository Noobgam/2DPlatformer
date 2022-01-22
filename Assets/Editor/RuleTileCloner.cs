using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEditor;
using System.Linq;
 
public class RuleTileCloner : EditorWindow
{
    private RuleTile referenceTile;
    private Texture2D sprite;
 
    [MenuItem("Tools/RuleTile Cloner")]
    public static void ShowTool()
    {
        GetWindow<RuleTileCloner>().Show();
    }
 
    public void OnGUI()
    {
        referenceTile = EditorGUILayout.ObjectField(referenceTile, typeof(RuleTile), false) as RuleTile;
        sprite = EditorGUILayout.ObjectField(sprite, typeof(Texture2D), false) as Texture2D;
 
        if (GUILayout.Button("Clone"))
        {
            string origPath = AssetDatabase.GetAssetPath(referenceTile);
            string spritePath = AssetDatabase.GetAssetPath(sprite);
            string targetPath = $"{spritePath.Substring(0, spritePath.LastIndexOf('/'))}/{sprite.name}_RuleTile.asset";
 
            AssetDatabase.CopyAsset(origPath, targetPath);
            RuleTile oldTile = AssetDatabase.LoadAssetAtPath<RuleTile>(origPath);
            RuleTile newTile = AssetDatabase.LoadAssetAtPath<RuleTile>(targetPath);
 
            if (newTile != null)
            {
                CloneBySpriteIndex(spritePath, newTile, oldTile);
            }
 
            AssetDatabase.SaveAssets();
        }
    }
 
    private void CloneBySpriteIndex(string spritePath, RuleTile newTile, RuleTile refTile)
    {
        // First load in the data for the reference tile.
        Texture2D refTex = newTile.m_TilingRules[0].m_Sprites[0].texture;
        string refPath = AssetDatabase.GetAssetPath(refTex);
        Sprite[] refSprites = AssetDatabase.LoadAllAssetsAtPath(refPath).OfType<Sprite>().ToArray();
 
        // New rule tile created, now to swap out the sprites.
        Sprite[] newSprites = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().ToArray();
        var diffs = new int[15,15];
        for (int i = 0; i < 15; ++i) {
            RuleTile.TilingRule rule = newTile.m_TilingRules[i];
            for (int j = 0; j < 15; ++j) {
                diffs[i, j] = CalcDiff(newSprites[i].name, rule.m_Sprites[j].name);
            }
        }

        var tmp = new int[15];
        
        for (int i = 0; i < newTile.m_TilingRules.Count; i++)
        {
            RuleTile.TilingRule rule = newTile.m_TilingRules[i];
 
            for (int j = 0; j < rule.m_Sprites.Length; j++)
            {
                int refIndex = FindIndex(refSprites, rule.m_Sprites[j]);
                rule.m_Sprites[j] = newSprites[refIndex];
            }
        }
 
        if (referenceTile.m_DefaultSprite != null)
        {
            newTile.m_DefaultSprite = newSprites[15];
        }
    }

    private int CalcDiff(string a, string b) {
        var dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i < a.Length + 1; ++i) {
            for (int j = 0; j <= b.Length; ++j) {
                dp[i, j] = 0;
                if (i > 0) {
                    dp[i, j] = Math.Max(dp[i, j], dp[i - 1, j]);
                }
                if (j > 0) {
                    dp[i, j] = Math.Max(dp[i, j], dp[i, j - 1]);
                }
                if (i > 0 && j > 0 && a[i - 1] == b[j - 1]) {
                    dp[i, j] = Math.Max(dp[i, j], dp[i - 1, j - 1] + 1);
                }
            }
        }

        return dp[a.Length, b.Length];
    }

    private int FindIndex(Sprite[] spriteArray, Sprite sprite)
    {
        for (int i = 0; i < spriteArray.Length; i++)
        {
            if (spriteArray[i] == sprite)
                return i;
        }
 
        return -1;
    }
}