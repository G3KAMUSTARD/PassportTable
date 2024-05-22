using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

namespace passport
{
    public partial class Form1 : Form
    {
        DB db = new DB();

        enum RowState
        {
            Existed,
            New,
            Modified,
            ModifiedNew,
            Deleted
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //вывести таблицы на форму
            db.OpenConection();
            string applicant = "select * from Applicant";
            string passport = "select * from Passport";

            SqlDataAdapter adapter1 = new SqlDataAdapter(applicant, db.GetConection());
            DataSet apl = new DataSet();
            adapter1.Fill(apl, "Applicant");
            dataGridView1.DataSource = apl.Tables[0];

            SqlDataAdapter adapter2 = new SqlDataAdapter(passport, db.GetConection());
            DataSet pas = new DataSet();
            adapter2.Fill(pas, "Passport");
            dataGridView2.DataSource = pas.Tables[0];
            db.CloseConection();

            //назначение макимальных дат в datetimepicker и назначение сегодняшней даты по умолчанию в выдаче паспорта
            dob.MaxDate = DateTime.Today.AddYears(-18);
            dob_chan.MaxDate = DateTime.Today.AddYears(-18);

            issue.MaxDate = DateTime.Today.AddDays(7);
            DateTime selectedDate = issue.Value;
            DateTime newDate = selectedDate.AddYears(10);
            expiration.Value = newDate;

            issue_chan.MaxDate = DateTime.Today.AddDays(7);
            DateTime selectedData = issue_chan.Value;
            DateTime newData = selectedData.AddYears(10);
            expiration_chan.Value = newData;

        }

        //метод "поиск заявителя"
        private void SearchAp(DataGridView dataGridView1)
        {
            string searchapString = $"select * from Applicant where concat (id, name, date_of_birth, gender, address, phone_number) like '%" + textBox1.Text + "%'";
            SqlCommand com = new SqlCommand(searchapString, db.GetConection());
            SqlDataAdapter adapter = new SqlDataAdapter(com);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "applicant");
            dataGridView1.DataSource = ds.Tables[0];
            db.CloseConection();
        }

        //вызов метода "поиск заявителя"
        private void SearchApp_Click(object sender, EventArgs e)
        {
            SearchAp(dataGridView1);
        }

        //метод поиск паспорта
        private void SearchPas(DataGridView dataGridView2)
        {
            string searchpasString = $"select * from Passport where concat (number,issue_date,expiration_date,type, applicant_id ) like '%" + textBox2.Text + "%'";
            SqlCommand com = new SqlCommand(searchpasString, db.GetConection());
            SqlDataAdapter adapter = new SqlDataAdapter(com);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "passport");
            dataGridView2.DataSource = ds.Tables[0];
            db.CloseConection();
        }

        //вызов метода "поиск паспорта"
        private void SearchPass_Click(object sender, EventArgs e)
        {
            SearchPas(dataGridView2);
        }

        //обновление отображения таблицы заявителей
        private void Updapp_Click(object sender, EventArgs e)
        {
            db.OpenConection();
            string applicant = "select * from Applicant";
            SqlDataAdapter adapter1 = new SqlDataAdapter(applicant, db.GetConection());
            DataSet apl = new DataSet();
            adapter1.Fill(apl, "Applicant");
            dataGridView1.DataSource = apl.Tables[0];
            db.CloseConection();
        }

        //обновление отображения таблицы паспортов
        private void Updpass_Click(object sender, EventArgs e)
        {
            db.OpenConection();
            string passport = "select * from Passport";
            SqlDataAdapter adapter2 = new SqlDataAdapter(passport, db.GetConection());
            DataSet pas = new DataSet();
            adapter2.Fill(pas, "Passport");
            dataGridView2.DataSource = pas.Tables[0];
            db.CloseConection();
        }


        //кнопка удаления заявителя
        private void Delapp_Click(object sender, EventArgs e)
        {

            SqlCommand com = new SqlCommand("DELETE FROM applicant WHERE id=@id", db.GetConection());
            int id = int.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString());
            com.Parameters.AddWithValue("@id", id);
            db.OpenConection();

            DialogResult result = MessageBox.Show("Вы уверены что хотите удалить заявителя?\n Будут так же удалены данные о паспортах, оформленных на него.",
                "Уведомление", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Yes)
            {
                try
                {
                    com.ExecuteNonQuery();
                    MessageBox.Show("Запись удалена. Обновите обе таблицы.","Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch
                {
                    MessageBox.Show("Вы ничего не выбрали!", "Возникла ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            db.CloseConection();
        }

        //кнопка удаления паспорта
        private void Delpass_Click(object sender, EventArgs e)
        {
            string number = dataGridView2.CurrentRow.Cells[0].Value.ToString();
            SqlCommand com = new SqlCommand($"DELETE FROM Passport WHERE number like'{number}'", db.GetConection());
            
            com.Parameters.AddWithValue("@number", number);
            db.OpenConection();

            DialogResult result = MessageBox.Show
                ("Вы уверены что хотите удалить данные о паспорте?", "Уведомление", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Yes)
            {
                try
                {
                    com.ExecuteNonQuery();
                    MessageBox.Show("Запись удалена. Обновите таблицу.", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch
                {
                    MessageBox.Show("Вы ничего не выбрали!","Возникла ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            db.CloseConection();
        }


        //добавление заявителя
        private void Adapp_Click(object sender, EventArgs e)
        {
            if (name.Text == "")
            {
                MessageBox.Show( "Вы не ввели ФИО заявителя!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (CloneApp())
            {
                return;
            }
            else if (phone.Text.Length < 17)
            {
                MessageBox.Show("Вы не до конца ввели номер телефона", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }         
            else if (gender.Text == "")
            {
                MessageBox.Show( "Вы не выбрали пол!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            { 
                db.OpenConection();
                SqlCommand cmd;
                SqlCommand iden;
                cmd = new SqlCommand("INSERT INTO Applicant ( name, date_of_birth, gender, address, phone_number)  " +
                    "VALUES ( @name, @date_of_birth, @gender, @address, @phone_number)", db.GetConection());
                iden = new SqlCommand("SET IDENTITY_INSERT dbo.Applicant ON", db.GetConection());
                cmd.Parameters.AddWithValue("@name", name.Text);
                cmd.Parameters.AddWithValue("@date_of_birth", dob.Value);
                cmd.Parameters.AddWithValue("@gender", gender.Text);
                cmd.Parameters.AddWithValue("@address", adress.Text);
                cmd.Parameters.AddWithValue("@phone_number", phone.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Успешно", "Заявитель был(а) добавлен(a) в БД!");
                db.CloseConection();
            }

        }

        //очистка данных в поле добавления заявителя
        private void Clear_app_Click(object sender, EventArgs e)
        {
            name.Text = "";
            name.Clear();
            dob.Value = DateTime.Today.AddYears(-18);
            gender.SelectedIndex = -1;
            adress.Text = "";
            adress.Clear();
            phone.Text = "";
            phone.Clear();
        }


        //добавление паспорта
        private void Adpas_Click(object sender, EventArgs e)
        {
           
            if (number.Text.Length < 12)
            {
                MessageBox.Show("Вы не до конца ввели серию и номер паспорта!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (type.SelectedIndex == -1)
            {
                MessageBox.Show("Вы не выбрали тип паспорта!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (apid.Text == "")
            {
                MessageBox.Show("Вы должны выбрать ID заявителя!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (NoIDPass())
            {
                return;
            }
            else if (ClonePass())
            {
                return;
            }
            else if (CloneTypePass())
            {
                return;
            }
            else if (МoreTwoPass())
            {
                return;
            }
            else
            {
                db.OpenConection();
                SqlCommand cmd;
                cmd = new SqlCommand("INSERT INTO Passport ( number, issue_date, expiration_date, type, applicant_id)  " +
                    "VALUES ( @number,@issue_date, @expiration_date, @type, @applicant_id)", db.GetConection());
                cmd.Parameters.AddWithValue("@number", number.Text);
                cmd.Parameters.AddWithValue("@issue_date", issue.Value);
                cmd.Parameters.AddWithValue("@expiration_date", expiration.Value);
                cmd.Parameters.AddWithValue("@type", type.Text);
                cmd.Parameters.AddWithValue("@applicant_id", apid.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Успешно", "Паспорт был добавлен в БД!");
                db.CloseConection();
            }
        }

        //очистка данных в поле добавления паспорта
        private void Clear_pas_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            number.Text = "";
            number.Clear();
            issue.Value = now;
            expiration.Value = now;
            type.SelectedIndex = -1;
            apid.Text = "";
            apid.Clear();
        }
      

        //изменить даннные о заявителе
        private void ChanApp_Click(object sender, EventArgs e)
        {
            if (id.Text == "")
            {
                MessageBox.Show("Чтобы изменить данные, кликните 2  раза на строку в таблице", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (name_chan.Text == "")
            {
                MessageBox.Show("Вы не ввели ФИО заявителя!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (CloneApp())
            {
                return;
            }
            else if (phone_chan.Text.Length < 17)
            {
                MessageBox.Show("Вы не до конца ввели номер телефона", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (gender_chan.Text == "")
            {
                MessageBox.Show("Вы не выбрали пол!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                db.OpenConection();
                SqlCommand cmd;
                cmd = new SqlCommand("UPDATE Applicant " +
                "SET  name = @name ,date_of_birth = @date_of_birth , gender =@gender, address = @address, phone_number = @phone_number " +
                "WHERE id  like '%" + id.Text + "%'", db.GetConection());
                cmd.Parameters.AddWithValue("@name", name_chan.Text);
                cmd.Parameters.AddWithValue("@date_of_birth", dob_chan.Value);
                cmd.Parameters.AddWithValue("@gender", gender_chan.Text);
                cmd.Parameters.AddWithValue("@address", adress_chan.Text);
                cmd.Parameters.AddWithValue("@phone_number", phone_chan.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Успешно", "Данные о заявителе были обновлены!");
                db.CloseConection();
            }
        }

        //автоподставка из таблицы заявителей
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int a = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[a];
                id.Text = row.Cells[0].Value.ToString();
                name_chan.Text = row.Cells[1].Value.ToString();
                dob_chan.Text = row.Cells[2].Value.ToString();
                gender_chan.Text = row.Cells[3].Value.ToString();
                adress_chan.Text = row.Cells[4].Value.ToString();
                phone_chan.Text = row.Cells[5].Value.ToString();
            }
        }

        //очистка данных в поле изменить заявителя
        private void СlearChanApp_Click(object sender, EventArgs e)
        {
            id.Text = "";
            id.Clear();
            name_chan.Text = "";
            name_chan.Clear();
            dob_chan.Value = DateTime.Today.AddYears(-18);
            gender_chan.SelectedIndex = -1;
            adress_chan.Text = "";
            adress_chan.Clear();
            phone_chan.Text = "";
            phone_chan.Clear();
        }

        //изменить паспорт
        private void ChanPass_Click(object sender, EventArgs e)
        {
            
            if (number_chan.Text.Length < 12)
            {
                MessageBox.Show("Вы не до конца ввели серию и номер паспорта!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (type_chan.SelectedIndex == -1)
            {
                MessageBox.Show("Вы не выбрали тип паспорта!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else if (apid_chan.Text == "")
            {
                MessageBox.Show("Вы должны выбрать ID заявителя!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
          
            else
            {
                db.OpenConection();
                SqlCommand cmd;
                cmd = new SqlCommand("UPDATE Passport " +
                "SET  number = @number,issue_date = @issue_date, expiration_date = @expiration_date, type = @type, applicant_id = @applicant_id " +
                "WHERE applicant_id like '%" + apid_chan.Text + "%'", db.GetConection());
                cmd.Parameters.AddWithValue("@number", number_chan.Text);
                cmd.Parameters.AddWithValue("@issue_date", issue_chan.Value);
                cmd.Parameters.AddWithValue("@expiration_date", expiration_chan.Value);
                cmd.Parameters.AddWithValue("@type", type_chan.Text);
                cmd.Parameters.AddWithValue("@applicant_id", apid_chan.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Успешно", "Данные о паспорте были обновлены!");
                db.CloseConection();
            }
        }

        //автоподставка из таблицы паспортов по клику на строку
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int a = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView2.Rows[a];
                number_chan.Text = row.Cells[0].Value.ToString();
                issue_chan.Text = row.Cells[1].Value.ToString();
                expiration_chan.Text = row.Cells[2].Value.ToString();
                type_chan.Text = row.Cells[3].Value.ToString();
                apid_chan.Text = row.Cells[4].Value.ToString();
            }
        }

        //очистка данных в поле изменить паспорт
        private void ClearChanPass_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            number_chan.Text = "";
            number_chan.Clear();
            issue_chan.Value = now;
            expiration_chan.Value = now;
            type_chan.SelectedIndex = -1;
            apid_chan.Text = "";
            apid_chan.Clear();
        }

        


        //исключения на некорректные символы

        private void name_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsPunctuation(e.KeyChar))
            {
                e.Handled = true;

            }
            if (!Char.IsDigit(e.KeyChar)) return;
            else
                e.Handled = true;
        }
        private void name_chan_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsPunctuation(e.KeyChar))
            {
                e.Handled = true;

            }
            if (!Char.IsDigit(e.KeyChar)) return;
            else
                e.Handled = true;
        }

        private void adress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsPunctuation(e.KeyChar))
            {
                e.Handled = true;

            }
            if (!Char.IsDigit(e.KeyChar)) return;
            else
                e.Handled = true;
        }
        private void adress_chan_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsPunctuation(e.KeyChar))
            {
                e.Handled = true;

            }
            if (!Char.IsDigit(e.KeyChar)) return;
            else
                e.Handled = true;
        }

        //проверка-исключение на клона-заявителя
        private Boolean CloneApp()
        {
            
            var phoneap = phone.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string quertystring = $"select name, date_of_birth, gender, address, phone_number from Applicant where phone_number = '{phoneap}'";

            SqlCommand command = new SqlCommand(quertystring, db.GetConection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Заявитель уже есть в базе!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return true;
            }

            else
            {
                return false;
            }
        }

        //проверка-исключение на клона-паспорт
        private Boolean ClonePass()
        {

            var numberpass = number.Text;
            
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string quertystring = $"select number, issue_date, expiration_date, type, applicant_id from Passport where number = '{numberpass}'";

            SqlCommand command = new SqlCommand(quertystring, db.GetConection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Паспорт уже есть в базе!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return true;
            }
            else
            {
                return false;
            }
        }

        //проверка-исключение на паспорт такого же типа
        private Boolean CloneTypePass()
        {
            var apid = apid_chan.Text;
            var tip = type_chan.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string quertystring = $"select number, issue_date, expiration_date, type, applicant_id from Passport where type like '{tip}' and applicant_id = '{apid}' ";

            SqlCommand command = new SqlCommand(quertystring, db.GetConection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count >= 1)
            {
                MessageBox.Show("Больше двух паспортов  одного типа быть не может!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return true;
            }
            else
            {
                return false;
            }
        }

        //проверка-исключение на то, есть ли у заявителя уже 2 паспорта
        private Boolean МoreTwoPass()
        {
            var appid = apid.Text;
           

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string quertystring = $"select number, issue_date, expiration_date, type, applicant_id from Passport where applicant_id like '{appid}' ";

            SqlCommand command = new SqlCommand(quertystring, db.GetConection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count >= 2)
            {
                MessageBox.Show("Больше двух паспортов на одного заявителя быть не может!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return true;
            }
            else
            {
                return false;
            }
        }
        //проверка-исключение добавления паспорта к несуществующему заявителю
        private Boolean NoIDPass()
        {
            var noappid = apid.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string quertystring = $"select name, date_of_birth, gender, address, phone_number from Applicant where id = '{noappid}'";
            
            SqlCommand command = new SqlCommand(quertystring, db.GetConection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Заявителя с таким ID не существует!", $"ОШИБКА!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return true;
            }
            else
            {
                return false;
            }
        }

       
        //автодобавление 10 лет к срокам истечения действия паспорта при изменении даты выдачи
        private void issue_ValueChanged(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Tick += (s, ev) =>
            {
                DateTime selectedDate = issue.Value;
                DateTime newDate = selectedDate.AddYears(10);
                expiration.Value = newDate;
                timer.Stop();
            };
            timer.Start();
        }

        //автодобавление 10 лет к срокам истечения действия паспорта при изменении даты выдачи в вкладке изменения паспорта
        private void issue_chan_ValueChanged(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Tick += (s, ev) =>
            {
                DateTime selectedDate = issue_chan.Value;
                DateTime newDate = selectedDate.AddYears(10);
                expiration_chan.Value = newDate;
                timer.Stop();
            };
            timer.Start();
        }

        //кнопка выхода из приложения
        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //кнопка информации
        private void info_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Помните!\n\nПри удалении заявителя удаляются все паспорта оформленные на него! \n\nБудьте внимательны в вводе данных! \n\nДля изменения данных кликните по строке несколько раз(желательно даже больше чем несколько!).",
             "Ответы на частые вопросы", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        }
    }


}

