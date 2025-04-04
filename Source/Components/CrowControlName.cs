namespace Celeste.Mod.CrowControl
{
    class CrowControlName : Monocle.Component
    {
        public string Name { get; set; }

        public CrowControlName(bool active, bool visible, string name) : base(active, visible)
        {
            Name = name;
        }
    }
}
