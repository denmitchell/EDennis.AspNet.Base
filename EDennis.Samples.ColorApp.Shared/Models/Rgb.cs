using EDennis.NetStandard.Base;

namespace EDennis.Samples.ColorApp
{
    public class RgbHistory : Rgb { }
    public class Rgb: TemporalEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
    }
}
