using System;
using System.Collections.Generic;
using System.Text;
using Futbol.Seasons.Services;
using Moq;
using NUnit.Framework;

namespace Futbol.SeasonsAPI.Tests.SeasonAdminControllerTests
{
    [TestFixture]
    public class VerifyTeamStatsTests
    {
        private Mock<ITeamsService> _service;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ITeamsService>();

        }
    }
}
