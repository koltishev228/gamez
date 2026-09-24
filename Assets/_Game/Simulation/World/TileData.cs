using System;

namespace Zomboid.Simulation.World
{
    [Flags]
    public enum TileFlags : ushort
    {
        None = 0,
        Walkable = 1 << 0,
        WallNorth = 1 << 1,
        WallEast = 1 << 2,
        WallSouth = 1 << 3,
        WallWest = 1 << 4,
        Floor = 1 << 5,
        DoorNorth = 1 << 6,
        DoorEast = 1 << 7,
        DoorSouth = 1 << 8,
        DoorWest = 1 << 9,
        WindowNorth = 1 << 10,
        WindowEast = 1 << 11,
        WindowSouth = 1 << 12,
        WindowWest = 1 << 13
    }

    [Flags] 
    public enum EdgeState : byte 
    { 
        None = 0, 
        Open = 1, 
        Curtain = 2, 
        Boarded = 4, 
        Broken = 8 
    }
    
    public enum Dir : byte { North, East, South, West }

    [Serializable]
    public struct TileData
    {
        public TileFlags Flags;
        public ushort RoomID;
        public EdgeState EdgeNorth;
        public EdgeState EdgeEast;
        public EdgeState EdgeSouth;
        public EdgeState EdgeWest;

        public bool Has(TileFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public void SetFlag(TileFlags flag, bool value)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }

        public EdgeState GetEdge(Dir d)
        {
            switch (d)
            {
                case Dir.North: return EdgeNorth;
                case Dir.East: return EdgeEast;
                case Dir.South: return EdgeSouth;
                case Dir.West: return EdgeWest;
                default: return EdgeState.None;
            }
        }
    }

    public static class Sight 
    {
        public static bool BlocksSight(in TileData t, Dir d) 
        {
            if (t.Has(WallFlag(d))) return true;
            
            var s = t.GetEdge(d);
            
            if (t.Has(DoorFlag(d)))   
                return (s & (EdgeState.Open | EdgeState.Broken)) == 0;
                
            if (t.Has(WindowFlag(d))) 
                return (s & (EdgeState.Curtain | EdgeState.Boarded)) != 0;
                
            return false;
        }

        private static TileFlags WallFlag(Dir d) => d switch { Dir.North => TileFlags.WallNorth, Dir.East => TileFlags.WallEast, Dir.South => TileFlags.WallSouth, _ => TileFlags.WallWest };
        private static TileFlags DoorFlag(Dir d) => d switch { Dir.North => TileFlags.DoorNorth, Dir.East => TileFlags.DoorEast, Dir.South => TileFlags.DoorSouth, _ => TileFlags.DoorWest };
        private static TileFlags WindowFlag(Dir d) => d switch { Dir.North => TileFlags.WindowNorth, Dir.East => TileFlags.WindowEast, Dir.South => TileFlags.WindowSouth, _ => TileFlags.WindowWest };
    }
}
