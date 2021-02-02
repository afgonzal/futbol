namespace Futbol.Seasons.BusinessEntities
{
    public class TeamSeasonStats
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public byte G { get; set; }

        public byte W { get; set; }

        public byte D { get; set; }

        public byte L { get; set; }

        public byte Sanctions { get; set; }

        public byte GF { get; set; }

        public byte GA { get; set; }

        public sbyte GD => (sbyte)(GF - GA);

        public byte Pts => (byte)(W * 3 + D);
    }
}