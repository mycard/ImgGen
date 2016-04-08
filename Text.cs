namespace ImgGen
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Text
    {
        public string name;
        public string text;
        public string[] desc;
    }
}

