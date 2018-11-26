using System.Collections.Generic;

namespace Logic
{
    public class UpdateInfo
    {
        public class GridItemPair
        {
            public GridItemPair(GridItem target, GridItem destination)
            {
                target = target;
                Destination = destination;
            }

            public GridItem target { get; private set; }
            public GridItem Destination { get; private set; }
        }
        
        public List<GridItemPair> Transformations;

        public bool IsValidBubble()
        {
            return Transformations != null;
        }
    }
}