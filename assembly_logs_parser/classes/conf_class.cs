using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    internal class conf_class
    {
        private int _ID; // уникальный ID селектора
        private string _pocessed_file_path; // путь в который сохранять строчки лога
        private DateTime _start_time; // время начала сектора
        private DateTime _stop_time = new DateTime(1990, 01, 01, 00, 00, 00); //время завершения селектора
        private string _manager_name = "Планировщик";

        private TimeSpan _real_duration; // фактическая продолжительность
        private int _real_participants_count; // фактическое кол-во участников
        private TimeSpan _seted_duration; // заданная продолжительность
        private int _seted_participants_count; // заданное кол-во участников  (NP_MAX_CH)
        private int _seted_can_speak_participants_count; // могут говорить (NP_MAX_DSP)



        private string _error_discription ="";
        private bool _is_error = false;

        public string _stop_reason = "Отсутствие участников";

        public conf_class(string cnf_ID)
        {
            try
            {
                _is_error = false;
                _ID = Convert.ToInt32(cnf_ID);
            }
            catch (Exception ex)
            {
                _is_error = true;
                _error_discription = String.Format("В конструктор conf_class должен быть передан ID селектора (string с числом) для Convert.ToInt32, а передано [{0}] | сообщение об ошибке [{1}]", cnf_ID, ex.Message);
             //   throw;
            }
          
        }


        public string stop_reason
        {
            get { return _stop_reason; }
            set { _stop_reason = value; }
        }



        public TimeSpan real_duration
        {
            get { return _real_duration; }
        }

        public int real_participants_count
        {
            get { return _real_participants_count; }
            set { _real_participants_count = value; }
        }

        public bool is_error
        {
            get { return _is_error; }
        }

        public string error_discription
        {
            get { return _error_discription; }
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
            get { return _start_time.ToString("yyyy.MM.dd HH:mm:ss"); }
            set { _start_time = Convert.ToDateTime(value); }
        }

        public string stop_time
        {
            get { return _stop_time.ToString("HH:mm:ss"); }
            set
            {
                _stop_time = Convert.ToDateTime(value);
                if (_stop_time > _start_time)
                {
                    _real_duration = _stop_time - _start_time;
                }
            }
        }

        public bool check_start_time(string check_date)
        {
            DateTime vl = Convert.ToDateTime(check_date);

            if (vl == _start_time)
            {
                return true;
            }
            else
                return false;
        }

        public string save_seted_params
        {
            set 
            {
                try
                {
                    _is_error = false;
                    _error_discription = "";
                    //  (ID:01-0208 VSPThread:CONF(1-64))->Conference 1-64 collection requested NP_MAX_CH=29, NP_MAX_DSP=31, duartion=5400 sec
                    string[] conf_params_line = value.Split(' ');
                    _seted_can_speak_participants_count = Convert.ToInt32(conf_params_line[5].Split('=')[1].Replace(',', ' ').Trim());         //[5] NP_MAX_CH=29, 
                    _seted_participants_count = Convert.ToInt32(conf_params_line[6].Split('=')[1].Replace(',', ' ').Trim());                //[6] NP_MAX_DSP=31,

                    _seted_duration = TimeSpan.FromSeconds(Convert.ToDouble(conf_params_line[7].Split('=')[1].Split(' ')[0]));   //[7] duartion=5400 sec
                }
                catch (Exception ex)
                {
                    _is_error = true;
                    _error_discription = String.Format("Ошибка при обработке строки [{0}] | Ошибка [{1}]", value, ex.Message);
                 //   throw;
                }
               
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
