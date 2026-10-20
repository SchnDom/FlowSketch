namespace PID2Process;
internal static class EquipmentSearch
{
    internal static bool Matches(EquipmentItem item,string query)
    {
        query=query.Trim();if(query.Length==0)return true;
        var fields=new[]{item.Tag,item.Function,item.InstallationLocation};
        return fields.Any(value=>query.IndexOfAny(['*','?'])>=0
            ?System.IO.Enumeration.FileSystemName.MatchesSimpleExpression(query.ToUpperInvariant(),(value??"").ToUpperInvariant(),ignoreCase:false)
            :(value??"").Contains(query,StringComparison.OrdinalIgnoreCase));
    }
}
