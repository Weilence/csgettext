using System.Text;

namespace csgettext;

public class PotFile
{
    private readonly SortedSet<PoMsg> _msgs = [];

    public void Add(IEnumerable<string> msgIds)
    {
        foreach (var msgId in msgIds)
        {
            _msgs.Add(new PoMsg()
            {
                MsgId = msgId,
            });
        }
    }

    public void Add(IEnumerable<PoMsg> msgs)
    {
        foreach (var poMsg in msgs)
        {
            _msgs.Add(poMsg);
        }
    }

    public IEnumerable<PoMsg> Msgs => _msgs;

    public string ToContent()
    {
        var sb = new StringBuilder();

        foreach (var msg in Msgs)
        {
            sb.AppendLine($"msgid \"{msg.MsgId}\"");
            sb.AppendLine($"msgstr \"{msg.MsgStr}\"");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}