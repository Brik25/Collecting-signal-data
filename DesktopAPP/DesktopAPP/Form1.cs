using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MetroFramework.Components;
using MetroFramework.Forms;
using System.IO;
using System.Net;
using System.Data.SQLite;





namespace DesktopAPP
{
    public partial class Form1 : MetroForm
    {
        int computation_interrupt = 0;
        int max_value;

        SQLiteConnection conn = new SQLiteConnection("Data Source=test.db;Version=3;");
        SQLiteDataReader re;

        private Mutex db_mtx = new Mutex();

        
        struct Params
        {
            public int chan_num;
            public int dac;
            public int dac_step;
            public int start_mode;
            public int pulse_type;
            public int sync_type;
            public int conn_type;
            public int input_voltage_1;
            public int input_voltage_2;
            public int input_voltage_3;
            public int input_voltage_4;
            public string frequency;
            public int frequency2;
            public int usb_type;
            public string datadir;
            public string fprefix;
            public string datafile;
        }
        protected void init_db()
        {
            SQLiteConnection.CreateFile("test.db");
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(conn);

            string sql_create =

                         "CREATE TABLE Start (" +
                            "idStart integer primary key, " +
                            "start_name varchar(45) NOT NULL" +
                         ");" +
                         "CREATE TABLE Napr (idNapr integer primary key, znach_nap integer NOT NULL);" +
                         "CREATE TABLE Impul (idImpul integer primary key, name_impu varchar(45) NOT NULL);" +
                         "CREATE TABLE Podkl (idPodkl integer primary key, name_podkl varchar(45) NOT NULL);" +
                         "CREATE TABLE Syncho (idSyncho integer primary key, synch_name varchar(45) NOT NULL);" +
                         "CREATE TABLE file (idfile integer primary key, name varchar(45) NOT NULL,data BLOB);" +
                         "CREATE TABLE chast (idchast integer primary key, znach_chast integer NOT NULL);" +
                         "CREATE TABLE channel (idchannel integer primary key, chan_znach integer NOT NULL);" +

                         "CREATE TABLE info_table (" +
                         "id integer primary key AUTOINCREMENT," +
                         "Napr_idNapr integer NOT NULL REFERENCES Napr(idNapr)," +
                         "Impul_idImpul integer NOT NULL REFERENCES Impul(idImpul)," +
                         "channel_idchannel integer NOT NULL REFERENCES channel(idchannel)," +
                         "chast_idchast integer NOT NULL REFERENCES chast(idchast)," +
                         "Start_idStart integer NOT NULL REFERENCES Start(idStart)," +
                         "Syncho_idSyncho integer NOT NULL REFERENCES Syncho(idSyncho)," +
                         "Podkl_idPodkl integer NOT NULL  REFERENCES Podkl(idPodkl)," +
                         "speed_idspeed integer NOT NULL  REFERENCES speed(idspeed)," +
                         "Cap integer NOT NULL," +
                         "file_idfile integer NOT NULL REFERENCES file(idfile)" +
                         ");";

            cmd.CommandText = sql_create;
            cmd.ExecuteNonQuery();

            string insert =

                //параметры Режима старта
                "insert into Start(idStart,start_name) values (0,\"Внутренний старт\");" +
                "insert into Start(idStart,start_name) values (1,\"Внутренний старт с трансляцией\");" +
                "insert into Start(idStart,start_name) values (2,\"Внешний старт по фронту\");" +
                "insert into Start(idStart,start_name) values (3,\"Внешний старт по спаду\");" +
                //параметры аналоговой синхронизации
                "insert into Syncho(idSyncho,synch_name) values (0,\"Отсутствие\");" +
                "insert into Syncho(idSyncho,synch_name) values (1,\"по переходу вверх\");" +
                "insert into Syncho(idSyncho,synch_name) values (2,\"по переходу вниз\");" +
                "insert into Syncho(idSyncho,synch_name) values (3,\"по уровню выше\");" +
                "insert into Syncho(idSyncho,synch_name) values (4,\"по уровню ниже\");" +
                //параметры тактовых импульсов
                "insert into Impul(idImpul,name_impu) values (0,\"Внутренние\");" +
                "insert into Impul(idImpul,name_impu) values (1,\"Внутренние с трансляцией\");" +
                "insert into Impul(idImpul,name_impu) values (2,\"Внешние по фронту\");" +
                "insert into Impul(idImpul,name_impu) values (3,\"Внешние по спаду\");" +
                //параметры типа подключения
                "insert into Podkl(idPodkl,name_podkl) values (0,\"Заземленный канал АЦП модуля\");" +
                "insert into Podkl(idPodkl,name_podkl) values (1,\"Подача выходного сигнала на вход АЦП модуля\");" +
                //параметры входного напряжения
                "insert into Napr(idNapr,znach_nap) values (0,3000);" +
                "insert into Napr(idNapr,znach_nap) values (1,1000);" +
                "insert into Napr(idNapr,znach_nap) values (2,300);" +
                //параметры частоты работы
                "insert into chast(idchast,znach_chast) values (0,1000);" +
                "insert into chast(idchast,znach_chast) values (1,2000);" +
                "insert into chast(idchast,znach_chast) values (2,3000);" +
                "insert into chast(idchast,znach_chast) values (3,4000);" +
                "insert into chast(idchast,znach_chast) values (4,5000);" +
                //параметры колличество каналов
                "insert into channel(idchannel,chan_znach) values (1,1);" +
                "insert into channel(idchannel,chan_znach) values (2,2);" +
                "insert into channel(idchannel,chan_znach) values (3,3);" +
                "insert into channel(idchannel,chan_znach) values (4,4);";

            cmd.CommandText = insert;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public Form1()
        {
            InitializeComponent();

            if (!File.Exists("test.db"))
            {
                init_db();
            }
            conn.Open();

            SQLiteCommand command_napr = new SQLiteCommand("select znach_nap from Napr", conn);
            re = command_napr.ExecuteReader();
            while (re.Read())
            {
                metroComboBox1.Items.Add(re.GetValue(0).ToString());         
                metroComboBox2.Items.Add(re.GetValue(0).ToString());
                metroComboBox3.Items.Add(re.GetValue(0).ToString());
                metroComboBox4.Items.Add(re.GetValue(0).ToString());
            }

            SQLiteCommand command_chast = new SQLiteCommand("select znach_chast from chast", conn);
            re = command_chast.ExecuteReader();
            while (re.Read())
            {
                metroComboBox5.Items.Add(re.GetValue(0).ToString());         
            }

            SQLiteCommand command_start = new SQLiteCommand("select start_name from Start", conn);
            re = command_start.ExecuteReader();
            while (re.Read())
            {
                metroComboBox6.Items.Add(re.GetValue(0).ToString());         
            }

            SQLiteCommand command_analog = new SQLiteCommand("select synch_name from Syncho", conn);
            re = command_analog.ExecuteReader();
            while (re.Read())
            {
                metroComboBox7.Items.Add(re.GetValue(0).ToString());        
            }

            SQLiteCommand command_impul = new SQLiteCommand("select name_impu from Impul", conn);
            re = command_impul.ExecuteReader();
            while (re.Read())
            {
                metroComboBox8.Items.Add(re.GetValue(0).ToString());        
            }

            SQLiteCommand command_podkl = new SQLiteCommand("select name_podkl from Podkl", conn);
            re = command_podkl.ExecuteReader();
            while (re.Read())
            {
                metroComboBox9.Items.Add(re.GetValue(0).ToString());         
            }
            //UpdataMena();
            conn.Close();
        }


     private void Form1_Load(object sender, EventArgs e)
        {
            metroComboBox1.SelectedIndex = 0;
            metroComboBox2.SelectedIndex = 0;
            metroComboBox3.SelectedIndex = 0;
            metroComboBox4.SelectedIndex = 0;
            metroComboBox5.SelectedIndex = 0;
            metroComboBox6.SelectedIndex = 0;
            metroComboBox7.SelectedIndex = 0;
            metroComboBox8.SelectedIndex = 0;
            metroComboBox9.SelectedIndex = 1;
        }

        private Params get_params()
        {
            Params prms = new Params();
            this.Invoke(new Action(() =>
            {
                prms.input_voltage_1 = metroComboBox1.SelectedIndex;
                prms.input_voltage_2 = metroComboBox2.SelectedIndex;
                prms.input_voltage_3 = metroComboBox3.SelectedIndex;
                prms.input_voltage_4 = metroComboBox4.SelectedIndex;
                prms.frequency = metroComboBox5.Text;
                prms.frequency2 = metroComboBox5.SelectedIndex;

                //prms.chan_num = int.Parse(metroComboBox4.Text);

                prms.start_mode = metroComboBox6.SelectedIndex;
                prms.sync_type = metroComboBox7.SelectedIndex;
                prms.pulse_type = metroComboBox8.SelectedIndex;
                prms.conn_type = metroComboBox9.SelectedIndex;

                prms.dac = Convert.ToInt32(metroTextBox1.Text); //Цап
                prms.dac_step = Convert.ToInt32(numericUpDown1.Value); //шаг
            }));

            return prms;
        }

    }
}
