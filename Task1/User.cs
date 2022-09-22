using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rootobject
{
    public User[] Property1 { get; set; }
}

public class User
{
    public int id { get; set; }
    public string login { get; set; }
    public string password { get; set; }
    public string name { get; set; }
    public int type_id { get; set; }
    public DateTime last_visit_date { get; set; }
}

