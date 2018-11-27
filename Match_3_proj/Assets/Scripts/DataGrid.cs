using System;
using Logic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Random = UnityEngine.Random;

public class DataGrid : MonoBehaviour
{
    #region Fields
    public GridItem[,] Items;

    [SerializeField] [Range(7, 10)] private int Xsize;
    [SerializeField] [Range(10, 13)] private int Ysize;
    [SerializeField] private GameObject[] Presets;
    [SerializeField] private float ItemWidth = 1f;
    [SerializeField] private float ItemHeight = 1f;
    [SerializeField] private float MoveDuration = .25f;
    [SerializeField] private float DestroyDuration = .25f;
    [SerializeField] private float TimeForCreation = 1f;
    [SerializeField] private GameObject BackgroundTile;

    private readonly int ItemsForMatch = 3;
    private GridItem SelectedItem;
    private MatchGetter MatchGetter;
    private BalanceParser BalanceParser;
    #endregion

    #region Properties
    #endregion

    #region Methods
    private void Awake()
    {
        MatchGetter = new MatchGetter(ItemsForMatch, this);
        BalanceParser = new BalanceParser(Xsize, Ysize);
    }

    public int GetSizeX()
    {
        return Xsize;
    }

    public int GetSizeY()
    {
        return Ysize;
    }

    private void Start()
    {
        CreateBoard();
        CreateField();
        GridItem.OnMouseOverItemHandler += OnMouseOverGridItem;

        AutoUpdateField();
    }

    private void AutoUpdateField()
    {
        //TODO Make a cycle 
        // проверять пока есть автосовпадения
        IsThereAutoMatchItems();
        DOVirtual.DelayedCall(DestroyDuration, UpdateFieldAfterAutoMatch);
        DOVirtual.DelayedCall(DestroyDuration+TimeForCreation, CreateNewItems);
    }

    private void CreateNewItems()
    {
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
        {
            if (Items[x, y].Type == ItemType.Empty)
            {
                Destroy(Items[x, y].gameObject);
                InstantiateItem(x, y, GetRandomType());
            }
        }
    }

    private ItemType GetRandomType()
    {
        return (ItemType) Random.Range(1, Presets.Length);
    }

    private void CreateField()
    {
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
            Items[x, y] = InstantiateItem(x, y, BalanceParser.GetItemType(x, y));
    }

    //TODO Refactor VITALIY
    private void CreateBoard()
    {
        Items = new GridItem[Xsize, Ysize];
        GameObject BgTiles = GameObject.Find("BgTiles");
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
            CreateBoardBackground(BgTiles, x, y);
    }

    private void CreateBoardBackground(GameObject bgTiles, int x, int y)
    {
        Vector2 bgTilePos = new Vector2(x, y);
        GameObject bgTile = Instantiate(BackgroundTile, bgTilePos, Quaternion.identity);
        bgTile.transform.SetParent(bgTiles.transform);
        bgTile.name = string.Format("BgTile{0}_{1}", x.ToString(), y.ToString());
    }

    private GridItem InstantiateItem(int x, int y, ItemType type)
    {
        GameObject itemGameObject = FindCorrectPreset(type);

        Vector2 tilePos = new Vector2(x * ItemWidth, y * ItemHeight);
        GridItem item = Instantiate(itemGameObject, tilePos, Quaternion.identity).GetComponent<GridItem>();
        item.transform.SetParent(this.transform);
        item.Type = type;
        item.OnItemPositionChange(x, y);
        return item;
    }

    private GameObject FindCorrectPreset(ItemType type)
    {
        foreach (GameObject preset in Presets)
            if (preset.GetComponent<GridItem>().Type == type)
                return preset;

        return Presets[0];
    }

    private void OnMouseOverGridItem(GridItem item)
    {
        if (SelectedItem == null)
        {
            SelectedItem = item;
        }
        else
        {
            int xDiff = Mathf.Abs(item.X - SelectedItem.X);
            int yDiff = Mathf.Abs(item.Y - SelectedItem.Y);
            if (xDiff + yDiff == 1)
            {
                print("Try match");
                TryMatch(SelectedItem, item);
            }
            else
            {
                print("Error");
            }

            SelectedItem = null;
        }
    }

    private bool IsThereAutoMatchItems()
    {
        Debug.Log("AutoMatch");

        HashSet<GridItem> autoMatchSet = new HashSet<GridItem>();
        for (int x = 0; x < Xsize; x++)
        {
            for (int y = 0; y < Ysize; y++)
            {
                MatchInfo matchInfo = MatchGetter.GetAutoMatchInfo(Items[x, y]);
                List<GridItem> autoMatchInfoList = matchInfo.Matches;
                if (autoMatchInfoList != null)
                {
                    autoMatchSet.UnionWith(autoMatchInfoList);
                }
            }
        }

        DestroyItems(autoMatchSet);
        return autoMatchSet.Count > 0;
    }

    private void UpdateFieldAfterAutoMatch()
    {
        for (int x = 0; x < Xsize; x++)
        {
            List<int> figuresVerticalIndexes = new List<int>();
            List<int> emptiesVerticalIndexes = new List<int>();
            List<int> updatedVerticalIndexes = new List<int>();
            for (int y = 0; y < Ysize; y++)
            {
                if (Items[x, y].Type == ItemType.Empty)
                {
                    emptiesVerticalIndexes.Add(y);
                }
                else
                {
                    figuresVerticalIndexes.Add(y);
                }
            }

            //consolidation
            updatedVerticalIndexes.AddRange(figuresVerticalIndexes);
            updatedVerticalIndexes.AddRange(emptiesVerticalIndexes);
            UpdateView(x, updatedVerticalIndexes);
            UpdateData(x, updatedVerticalIndexes);
        }
    }

    private void UpdateData(int x, List<int> updatedVerticalIndexes)
    {
        GridItem[] tmp = new GridItem[Ysize];
        
        for (int y = 0; y < Ysize; y++)
        {
            int updatedIndex = updatedVerticalIndexes[y];
            tmp[y] = Items[x, updatedIndex];
            tmp[y].OnItemPositionChange(x, updatedIndex);
        }

        for (int y = 0; y < Ysize; y++)
        {
            Items[x, y] = tmp[y];
        }
    }

    private void UpdateView(int x, List<int> updatedVerticalIndexes)
    {
        for (int y = 0; y < Ysize; y++)
        {
            int updatedIndex = updatedVerticalIndexes[y];
            AutoChangeItemView(Items[x, updatedIndex], Items[x, y]);
        }
    }

    //TODO: Block mouse on move period
    private void TryMatch(GridItem selectedItem, GridItem item)
    {
        Debug.Log("Start swap");
        SwapItemsView(selectedItem, item);
        SwapItemsLogic(selectedItem, item);

        MatchInfo matchForSelectedItem = MatchGetter.GetMatchInfo(selectedItem);
        MatchInfo matchForItem = MatchGetter.GetMatchInfo(item);

        if (!matchForSelectedItem.IsValidMatch() && !matchForItem.IsValidMatch())
        {
            SwapItemsView(selectedItem, item, true);
            SwapItemsLogic(selectedItem, item);
        }

        if (matchForSelectedItem.IsValidMatch())
        {
            Debug.Log("Match For Selected Item");
            DestroyItems(matchForSelectedItem.Matches);

            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchA));
        }
        else if (matchForItem.IsValidMatch())
        {
            Debug.Log("Match For Item");
            DestroyItems(matchForItem.Matches);

            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchB));
        }
    }

    private void DestroyItems(ICollection<GridItem> matches)
    {
        foreach (GridItem item in matches)
        {
            item.transform.DOScale(Vector3.zero, DestroyDuration);
            Destroy(item.gameObject, DestroyDuration);
            DOVirtual.DelayedCall(DestroyDuration, CreateEmptyItem(item.X, item.Y));
        }
#if DEBUG
        PrintColors();
#endif
    }

    private TweenCallback CreateEmptyItem(int x, int y)
    {
        Items[x, y] = InstantiateItem(x, y, ItemType.Empty);
        return null;
    }

    private void SwapItemsView(GridItem selectedItem, GridItem item, bool isDelay = false)
    {
        Vector3 selectedItemPosition = selectedItem.transform.position;
        Vector3 itemPosition = item.transform.position;
        float delay = isDelay ? MoveDuration : 0;
        selectedItem.transform.DOMove(itemPosition, MoveDuration).SetDelay(delay);
        item.transform.DOMove(selectedItemPosition, MoveDuration).SetDelay(delay);
    }

    private void AutoChangeItemView(GridItem item, GridItem newItem)
    {
        Vector3 newPosition = newItem.transform.position;
        item.transform.DOMove(newPosition, TimeForCreation);
    }

    private void SwapItemsLogic(GridItem selectedItem, GridItem item)
    {
        //Swap in Items[,]
        GridItem tmp = Items[selectedItem.X, selectedItem.Y];
        Items[selectedItem.X, selectedItem.Y] = Items[item.X, item.Y];
        Items[item.X, item.Y] = tmp;

        //Swap in Scene
        int tmpX = selectedItem.X;
        int tmpY = selectedItem.Y;
        selectedItem.OnItemPositionChange(item.X, item.Y);
        item.OnItemPositionChange(tmpX, tmpY);
    }
    #endregion
    #region Auxilary
    [ContextMenu("Print grid Ints")]
    private void PrintInts()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\n");
        for (int y = Ysize - 1; y > -1; y--)
        {
            for (int x = 0; x < Xsize; x++)
            {
                sb.Append((int) Items[x, y].Type);
                sb.Append(", ");
            }

            sb.Append("\n");
        }

        Debug.Log(sb);
    }

    [ContextMenu("Print grid Colors")]
    private void PrintColors()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\n");
        for (int y = Ysize - 1; y > -1; y--)
        {
            for (int x = 0; x < Xsize; x++)
            {
                sb.Append(Items[x, y].Type);
                sb.Append(", ");
            }

            sb.Append("\n");
        }

        Debug.Log(sb);
    }
    #endregion
}