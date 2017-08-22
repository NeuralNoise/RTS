public static partial class Pathfinder {
    private enum PathfinderASNodeState : byte {

        NONE = 0,

        NOT_TESTED = 1,
        OPEN = 2,
        CLOSED = 3

    }
}