using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DataGrid : MonoBehaviour
{
    #region Fields

    public GridItem[,] Items;
    
    [SerializeField][Range(7, 10)] private int Xsize;
    [SerializeField][Range(10, 13)] private int Ysize;
    [SerializeField] private GameObject[] Presets;
    [SerializeField] private float ItemWidth = 1f;
    [SerializeField] private float ItemHeight = 1f;
    [SerializeField] private float MoveDuration = .15f;
    [SerializeField] private float DestroyDuration = .15f;
    [SerializeField] private GameObject BackgroundTile;

    private readonly int ItemsForMatch = 3;
    private GridItem SelectedItem;
    private readonly Logic.MatchGetter MatchGetter;

    #endregion

    #region Properties

    #endregion

     #region Methods

    public DataGrid()
    {
        MatchGetter = new Logic.MatchGetter(ItemsForMatch, this);
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
        StartCoroutine(CheckField());
        GridItem.OnMouseOverItemHandler += OnMouseOverGridItem;
    }

    private void CreateField()
    {
        Items = new GridItem[Xsize, Ysize];

        for (int x = 0; x < Xsize; x++)
            for (int y = 0; y < Ysize; y++)
                Items[x, y] = InstantiateItem(x, y);
    }
    
    private void CreateBoard()
    {
        Items = new GridItem[Xsize, Ysize];

        for (int x = 0; x < Xsize; x++)
            for (int y = 0; y < Ysize; y++)
                CreateBoardBackground(x, y);
    }
    
    private void CreateBoardBackground(int x, int y)
    {
        Vector2 bgTilePos = new Vector2(x, y);
        GameObject bgTile = Instantiate(BackgroundTile, bgTilePos, Quaternion.identity);
        bgTile.transform.SetParent(this.transform);
        bgTile.name = string.Format("BgTile{0}_{1}", x.ToString(), y.ToString());
    }

    private GridItem InstantiateItem(int x, int y)
    {
        GameObject itemGameObject = GetRandomItemFromPreset(Presets.Length);
        Vector2 tilePos = new Vector2(x, y);
        GridItem item = Instantiate(itemGameObject, tilePos, Quaternion.identity).GetComponent<GridItem>();
        item.transform.SetParent(this.transform);
        item.name = string.Format("Tile{0}_{1}", x.ToString(), y.ToString());
        item.OnItemPositionChange(x,y);
        return item;
    }

    private GameObject GetRandomItemFromPreset(int presetsLength)
    {
        int randomIndex = Random.Range(0, presetsLength);
        return Presets[randomIndex];
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
                StartCoroutine(TryMatch(SelectedItem, item, false));
            }
            else
            {
                print("Error");
            }
            SelectedItem = null;
        }
    }

    private IEnumerator CheckField()
    {
        for (int x = 0; x < Xsize; x++)
        {
            for (int y = 0; y < Ysize; y++)
            {
                
                int left = y + 1;
                if (left < Ysize && Items[x, left] != null)
                {
                    GridItem selectedItem = Items[x, y];
                    GridItem item = Items[x, left];
                    yield return StartCoroutine(TryMatch(selectedItem, item));
                }
                int up = x + 1;
                if (up < Xsize && Items[up, y] != null)
                {
                    GridItem selectedItem = Items[x, y];
                    GridItem item = Items[up, y];
                    yield return StartCoroutine(TryMatch(selectedItem, item));
                }
                
            }
        }
    }

    //TODO: Block mouse on move period
    //TODO: Create new TryMatch for auto match
    private IEnumerator TryMatch(GridItem selectedItem, GridItem item, bool isAuto = true)
    {
        if (!isAuto)
        {
            yield return StartCoroutine(Swap(selectedItem, item));
            Debug.Log("Start swap");
        }

        Logic.MatchInfo matchForSelectedItem = MatchGetter.GetMatchInfo(selectedItem);
        Logic.MatchInfo matchForItem = MatchGetter.GetMatchInfo(item);
        
        if (!matchForSelectedItem.IsValidMatch() && !matchForItem.IsValidMatch() && !isAuto)
        {
            yield return StartCoroutine(Swap(selectedItem, item));
        }
        if (matchForSelectedItem.IsValidMatch())
        {
            Debug.Log("Match For Selected Item");
            yield return StartCoroutine(DestroyItems(matchForSelectedItem.Matches));
            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchA));

        }
        else if (matchForItem.IsValidMatch())
        {
            Debug.Log("Match For Item");
            yield return StartCoroutine(DestroyItems(matchForItem.Matches));
            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchB));
        }
    }

    private IEnumerator DestroyItems(List<GridItem> matches)
    {
        foreach (GridItem item in matches)
        {
//            ParticleSystem.MainModule destroyEffectMain = item.DestroyEffect.main;
//            destroyEffectMain.startColor = new ParticleSystem.MinMaxGradient(item.GetComponent<SpriteRenderer>().color);
            yield return StartCoroutine(item.transform.Scale(Vector3.zero, DestroyDuration));
//            item.DestroyEffect.Play();
//            yield return new WaitForSeconds(item.DestroyEffect.main.duration);
            Destroy(item.gameObject);
        }
    }
    
    private IEnumerator Swap(GridItem selectedItem, GridItem item)
    {
        Vector3 selectedItemPosition = selectedItem.transform.position;
        Vector3 itemPosition = item.transform.position;
        StartCoroutine(selectedItem.transform.Move(itemPosition, MoveDuration));
        StartCoroutine(item.transform.Move(selectedItemPosition, MoveDuration));
        yield return new WaitForSeconds(MoveDuration);
        SwapItemsIndex(selectedItem, item);
    }

    private void SwapItemsIndex(GridItem selectedItem, GridItem item)
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
}