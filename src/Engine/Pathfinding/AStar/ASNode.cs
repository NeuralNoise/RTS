using System;

public static partial class Pathfinder {
    private unsafe struct ASNode {
        public ASNode* Parent;
        public ASNodeState State;

        public int X;
        public int Y;

        public bool Initialized;

        /// <summary>
        /// Node distance from start
        /// </summary>
        public float G;

        /// <summary>
        /// Node distance to end
        /// </summary>
        public float H;
    }
}