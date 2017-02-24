namespace ImgGen
{
    using System;

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
}

