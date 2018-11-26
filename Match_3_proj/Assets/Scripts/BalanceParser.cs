using System.Globalization;
using System.IO;
using UnityEngine;

public class BalanceParser
{
    private const string Path = "Assets/Resources/Balance/balance.txt";
    private readonly int[,] Items;
    private readonly char[] Delimiters = {','};
    
    private string Balance = string.Empty;

    public BalanceParser(int sizeX, int sizeY)
    {
        OpenBalanceFile();
        Items = ParseBalanceFile(sizeX, sizeY);
    }

    private void OpenBalanceFile()
    {
#if UNITY_EDITOR
        Balance = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(Path).ToString();
#else
	    TextAsset textAsset = (TextAsset)Resources.Load ("balance.txt", typeof(TextAsset));
	    Balance = textAsset.ToString ();
#endif
    }

    private int[,] ParseBalanceFile(int sizeX, int sizeY)
    {
        int[,] tmp = new int[sizeX, sizeY];
        string[] parsedBalance = Balance.Split(Delimiters);
        int iterator = 0;
        for (int y = sizeY - 1; y > -1; y--)
        for (int x = 0; x < sizeX; x++)
        {
            tmp[x, y] = int.Parse(parsedBalance[iterator], NumberStyles.Integer);
            iterator++;
        }

        return tmp;
    }

    public int GetItemIndex(int x, int y)
    {
        return Items[x, y];
    }
    
    public ItemType GetItemType(int x, int y)
    {
        return (ItemType)Items[x, y];
    }
}