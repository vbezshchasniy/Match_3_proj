using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Logic
{
    public class MatchGetter
    {
        private readonly int _itemsForMatch;
        private readonly DataGrid _data;

        public MatchGetter(int itemsForMatch, DataGrid data)
        {
            _itemsForMatch = itemsForMatch;
            _data = data;
        }

        public MatchInfo GetMatchInfo(GridItem item)
        {
            MatchInfo matchInfo = new MatchInfo {Matches = null};

            List<GridItem> horizontalItems = GetHorizontalSimilarItems(item);
            List<GridItem> verticalItems = GetVerticalSimilarItems(item);

            if (horizontalItems.Count >= _itemsForMatch) // && match.Count > match.Count)
            {
                matchInfo.MatchStartX = GetLowestX(horizontalItems);
                matchInfo.MatchEndX = GetHighestX(horizontalItems);
                matchInfo.MatchStartY = horizontalItems.First().Y;
                matchInfo.MatchEndY = horizontalItems.First().Y;

                matchInfo.Matches = horizontalItems;
                Debug.Log("Horizontal match");
            }
            else if (verticalItems.Count >= _itemsForMatch)
            {
                matchInfo.MatchStartY = GetLowestY(verticalItems);
                matchInfo.MatchEndY = GetHighestY(verticalItems);
                matchInfo.MatchStartX = verticalItems.First().X;
                matchInfo.MatchEndX = verticalItems.First().X;

                matchInfo.Matches = verticalItems;
                Debug.Log("Vertical match");
            }

            return matchInfo;
        }

        public MatchInfo GetAutoMatchInfo(GridItem item)
        {
            MatchInfo matchInfo = new MatchInfo {Matches = null};

            List<GridItem> horizontalItems = GetHorizontalSimilarItems(item);
            List<GridItem> verticalItems = GetVerticalSimilarItems(item);

            if (horizontalItems.Count >= _itemsForMatch)
            {
                matchInfo.Matches = new List<GridItem>(horizontalItems);
                Debug.Log("Horizontal match");
            }
            else if (verticalItems.Count >= _itemsForMatch)
            {
                if (matchInfo.Matches == null)
                    matchInfo.Matches = new List<GridItem>(verticalItems);
                else
                    matchInfo.Matches.AddRange(verticalItems);
                Debug.Log("Vertical match");
            }

            return matchInfo;
        }

        private bool CheckHorizontalMatchBeforeBound(int index, GridItem item, int bound)
        {
            bool isMatch = false;

            if (index > bound && _data.Items[index, item.Y] != null)
                isMatch = _data.Items[index, item.Y].Type == item.Type;

            return isMatch;
        }

        private bool CheckHorizontalMatchToBound(int index, GridItem item, int bound)
        {
            bool isMatch = false;

            if (index < bound && _data.Items[index, item.Y] != null)
                isMatch = _data.Items[index, item.Y].Type == item.Type;

            return isMatch;
        }

        private bool CheckVerticalMatchBeforeBound(int index, GridItem item, int bound)
        {
            bool isMatch = false;

            if (index > bound && _data.Items[item.X, index] != null)
                isMatch = _data.Items[item.X, index].Type == item.Type;

            return isMatch;
        }

        private bool CheckVerticalMatchToBound(int index, GridItem item, int bound)
        {
            bool isMatch = false;

            if (index < bound && _data.Items[item.X, index] != null)
                isMatch = _data.Items[item.X, index].Type == item.Type;

            return isMatch;
        }

        private List<GridItem> GetHorizontalSimilarItems(GridItem item)
        {
            List<GridItem> horizontalItems = new List<GridItem> {item};
            int left = item.X - 1;
            int right = item.X + 1;
            int leftBound = -1;
            int rightBound = _data.GetSizeX();

            while (CheckHorizontalMatchBeforeBound(left, item, leftBound))
            {
                horizontalItems.Add(_data.Items[left, item.Y]);
                left--;
            }

            while (CheckHorizontalMatchToBound(right, item, rightBound))
            {
                horizontalItems.Add(_data.Items[right, item.Y]);
                right++;
            }

            return horizontalItems;
        }

        private List<GridItem> GetVerticalSimilarItems(GridItem item)
        {
            List<GridItem> verticalItems = new List<GridItem> {item};
            int up = item.Y - 1;
            int down = item.Y + 1;
            int upperBound = -1;
            int lowerBound = _data.GetSizeY();

            while (CheckVerticalMatchBeforeBound(up, item, upperBound))
            {
                verticalItems.Add(_data.Items[item.X, up]);
                up--;
            }

            while (CheckVerticalMatchToBound(down, item, lowerBound))
            {
                verticalItems.Add(_data.Items[item.X, down]);
                down++;
            }

            return verticalItems;
        }

        private int GetLowestX(List<GridItem> items)
        {
            int[] index = new int[items.Count];
            for (int i = 0; i < index.Length; i++)
                index[i] = items[i].X;
            return index.Min();
        }

        private int GetHighestX(List<GridItem> items)
        {
            int[] index = new int[items.Count];
            for (int i = 0; i < index.Length; i++)
                index[i] = items[i].X;
            return index.Max();
        }

        private int GetLowestY(List<GridItem> items)
        {
            int[] index = new int[items.Count];
            for (int i = 0; i < index.Length; i++)
                index[i] = items[i].Y;
            return index.Min();
        }

        private int GetHighestY(List<GridItem> items)
        {
            int[] index = new int[items.Count];
            for (int i = 0; i < index.Length; i++)
                index[i] = items[i].Y;
            return index.Max();
        }
    }
}