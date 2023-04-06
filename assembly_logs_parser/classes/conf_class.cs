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
        private DateTime _start_time; // время начала сектора
        private DateTime _stop_time; //время завершения селектора
        private string _manager_name = "Планировщик";

        private int _real_duration; // фактическая продолжительность
        private int _real_participants_count; // фактическое кол-во участников
        private int _seted_duration; // заданная продолжительность
        private int _seted_participants_count; // заданное кол-во участников  (NP_MAX_CH)
        private int _seted_can_speak_participants_count; // могут говорить (NP_MAX_DSP)

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


        public string save_seted_params
        {
            set 
            {
                //  (ID:01-0208 VSPThread:CONF(1-64))->Conference 1-64 collection requested NP_MAX_CH=29, NP_MAX_DSP=31, duartion=5400 sec
                string[] conf_params_line = value.Split(' ');
                _seted_participants_count = Convert.ToInt32(conf_params_line[5].Split('=')[1].Replace(',', ' ').Trim());                 //[5] NP_MAX_CH=29,
                _seted_can_speak_participants_count = Convert.ToInt32(conf_params_line[6].Split('=')[1].Replace(',', ' ').Trim());         //[6] NP_MAX_DSP=31,
                _seted_duration = Convert.ToInt32(conf_params_line[7].Split('=')[1].Split(' ')[0]) / 60;   //[7] duartion=5400 sec
            }
        }

        public string seted_participants_count
        {
            get { return _seted_participants_count.ToString(); }
        }

        public string seted_can_speak_participants_count
        {
            get { return _seted_can_speak_participants_count.ToString(); }
        }
        public string seted_duration
        {
            get { return _seted_duration.ToString(); }
        }


    }
}
