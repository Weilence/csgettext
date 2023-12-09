namespace csgettext;

public class PoMsg : IEqualityComparer<PoMsg>, IComparable<PoMsg>
{
    public string MsgId { get; set; } = "";
    public string MsgStr { get; set; } = "";

    public bool Equals(PoMsg? x, PoMsg? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.MsgId == y.MsgId;
    }

    public int GetHashCode(PoMsg obj)
    {
        return obj.MsgId.GetHashCode();
    }

    public int CompareTo(PoMsg? other)
    {
        return string.Compare(MsgId, other?.MsgId, StringComparison.OrdinalIgnoreCase);
    }
}