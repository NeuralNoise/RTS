public static partial class Pathfinder {
    private enum ASNodeState : byte {

        NONE = 0,

        NOT_TESTED = 1,
        OPEN = 2,
        CLOSED = 3

    }
}