using System.Collections;
using System.Collections.Generic;
using System;

namespace Kappa.utilities {
    class Pathfinder {

        /// <summary>
        /// Temporary method to generate a List of nodes arranged in a square grid way.
        /// Mostly for testing purposes though it can still be used as an example.
        /// Note that there's most definitely a cleaner way to do this.
        /// </summary>
        public static List<PathfinderNode> GenerateTestList(int size){
            List<PathfinderNode> grid = new List<PathfinderNode>();
            int i = size;
            while(i-- > 0) {
                int j = size;
                while(j-- > 0) {
                    PathfinderNode n = new PathfinderNode();
                    n.X = j;
                    n.Y = i;
                    n.W = 1;
                    n.Children = new List<PathfinderNode>();
                    grid.Add(n);
                }
            }
            i = size;
            while(i-- > 0) {
                int j = size;
                while(j-- > 0) {
                    PathfinderNode n = grid[i * size + j];
                    if(i * size + j - size >= 0 && grid[i * size + j - size] != null) n.Children.Add(grid[i * size + j - size]); // Add top child if it exists.
                    if(j != 0 && grid[i * size + j - 1] != null) n.Children.Add(grid[i * size + j - 1]); // Add left child if it exists.
                    if(i * size + j + size < size * size && grid[i * size + j + size] != null) n.Children.Add(grid[i * size + j + size]); // Add bottom child if it exists.
                    if(j != size - 1 && grid[i * size + j + 1] != null) n.Children.Add(grid[i * size + j + 1]); // Add right child if it exists.
                }
            }
            return grid;
        }

        public List<PathfinderNode> Grid;   // The Grid, a List of PathfinderNode objects.
        private SortByF Sort;               // Instance of the IComparer SortByF used to sort the nodes.

        /// <summary>
        /// Main node class to be used whenever a path needs to be found, the Pathfinder expects a list of those.
        /// F, G and P can be ignored, the pathfinder takes care of them.
        /// The List Children is necessary otherwise no search will be possible.
        /// The weight W is necessary, though it defaults at one.
        /// The X and Y coordinates can be omitted if the boolean useH of the "FindPath" call is set to false, which will ignore coordinates and use breadth-first search instead.
        /// </summary>
        public class PathfinderNode {
            public int F;                           // Node potential.
            public int G;                           // Distance from start node.
            public PathfinderNode P;                // Parent node.
            public List<PathfinderNode> Children;   // List of children Nodes;
            public ushort W = 1;                    // Node traversal weight.
            public int X;                           // Node's X coord.
            public int Y;                           // Node's Y coord.
        }

        /// <summary>
        /// Class used to sort the open list to make sure the first element is always the best node to check.
        /// </summary>
        private class SortByF: IComparer<PathfinderNode> {
            PathfinderNode N1, N2;
            public int Compare(PathfinderNode N1, PathfinderNode N2) {
                // If F is equal for both N1 and N2, compare G instead. If G is equal, disregard the tie and return the first Node.
                return (N1.F < N2.F) ? -1 : (N2.F < N1.F) ? 1 : (N1.G <= N2.G) ? -1 : 1; 
            }
        }

        /// <summary>
        /// Pathfinder Class constructor, saves the passed G grid in Grid and a new instance of SortByF in Sort.
        /// </summary>
        public Pathfinder(List<PathfinderNode> G) {
            Grid = G;               // Initialize the Grid.
            Sort = new SortByF();   // Initialize the IComparer.
        }

        /// <summary>
        /// Main A* pathfinding algorithm, expects a Start and End nodes located in the List Grid.
        /// The useH param toggle between using an heuristic or breadth-first search, using an heuristic requires nodes with X and Y params.
        /// </summary>
        public List<PathfinderNode> FindPath(PathfinderNode Start, PathfinderNode End, bool useH) {
            List<PathfinderNode> Open = new List<PathfinderNode>();
            List<PathfinderNode> Closed = new List<PathfinderNode>();
            Open.Add(Start);
            while(Open.Count > 0) {
                Open.Sort(Sort);
                PathfinderNode A = Open[0];
                if(A == End) break;
                Open.RemoveAt(0);
                byte i = (byte)A.Children.Count; // Not expecting the amount of children to be bigger than one byte.
                while(i-- > 0) {

                    PathfinderNode C = A.Children[i]; // A bit more memory to store the child by at least we're not gonna keep accessing the Children List.

                    // If we are not using an heuristic (AKA have nodes with X, Y coordinates), use only G (Basically Breadth-First).
                    int newF = (useH) ? C.G + GetH(C, End) : C.G;

                    // Skip node if it's in the closed list.
                    if(Closed.Contains(C)) {
                        continue;
                    }

                    // If true, this node has already been processed and this new path isn't shorter, abort this check.
                    if(Open.Contains(C) && C.F <= newF) {
                        continue;
                    }

                    // Otherwise, update the values and add it to the open list.
                    C.G = A.G + C.W;
                    C.F = newF;
                    C.P = A;
                    Open.Add(C);
                }
                Closed.Add(A);
            }

            // Pathfinding is done, return the path
            List<PathfinderNode> Path = new List<PathfinderNode>();
            PathfinderNode N = Open[0];
            while(N != null) {
                Path.Add(N);
                N = N.P;
            }
            Path.Reverse();
            return Path;
        }

        /// <summary>
        /// The heuristic method used if the useH param of FindPath is set to true.
        /// </summary>
        private int GetH(PathfinderNode Node, PathfinderNode TargetNode) {
            return (int) Math.Sqrt((TargetNode.X - Node.X) * (TargetNode.X - Node.X) + (TargetNode.Y - Node.Y) * (TargetNode.Y - Node.Y));
        }

        /// <summary>
        /// Returns the first node in Grid with the specified X and Y coordinates. (Should be only one if nobody fucked up.)
        /// Mostly for convenience by allowing to use it in the FindPath call instead of the actual nodes.
        /// Still just traversing the List, if the grid size is known nothing stops you from calculating to node's index directly.
        /// </summary>
        public PathfinderNode GetNodeAt(int X, int Y) {
            int i = Grid.Count;
            while(i-- > 0) {
                if(Grid[i].X == X && Grid[i].Y == Y) return Grid[i];
            }
            return null;
        }
    }
}
