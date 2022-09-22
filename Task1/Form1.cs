using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Task1
{
    public partial class Form1 : Form
    {
        private String users;
        private JArray rootobject;
        private Rootobject root;
        private int seletedValue;
        private HashSet<int> unique_type_id = new HashSet<int>();
        private List<TextBox[]> allTextBoxes = new List<TextBox[]>();
        private int row_count = 0;


        public Form1()
        {
            InitializeComponent();
            initValues();
            getValues(rootobject, unique_type_id);
        }
        private void initValues()
        {
             users = "C:\\Users\\nauru\\source\\repos\\Task1\\Task1\\Users.json";
             rootobject = loadJson(users);
             root = JsonConvert.DeserializeObject<Rootobject>(rootobject[0].ToString());
            
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
            TextBox id, login, name, password, type_id, last_visit_date;

            for (int i = 0; i < rootObject.Count; i++)
            {
                 User user = JsonConvert.DeserializeObject<User>(rootObject[i].ToString());


                if (isFilterOn)
                {
                    if (filter_value == user.type_id)
                    {
                        row_count = this.dataGridView1.Rows.Add();
                        insertIntoGrid(row_count, user);
                    }
                } else
                {
                    row_count = this.dataGridView1.Rows.Add();
                    insertIntoGrid(row_count, user);
                }
            }
        }

        private void insertIntoGrid(int row_count, User user)
        {
            dataGridView1.Rows[row_count].Cells[0].Value = user.id.ToString();
            dataGridView1.Rows[row_count].Cells[1].Value = user.name.ToString();
            dataGridView1.Rows[row_count].Cells[2].Value = user.login.ToString();
            dataGridView1.Rows[row_count].Cells[3].Value = user.password.ToString();
            dataGridView1.Rows[row_count].Cells[4].Value = user.type_id.ToString();
            dataGridView1.Rows[row_count].Cells[5].Value = user.last_visit_date.ToString();
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
            addValues(true, seletedValue, rootobject, unique_type_id);
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
            addValues(false, seletedValue, rootobject, unique_type_id);
            
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
                    type_id = Int32.Parse(dataGridView1.Rows[i].Cells[4].Value.ToString());
                    date = DateTime.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString());
                    User user = new User()
                    {
                        id = id,
                        login = login,
                        password = password,
                        name = name,
                        type_id = type_id,
                        last_visit_date = date
                    };
                    System.Threading.Thread.Sleep(900);
                    users[i] = user;
                    progress.Report(Int32.Parse((percent * i).ToString()));
                }

                var json_file = JsonConvert.SerializeObject(users);
                String pathToFile = "C:\\Users\\nauru\\source\\repos\\Task1\\Task1\\EditedUsers.json";
                writeFileAsync(pathToFile, json_file, false);
            });
        }


        public async Task writeFileAsync(string filePath, string messaage, bool append = true)
        {
            using (FileStream stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                await sw.WriteLineAsync(messaage);
            }
        }

        private void connectToDatabase()
        {
            var connectionString = "Host=localhost;Username=postgres;Password=nauruz0304;Database=postgres";

            using var con = new NpgsqlConnection(connectionString);
            con.Open();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        

    }

}

