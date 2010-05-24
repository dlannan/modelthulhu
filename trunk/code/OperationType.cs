namespace Modelthulhu
{
    /* 
     * Enumeration for the various kinds of CSG operation
     * In the names, "First" and "Second" refer to the two objects being Boolean'd together
     * The items starting with "Keep" are bitfields controlling which polygons are kept and which are eliminated after edge cuts are made
     *
     * Example:
     * If the KeepSecondBehindFirst bitfield is set, then polygons from the second object which are "behind" the surface of the first object are kept in the result
     * Otherwise, this set of polygons is discarded
     */
    public enum OperationType : int
    {
        KeepFirstInsideSecond =     (1 << 1),
        KeepFirstOutsideSecond =    (1 << 2),
        KeepSecondInsideFirst =     (1 << 3),
        KeepSecondOutsideFirst =    (1 << 4),

        Union = KeepFirstOutsideSecond | KeepSecondOutsideFirst,
        Intersect = KeepFirstInsideSecond | KeepSecondInsideFirst,
        Subtract = KeepFirstOutsideSecond | KeepSecondInsideFirst,
        DoNothing = KeepFirstInsideSecond | KeepFirstOutsideSecond | KeepSecondInsideFirst | KeepSecondOutsideFirst
    }
}