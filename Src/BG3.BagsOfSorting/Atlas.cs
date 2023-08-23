namespace BG3.BagsOfSorting
{
    public class Atlas
    {
        public class Icon
        {
            public string MapKey { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public int IconWidth { get; set; }
        public int IconHeight { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public string TextureFile { get; set; }

        public List<Icon> Icons { get; set; }
    }
}
