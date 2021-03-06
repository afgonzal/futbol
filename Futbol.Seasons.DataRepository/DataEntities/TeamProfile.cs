﻿using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Teams")]

    public class TeamProfile
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        internal string Sk { get; set; }

        [DynamoDBIgnore]
        public int TeamId
        {
            get => Sk.ParseCompositeKey<int>(1, false).GetValueOrDefault();
            set => Sk = $"Profile#{value}";
        }

        [DynamoDBProperty]
        public string TeamName { get; set; }

        [DynamoDBProperty]
        public byte ConferenceId { get; set; }

        [DynamoDBProperty]
        public List<short> Years { get; set; }

        [DynamoDBProperty]
        public List<string> Delegates { get; set; }

        [DynamoDBProperty]
        public string Abbreviation { get; set; }

    }
}
