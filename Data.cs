namespace ImgGen
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Data
    {
        public int code;
        public int alias;
        public int setcode;
        public int type;
        public int level;
        public int attribute;
        public int race;
        public int attack;
        public int defence;
    }
}

