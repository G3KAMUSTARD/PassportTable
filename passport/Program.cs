﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace passport
{
    internal class DB
    {
        SqlConnection SqlConnection = new SqlConnection(@"Data Source=510-013\SQLEXPRESS;Initial Catalog=pasport;Integrated Security=True");

        public void OpenConection()
        {
            if (SqlConnection.State == System.Data.ConnectionState.Closed)
            {
                SqlConnection.Open();
            }
        }
        public void CloseConection()
        {
            if (SqlConnection.State == System.Data.ConnectionState.Open)
            {
                SqlConnection.Close();
            }
        }
        public SqlConnection GetConection()
        {
            return SqlConnection;
        }

        public string StrCon()
        {
            return @"Data Source=510-013\SQLEXPRESS;Initial Catalog=pasport;Integrated Security=true";
        }


        public SqlDataAdapter queryEx(string query)
        {
            try
            {
                SqlConnection myCon = new SqlConnection(StrCon());
                myCon.Open();
                SqlDataAdapter SDA = new SqlDataAdapter(query, myCon);
                SDA.SelectCommand.ExecuteNonQuery();
                MessageBox.Show("Успешно");
                return SDA;
            }
            catch
            {
               
                return null;
            }
        }
        static class Program
        {

            /// <summary>
            /// Главная точка входа для приложения.
            /// </summary>
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form2());
            }
        }
    }

}
