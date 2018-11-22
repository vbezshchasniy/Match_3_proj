using System.Collections.Generic;

namespace Logic
{
    public class MatchInfo
    {
        public List<GridItem> Matches;
        public int MatchStartX;
        public int MatchEndX;
        public int MatchStartY;
        public int MatchEndY;

        public bool IsValidMatch()
        {
            return Matches != null;
        }
    }
}