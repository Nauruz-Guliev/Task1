using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Npgsql;

using System.Text.RegularExpressions;

namespace Task1
{



    public partial class Form1 : Form
    {
        private String users;
        private String users_types;
        private JArray users_root;
        private JArray users_type_root;

        private UserType[] userTypesArr;

        private UsersRoot users_array;
        private UsersTypesRoot users_types_array;


        private int seletedValue;
        private HashSet<int> unique_type_id = new HashSet<int>();
        private List<TextBox[]> allTextBoxes = new List<TextBox[]>();
        private int row_count = 0;
        private Boolean is_insertion_completed = false;

        private string date_regex = "[0-9]{2}.[0-9]{2}.[0-9]{4}\\s[0-9]{1,2}:[0-9]{2}:[0-9]{2}";
        private string id_regex = "([^0\\s][0-9]{1,6})";


        private DataGridViewComboBoxColumn dgvCbx = new DataGridViewComboBoxColumn();
        

        public Form1()
        {
            InitializeComponent();
            initValues();
            getValues(users_root, unique_type_id);
        }
        private void initValues()
        {

            users = "C:\\Users\\gulievnt\\source\\repos\\Task1\\Task1\\Users.json";
            users_types = "C:\\Users\\gulievnt\\source\\repos\\Task1\\Task1\\UserTypes.json";
            users_root = loadJson(users);
            users_type_root = loadJson(users_types);
            users_array = JsonConvert.DeserializeObject<UsersRoot>(users_root[0].ToString());
            users_types_array = JsonConvert.DeserializeObject<UsersTypesRoot>(users_type_root[0].ToString());
            dgvCbx.HeaderText = "User type";
            dataGridView1.Columns.Add(dgvCbx);
            dgvCbx.DataSource = getUsersTypesStrings(users_type_root);


        }

        private void getValues(JArray rootObject, HashSet<int> unique_type_id)
        {
            calculateUnique(rootObject, unique_type_id);
            foreach (int item in unique_type_id)
            {
                comboBox1.Items.Add(item.ToString());
            }
            comboBox1.SelectedIndex = 0;

        }

        private async Task addValues(Boolean isFilterOn, int filter_value, JArray rootObject, HashSet<int> unique_type_id)
        {
          
            dgvCbx.DataSource = getUsersTypesStrings(users_type_root);

            for (int i = 0; i < rootObject.Count; i++)
            {
                User user = JsonConvert.DeserializeObject<User>(rootObject[i].ToString());

                

                if (isFilterOn)
                {
                    if (filter_value == user.type_id)
                    {
                        row_count = this.dataGridView1.Rows.Add();
                        insertIntoGrid(row_count, user, dgvCbx);
                        

                    }
                } else
                {
                    row_count = this.dataGridView1.Rows.Add();
                    insertIntoGrid(row_count, user, dgvCbx);
                    
                }
            }
            
            is_insertion_completed = true;
        }

        

        private String[] getUsersTypesStrings(JArray types_root)
        {
            String[] types = new String[types_root.Count];
            for (int i = 0; i < types_root.Count; i++)
            {
                UserType userType = JsonConvert.DeserializeObject<UserType>(types_root[i].ToString());
                types[i] = userType.name;
            }
            return types;
        }

        private UserType[] getUsersTypesArr(JArray types_root)
        {
            UserType[] types = new UserType[types_root.Count];
            for(int i = 0; i < types_root.Count; i++)
            {
                UserType userType = JsonConvert.DeserializeObject<UserType>(types_root[i].ToString());
                types[i] = userType;
            }
            return types;
        }  

        private void insertIntoGrid(int row_count, User user, DataGridViewComboBoxColumn dgvComboColumn)
        {
            
            dataGridView1.Rows[row_count].Cells[0].Value = user.id.ToString();
            dataGridView1.Rows[row_count].Cells[1].Value = user.name.ToString();
            dataGridView1.Rows[row_count].Cells[2].Value = user.login.ToString();
            dataGridView1.Rows[row_count].Cells[3].Value = user.password.ToString();
            dataGridView1.Rows[row_count].Cells[4].Value = user.last_visit_date.ToString();

            dataGridView1.Rows[row_count].Cells[5].Value = dgvComboColumn.Items[user.type_id-1];

           
          

        }

       
        private void calculateUnique(JArray rootObject, HashSet<int> unique_type_id)
        {
            for (int i = 0; i < rootObject.Count; i++)
            {
                User user = JsonConvert.DeserializeObject<User>(rootObject[i].ToString());
                unique_type_id.Add(user.type_id);
            }
        }
     

      
        
        private JArray loadJson(string filePath)
        {
            JArray obj;
            using (StreamReader file = File.OpenText(@filePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                obj = (JArray)JToken.ReadFrom(reader);
            }
            return obj;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            dataGridView1.Rows.Clear();
            seletedValue = Int32.Parse(comboBox1.Text.ToString());
            addValues(true, seletedValue, users_root, unique_type_id);
        }

      
        private int calcualteAmountOfRows(int valueSelected, JArray rootObject)
        {
            int count = 0;
            for (int i = 0; i < rootObject.Count; i++)
            {
                User user = JsonConvert.DeserializeObject<User>(rootObject[i].ToString());
                if (user.type_id == valueSelected)
                {
                    count++;
                }
            }
            return count;
        }

        private void btn_showAll_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            dataGridView1.Rows.Clear();
            addValues(false, seletedValue, users_root, unique_type_id);
            
        }



        private async void btn_SaveAll_Click(object sender, EventArgs e)
        {

            var progress = new Progress<int>(percent =>
            {
                progressBar1.Value = percent;
            });
            await saveAll(progress);
            progressBar1.Value = 100;


        }

        private async Task saveAll(IProgress<int> progress)
        {
            await Task.Run(() =>
            {
                int amountOfUsers = dataGridView1.Rows.Count - 1;
                float percent = 100 / amountOfUsers;

                User[] users = new User[amountOfUsers];

                int id;
                string login;
                string password;
                string name;
                int type_id;
                DateTime date;

                for (int i = 0; i < amountOfUsers; i++)
                {
                    id = Int32.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString());
                    login = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    password = dataGridView1.Rows[i].Cells[2].Value.ToString();
                    name = dataGridView1.Rows[i].Cells[3].Value.ToString();
                    type_id = findIdOfUser(dataGridView1.Rows[i].Cells[5].Value.ToString());
                    date = DateTime.Parse(dataGridView1.Rows[i].Cells[4].Value.ToString());
                    User user = new User()
                    {
                        id = id,
                        login = login,
                        password = password,
                        name = name,
                        type_id = type_id,
                        last_visit_date = date
                    };
                    System.Threading.Thread.Sleep(100);
                    users[i] = user;
                    progress.Report(Int32.Parse((percent * i).ToString()));
                }

                var json_file = JsonConvert.SerializeObject(users);
                String pathToFile = "C:\\Users\\gulievnt\\source\\repos\\Task1\\Task1\\EditedUsers.json";
                writeFileAsync(pathToFile, json_file, false);
            });
        }

        private int findIdOfUser(String user_type)
        {
            UserType[] types = getUsersTypesArr(users_type_root);
            for (int i = 0; i < getUsersTypesArr(users_type_root).Length; i++)
            {
                if(types[i].name == user_type)
                {
                    return types[i].id;
                }
            }
            return -1;
        }


        public async Task writeFileAsync(string filePath, string messaage, bool append = true)
        {
            using (FileStream stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                await sw.WriteLineAsync(messaage);
            }
        }

     

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4 && e.RowIndex != -1 && is_insertion_completed) 
            { 
                String value = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (!Regex.IsMatch(value, date_regex))
                { 
                    MessageBox.Show("Wrong input was given");
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                    return;

                }
            }
            if (e.ColumnIndex == 0 && e.RowIndex != -1 && is_insertion_completed) 
            { 
                String id_from_grid = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                int id_to_search = 0;

                if (!Regex.IsMatch(id_from_grid, id_regex) || id_from_grid.Length > 4 || Regex.IsMatch(id_from_grid, "[a-zA-Z.]{1,5}"))
                { 
                    MessageBox.Show("ID is too big or contains inappropriate symbols!");
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                    return;

                }
                else { 
                     id_to_search = Int32.Parse(id_from_grid);

                }
                if (isIdAlreadyPresent(id_to_search))
                {
                    MessageBox.Show("Such an id already exists!");
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                    return;
                }
                

            }

            if (e.ColumnIndex == 2 && e.RowIndex != -1 && is_insertion_completed)
            {
                String loginToSearch = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (isLoginAlreadyPresent(loginToSearch))
                {
                    MessageBox.Show("Such a Login already exists!");
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                    return;
                }
            }
        }

        private Boolean isLoginAlreadyPresent(String login)
        {
            for (int i = 0; i < users_root.Count; i++)
            {
                User user = JsonConvert.DeserializeObject<User>(users_root[i].ToString());
                if (user.login == login)
                {
                    return true;
                }
            }
            return false;
        }
        private Boolean isIdAlreadyPresent(int id_to_search)
        {
            for (int i = 0; i < users_root.Count; i++)
            {
                User user = JsonConvert.DeserializeObject<User>(users_root[i].ToString());
                int id = user.id; 
                if (id == id_to_search) 
                {
                    return true;
                }
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
    
