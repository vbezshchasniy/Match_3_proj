using System.Collections.Generic;

namespace Logic
{
    public class MatchInfo
    {
        public List<GridItem> Matches;

        public bool IsValidMatch()
        {
            return Matches != null;
        }
    }
}