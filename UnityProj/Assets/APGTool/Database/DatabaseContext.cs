using System.IO;
using SQLite;
using UnityEngine;

public class DatabaseContext
{
    private static DatabaseContext _instance = null;
    public static DatabaseContext Instance
    {
        get
        {
            _instance ??= new DatabaseContext();
            return _instance;
        }
    }

    private string _databasePath = Path.Combine(Application.dataPath, "APGTool", "Database.db");

    public SQLiteConnection Connection => new SQLiteConnection(_databasePath);

    private DatabaseContext()
    {
        using (var conn = new SQLiteConnection(_databasePath))
        {
            conn.CreateTable<Command>();
        }
    }

}

public class Command
{
    [PrimaryKey,AutoIncrement]
    public int ID { get; set; }

    public string Name { get; set; }
    public int Cooldown { get; set; }

    public int NetworkTriggerObjectID { get; set; }
}
