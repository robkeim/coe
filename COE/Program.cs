namespace COE;

class Program
{
    public static readonly Dictionary<Name, Name> ForcedMatches = new Dictionary<Name, Name>
    {
        // Add items to this list to ensure certain people get paired with others in the format:
        // { Name.Person_To_Given, Name.Person_To_Receive },
    };

    public static readonly HashSet<Name> ForcedMatchees = new HashSet<Name>(ForcedMatches.Values);

    public const int CurrentYear = 2016;
    public static readonly EmailStatus EmailStatus = EmailStatus.Disabled;

    // Lines in the file follow the following format:
    // Name;[Yes/No];Address
    // Address format: 1600 Pennsylvania Ave NW, Washington, DC 20500
    public const string ResponsesDocument = @"c:\users\rkeim\desktop\responses.txt";

    static void Main(string[] args)
    {
        // Steps to use the program
        // 0. Update the current year and set the EmailStatus to Enabled

        // 1. Send initial email
        //Email.SendInitialEmail();

        // 2. Send wall of fame
        //Email.SendWallOfFame();

        // 3. Send wall of shame
        //Email.SendWallOfShame();
        
        // 4. Send pairings and respond to everyone ensuring they have received their pairings
        //Email.SendPairings();

        Console.WriteLine("Done!");
        Console.ReadLine();
    }
}
