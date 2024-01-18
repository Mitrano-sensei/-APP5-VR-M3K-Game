using UnityEngine;

public class DockerBuyerItem : SingleUseItem
{
    protected override void Start()
    {
        base.Start();

        OnPick.AddListener(HighlightBuyableDocker);
        OnUnPick.AddListener(UnHighlightBuyableDocker);

        OnUse.AddListener(BuyDock);
    }

    private void HighlightBuyableDocker()
    {
        var dockManager = DockManager.Instance;
        dockManager.HighlightBuyableDocks();
    }

    private void UnHighlightBuyableDocker()
    {
        var dockManager = DockManager.Instance;
        dockManager.HighlightBuyableDocks(false);
    }
    
    private void BuyDock(UseEvent useEvent)
    {
        var dockManager = DockManager.Instance;
        var docker = useEvent.UsedOn?.GetComponent<Docker>();
        if (docker == null) return;
        
        _logger.Trace("Buying dock at " + docker.X + ", " + docker.Y + ", " + docker.Z);
        if (dockManager.IsBuyable(docker)) docker.Buy();
    }

    protected override bool CanBeUsedOnObject(GameObject obj)
    {
        var dock = obj.GetComponent<Docker>();
        if (dock == null) return false;
        var dockManager = DockManager.Instance;
        return !dock.IsActive && dockManager.IsBuyable(dock);
    }

    protected override bool CanBeUsedOnGround() => false;
}
