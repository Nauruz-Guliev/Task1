
public class UsersTypesRoot
{
    public UserType[] Property1 { get; set; }
}

public class UserType
{
    public int id { get; set; }
    public string name { get; set; }
    public bool allow_edit { get; set; }
}
