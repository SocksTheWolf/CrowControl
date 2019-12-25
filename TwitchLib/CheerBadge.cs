namespace Celeste.Mod.CrowControl
{
    public class CheerBadge
    {
        /// <summary>Property representing raw cheer amount represented by badge.</summary>
        public int CheerAmount { get; }
        /// <summary>Property representing the color of badge via an enum.</summary>
        public BadgeColor Color { get; }

        /// <summary>Constructor for CheerBadge</summary>
        public CheerBadge(int cheerAmount)
        {
            CheerAmount = cheerAmount;
            Color = getColor(cheerAmount);
        }

        private BadgeColor getColor(int cheerAmount)
        {
            if (cheerAmount >= 10000)
                return BadgeColor.Red;
            if (cheerAmount >= 5000)
                return BadgeColor.Blue;
            if (cheerAmount >= 1000)
                return BadgeColor.Green;
            return cheerAmount >= 100 ? BadgeColor.Purple : BadgeColor.Gray;
        }
    }
}
