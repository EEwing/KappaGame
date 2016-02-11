using System.Collections;
using System.Collections.Generic;

namespace Kappa.utilities {
    class Pathfinder {

        private SortedSet<PathfinderNode> Grid; // The Grid, a sorted set of PathfinderNode objects.

        public class PathfinderNode {
            public ushort F;                    // Potential.
            public ushort G;                    // Distance from start node.
            public List<PathfinderNode> P;      // List of Parent nodes.
            public ushort W;                    // Node traversal weight.
        }

        private class SortByF: IComparer<PathfinderNode> {
            PathfinderNode N1, N2;
            public int Compare(PathfinderNode N1, PathfinderNode N2) {
                // If F is equal for both N1 and N2, compare G instead. If G is equal, disregard the tie and return the first Node.
                return (N1.F < N2.F) ? -1 : (N2.F < N1.F) ? 1 : (N1.G <= N2.G) ? -1 : 1; 
            }
        }

        public Pathfinder(SortedSet<PathfinderNode> Grid, PathfinderNode Start, PathfinderNode End) {
            Grid = new SortedSet<PathfinderNode>(new SortByF()); // Initialize the Grid.
        }

        // Temporary incomplete FindPath main function
        public List<PathfinderNode> FindPath() {
            return new List<PathfinderNode>();
        }
    }
}
