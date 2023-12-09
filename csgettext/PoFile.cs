using System.Text;
using System.Text.RegularExpressions;

namespace csgettext;

public class PoFile
{
    private readonly Dictionary<string, PoMsg> _msgDic = new();

    public PoFile(string content)
    {
        var msgs = Regex.Split(content, "(\r?\n){2,}")
            .Select(m => m.Trim())
            .Where(m => !m.StartsWith("#"))
            .Select(m =>
            {
                var lines = Regex.Split(m, "\r?\n").ToList();
                var msg = new PoMsg();

                foreach (var line in lines)
                {
                    var index = line.IndexOf(" ");
                    if (index < 0)
                    {
                        continue;
                    }

                    var key = line.Substring(0, index).Trim('"');
                    var value = line.Substring(index + 1).Trim('"');
                    switch (key)
                    {
                        case "msgid":
                            msg.MsgId = value;
                            break;
                        case "msgstr":
                            msg.MsgStr = value;
                            break;
                    }
                }

                return msg;
            })
            .Where(m => m.MsgId != "");

        foreach (var msg in msgs)
        {
            _msgDic[msg.MsgId] = msg;
        }
    }

    public string ToContent(PotFile pot)
    {
        var sb = new StringBuilder();

        foreach (var potMsg in pot.Msgs)
        {
            var msg = _msgDic.GetValueOrDefault(potMsg.MsgId);
            sb.AppendLine($"msgid \"{potMsg.MsgId}\"");
            if (msg == null)
            {
                sb.AppendLine($"msgstr \"\"");
            }
            else
            {
                sb.AppendLine($"msgstr \"{msg.MsgStr}\"");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}