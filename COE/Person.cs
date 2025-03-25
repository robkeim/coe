using System.Diagnostics;

namespace COE;

[DebuggerDisplay("Name = {Name}")]
public class Person
{
    public Name Name { get; set; }

    public string Email { get; set; }

    public List<Name> Siblings { get; set; }

    public Name? Spouse { get; set; }

    public Dictionary<int, Name?> History { get; set; }

    public bool IsInactive => string.IsNullOrWhiteSpace(Email);
}
