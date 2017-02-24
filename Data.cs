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
        public Type type;
        public int level;
        public Attribute attribute;
        public Race race;
        public int attack;
        public int defence;

        public bool isType(Type typ)
        {
            return (type & typ) == typ;
        }
    }
}

