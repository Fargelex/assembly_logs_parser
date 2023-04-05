using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    internal class conf_class
    {
        private int _ID; // уникальный ID селектора
        private string _pocessed_file_path; // путь в который сохранять строчки лога
        private int _participants_count; // кол-во участников (NP_MAX_CH)
        private int _duration; // продолжительность (NP_MAX_TIME)
        private DateTime _start_time; // время начала сектора
        private DateTime _stop_time; //время завершения селектора
        private string _manager_name = "Планировщик";


        public conf_class(string cnf_ID)
        {
            _ID = Convert.ToInt32(cnf_ID);
        }


        public int ID
        {
            get { return _ID; }
        }

        public string year
        {
            get { return _start_time.Year.ToString(); }
        }

        public string pocessed_file_path
        {
            get { return _pocessed_file_path; }
            set { _pocessed_file_path = value; }
        }

        public string manager_name
        {
            get { return _manager_name; }
            set { _manager_name = value; }
        }

        public string start_time
        {
            get { return _start_time.ToString("yyyy.MM.dd HH.mm.ss"); }
            set { _start_time = Convert.ToDateTime(value); }
        }

        public string stop_time
        {
            get { return _stop_time.ToString("HH.mm.ss"); }
            set { _stop_time = Convert.ToDateTime(value); }
        }


    }
}
