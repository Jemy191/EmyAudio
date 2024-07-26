namespace Core.Models;

public class Setting
{
    public bool FirstTime { get; set; } = true;
    public bool Loop { get; set; }
    public int Volume { get; set; } = 100;
}