using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using MetroFramework.Forms;

namespace DesktopAPP
{
    public partial class Form2 : MetroForm
    {

        SQLiteConnection conn = new SQLiteConnection("Data Source=test.db;Version=3;");
        SQLiteCommand cmd = new SQLiteCommand();
        
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            update_info();
            if (metroGrid1.Rows.Count == 0)
            {
                metroButton3.Enabled = false;
                metroButton2.Enabled = false;
            }
            else
            {
                metroButton3.Enabled = true;
                metroButton2.Enabled = true;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            string insert_com = "insert into Com_ports (commands) values ('"+ metroTextBox1.Text+"');";
            cmd.CommandText = insert_com;
            cmd.ExecuteNonQuery(); 
            conn.Close();
            update_info();

            if (metroGrid1.Rows.Count == 1)
                metroButton3.Enabled = true;
                metroButton2.Enabled = true;
            
          

        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            string id = metroGrid1.CurrentRow.Cells[0].Value.ToString();
            string upd_com = "update Com_ports set commands= '" + metroTextBox1.Text+"' where id = '"+ id+"'";
            cmd.CommandText = upd_com;
            cmd.ExecuteNonQuery();
            conn.Close();
            update_info();
         
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {    
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                string id = metroGrid1.CurrentRow.Cells[0].Value.ToString();
                string delete = "Delete FROM Com_ports where id = '" + id + "'";
                cmd.CommandText = delete;
                cmd.ExecuteNonQuery();
                conn.Close();
                update_info();

             if (metroGrid1.Rows.Count == 0)
             {
                metroButton3.Enabled = false;
                metroButton2.Enabled = false;
            }
            else
            {
                metroButton3.Enabled = true;
                metroButton2.Enabled = true;
            }
        }

        private void update_info()
        {
            conn.Open();
            metroGrid1.Rows.Clear();
            string info = "Select * from Com_ports";
            
            SQLiteCommand cmd = new SQLiteCommand(info,conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            List<string[]> data = new List<string[]>();
            while (reader.Read())
            {
                data.Add(new string[2]);
                data[data.Count - 1][0] = reader[0].ToString();
                data[data.Count - 1][1] = reader[1].ToString();               
            }
            foreach (string[] s in data)
            {
                metroGrid1.Rows.Add(s);
            }
            
            conn.Close();
        }

      
    }
}
