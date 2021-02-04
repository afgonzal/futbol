
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Futbol.SeasonsAPI.Models
{
    public class TeamModel
    {
        [Required]
        public int? Id { get; set; }
        [Required]
        public short? Year { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public byte? ConferenceId { get; set; }
    }

    public class TeamAddRequest : TeamModel
    {
        [Required]
        public IEnumerable<short> Years { get; set; }

        public IEnumerable<string> Delegates { get; set; }

        public string Abbr { get; set; }
    }
}