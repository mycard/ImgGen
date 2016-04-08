namespace ImgGen
{
    using System;

    public enum Position : byte
    {
        POS_ATTACK = 3,
        POS_DEFENCE = 12,
        POS_FACEDOWN = 10,
        POS_FACEDOWN_ATTACK = 2,
        POS_FACEDOWN_DEFENCE = 8,
        POS_FACEUP = 5,
        POS_FACEUP_ATTACK = 1,
        POS_FACEUP_DEFENCE = 4
    }
}

