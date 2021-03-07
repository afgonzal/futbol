using System;

namespace Futbol.Seasons.BusinessEntities
{
    public class TeamStats
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public byte G { get; set; }

        public byte W { get; set; }

        public byte D { get; set; }

        public byte L { get; set; }

        public sbyte  Sanctions { get; set; }

        public byte GF { get; set; }

        public byte GA { get; set; }

        public sbyte GD => (sbyte)(GF - GA);

        public byte Pts => (byte)(W * 3 + D);

        /// <summary>
        /// Reset to 0 everything
        /// </summary>
        public void Reset()
        {
            G = 0;
            W = 0;
            L = 0;
            D = 0;
            GF = 0;
            GA = 0;
            Sanctions = 0;
        }

        public static TeamStats operator +(TeamStats a, TeamStats b)
        {
            if (a.Id != b.Id)
                throw new InvalidOperationException("Can't add stats from different teams.");
            return new TeamStats
            {
                Id = a.Id, W = (byte) (a.W + b.W), D = (byte) (a.D + b.D), L = (byte) (a.L + b.L), G = (byte) (a.G + b.G),
                Name = a.Name,
                GF = (byte) (a.GF + b.GF),
                GA = (byte) (a.GA + b.GA), Sanctions = (sbyte) (a.Sanctions + b.Sanctions)
            };
        }
    }
}