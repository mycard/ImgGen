namespace ImgGen
{
    using System;
    using System.Runtime.InteropServices;

    [Flags]
    public enum Attribute : int
    {
        ATTRIBUTE_EARTH = 0x01,
        ATTRIBUTE_WATER = 0x02,
        ATTRIBUTE_FIRE = 0x04,
        ATTRIBUTE_WIND = 0x08,
        ATTRIBUTE_LIGHT = 0x10,
        ATTRIBUTE_DARK = 0x20,
        ATTRIBUTE_DEVINE = 0x40
    }

    [Flags]
    public enum Race : int
    {
        RACE_WARRIOR = 0x1,
        RACE_SPELLCASTER = 0x2,
        RACE_FAIRY = 0x4,
        RACE_FIEND = 0x8,
        RACE_ZOMBIE = 0x10,
        RACE_MACHINE = 0x20,
        RACE_AQUA = 0x40,
        RACE_PYRO = 0x80,
        RACE_ROCK = 0x100,
        RACE_WINDBEAST = 0x200,
        RACE_PLANT = 0x400,
        RACE_INSECT = 0x800,
        RACE_THUNDER = 0x1000,
        RACE_DRAGON = 0x2000,
        RACE_BEAST = 0x4000,
        RACE_BEASTWARRIOR = 0x8000,
        RACE_DINOSAUR = 0x10000,
        RACE_FISH = 0x20000,
        RACE_SEASERPENT = 0x40000,
        RACE_REPTILE = 0x80000,
        RACE_PSYCHO = 0x100000,
        RACE_DEVINE = 0x200000,
        RACE_CREATORGOD = 0x400000,
        RACE_WYRM = 0x800000,
        RACE_CYBERS = 0x1000000
    }

    [Flags]
    public enum Type : int
    {
        TYPE_MONSTER = 0x1,
        TYPE_SPELL = 0x2,
        TYPE_TRAP = 0x4,
        TYPE_NORMAL = 0x10,
        TYPE_EFFECT = 0x20,
        TYPE_FUSION = 0x40,
        TYPE_RITUAL = 0x80,
        TYPE_TRAPMONSTER = 0x100,
        TYPE_SPIRIT = 0x200,
        TYPE_UNION = 0x400,
        TYPE_DUAL = 0x800,
        TYPE_TUNER = 0x1000,
        TYPE_SYNCHRO = 0x2000,
        TYPE_TOKEN = 0x4000,
        TYPE_QUICKPLAY = 0x10000,
        TYPE_CONTINUOUS = 0x20000,
        TYPE_EQUIP = 0x40000,
        TYPE_FIELD = 0x80000,
        TYPE_COUNTER = 0x100000,
        TYPE_FLIP = 0x200000,
        TYPE_TOON = 0x400000,
        TYPE_XYZ = 0x800000,
        TYPE_PENDULUM = 0x1000000,
        TYPE_SPSUMMON = 0x2000000,
        TYPE_LINK = 0x4000000
    }

    [Flags]
    public enum LinkMarker : int
    {
        LINK_MARKER_BOTTOM_LEFT = 0x1,
        LINK_MARKER_BOTTOM = 0x2,
        LINK_MARKER_BOTTOM_RIGHT = 0x4,
        LINK_MARKER_LEFT = 0x8,
        LINK_MARKER_RIGHT = 0x20,
        LINK_MARKER_TOP_LEFT = 0x40,
        LINK_MARKER_TOP = 0x80,
        LINK_MARKER_TOP_RIGHT = 0x100
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Data
    {
        public int code;
        public int alias;
        public int setcode;
        public Type type;
        public int level;
        public Attribute attribute;
        public Race race;
        public int attack;
        public int defence;
        public string name;
        public string text;

        public bool IsType(Type typ)
        {
            return (this.type & typ) > 0;
        }
    }
}

