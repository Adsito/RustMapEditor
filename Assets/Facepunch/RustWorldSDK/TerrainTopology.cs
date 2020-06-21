using System.Collections.Generic;

public class TerrainTopology
{
    public enum Enum
    {
        Field = 1 << 0,
        Cliff = 1 << 1,
        Summit = 1 << 2,
        Beachside = 1 << 3,
        Beach = 1 << 4,
        Forest = 1 << 5,
        Forestside = 1 << 6,
        Ocean = 1 << 7,
        Oceanside = 1 << 8,
        Decor = 1 << 9,
        Monument = 1 << 10,
        Road = 1 << 11,
        Roadside = 1 << 12,
        Swamp = 1 << 13,
        River = 1 << 14,
        Riverside = 1 << 15,
        Lake = 1 << 16,
        Lakeside = 1 << 17,
        Offshore = 1 << 18,
        Powerline = 1 << 19,
        Runway = 1 << 20,
        Building = 1 << 21,
        Cliffside = 1 << 22,
        Mountain = 1 << 23,
        Clutter = 1 << 24,
        Alt = 1 << 25,
        Tier0 = 1 << 26,
        Tier1 = 1 << 27,
        Tier2 = 1 << 28,
        Mainland = 1 << 29,
        Hilltop = 1 << 30,
    }

    public const int COUNT = 31;
    public const int EVERYTHING = ~0;
    public const int NOTHING = 0;

    public const int FIELD = (int)Enum.Field;
    public const int CLIFF = (int)Enum.Cliff;
    public const int SUMMIT = (int)Enum.Summit;
    public const int BEACHSIDE = (int)Enum.Beachside;
    public const int BEACH = (int)Enum.Beach;
    public const int FOREST = (int)Enum.Forest;
    public const int FORESTSIDE = (int)Enum.Forestside;
    public const int OCEAN = (int)Enum.Ocean;
    public const int OCEANSIDE = (int)Enum.Oceanside;
    public const int DECOR = (int)Enum.Decor;
    public const int MONUMENT = (int)Enum.Monument;
    public const int ROAD = (int)Enum.Road;
    public const int ROADSIDE = (int)Enum.Roadside;
    public const int SWAMP = (int)Enum.Swamp;
    public const int RIVER = (int)Enum.River;
    public const int RIVERSIDE = (int)Enum.Riverside;
    public const int LAKE = (int)Enum.Lake;
    public const int LAKESIDE = (int)Enum.Lakeside;
    public const int OFFSHORE = (int)Enum.Offshore;
    public const int POWERLINE = (int)Enum.Powerline;
    public const int RUNWAY = (int)Enum.Runway;
    public const int BUILDING = (int)Enum.Building;
    public const int CLIFFSIDE = (int)Enum.Cliffside;
    public const int MOUNTAIN = (int)Enum.Mountain;
    public const int CLUTTER = (int)Enum.Clutter;
    public const int ALT = (int)Enum.Alt;
    public const int TIER0 = (int)Enum.Tier0;
    public const int TIER1 = (int)Enum.Tier1;
    public const int TIER2 = (int)Enum.Tier2;
    public const int MAINLAND = (int)Enum.Mainland;
    public const int HILLTOP = (int)Enum.Hilltop;

    public const int FIELD_IDX = 0;
    public const int CLIFF_IDX = 1;
    public const int SUMMIT_IDX = 2;
    public const int BEACHSIDE_IDX = 3;
    public const int BEACH_IDX = 4;
    public const int FOREST_IDX = 5;
    public const int FORESTSIDE_IDX = 6;
    public const int OCEAN_IDX = 7;
    public const int OCEANSIDE_IDX = 8;
    public const int DECOR_IDX = 9;
    public const int MONUMENT_IDX = 10;
    public const int ROAD_IDX = 11;
    public const int ROADSIDE_IDX = 12;
    public const int SWAMP_IDX = 13;
    public const int RIVER_IDX = 14;
    public const int RIVERSIDE_IDX = 15;
    public const int LAKE_IDX = 16;
    public const int LAKESIDE_IDX = 17;
    public const int OFFSHORE_IDX = 18;
    public const int POWERLINE_IDX = 19;
    public const int RUNWAY_IDX = 20;
    public const int BUILDING_IDX = 21;
    public const int CLIFFSIDE_IDX = 22;
    public const int MOUNTAIN_IDX = 23;
    public const int CLUTTER_IDX = 24;
    public const int ALT_IDX = 25;
    public const int TIER0_IDX = 26;
    public const int TIER1_IDX = 27;
    public const int TIER2_IDX = 28;
    public const int MAINLAND_IDX = 29;
    public const int HILLTOP_IDX = 30;
     
    public const int WATER = OCEAN | RIVER | LAKE;
    public const int WATERSIDE = OCEANSIDE | RIVERSIDE | LAKESIDE;
    public const int SAND = OCEAN | OCEANSIDE | LAKE | LAKESIDE | BEACH | BEACHSIDE;

    private static Dictionary<int, int> type2index = new Dictionary<int, int>() {
        { FIELD       , FIELD_IDX      },
        { CLIFF       , CLIFF_IDX      },
        { SUMMIT      , SUMMIT_IDX     },
        { BEACHSIDE   , BEACHSIDE_IDX  },
        { BEACH       , BEACH_IDX      },
        { FOREST      , FOREST_IDX     },
        { FORESTSIDE  , FORESTSIDE_IDX },
        { OCEAN       , OCEAN_IDX      },
        { OCEANSIDE   , OCEANSIDE_IDX  },
        { DECOR       , DECOR_IDX      },
        { MONUMENT    , MONUMENT_IDX   },
        { ROAD        , ROAD_IDX       },
        { ROADSIDE    , ROADSIDE_IDX   },
        { SWAMP       , SWAMP_IDX      },
        { RIVER       , RIVER_IDX      },
        { RIVERSIDE   , RIVERSIDE_IDX  },
        { LAKE        , LAKE_IDX       },
        { LAKESIDE    , LAKESIDE_IDX   },
        { OFFSHORE    , OFFSHORE_IDX   },
        { POWERLINE   , POWERLINE_IDX  },
        { RUNWAY      , RUNWAY_IDX     },
        { BUILDING    , BUILDING_IDX   },
        { CLIFFSIDE   , CLIFFSIDE_IDX  },
        { MOUNTAIN    , MOUNTAIN_IDX   },
        { CLUTTER     , CLUTTER_IDX    },
        { ALT         , ALT_IDX        },
        { TIER0       , TIER0_IDX      },
        { TIER1       , TIER1_IDX      },
        { TIER2       , TIER2_IDX      },
        { MAINLAND    , MAINLAND_IDX   },
        { HILLTOP     , HILLTOP_IDX    },
    };
    public static int TypeToIndex(int id)
    {
        type2index.TryGetValue(id, out int value);
        return value;
    }
    public static int IndexToType(int idx)
    {
        return 1 << idx;
    }
}
