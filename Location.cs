namespace ImgGen
{
    using System;

    public enum Location : byte
    {
        LOCATION_DECK = 1,
        LOCATION_EXTRA = 0x40,
        LOCATION_GRAVE = 0x10,
        LOCATION_HAND = 2,
        LOCATION_MZONE = 4,
        LOCATION_ONFIELD = 12,
        LOCATION_OVERLAY = 0x80,
        LOCATION_REMOVED = 0x20,
        LOCATION_SZONE = 8
    }
}

