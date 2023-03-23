using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    internal class conference
    {
        private DateTime _date; // дата проведения селектора
        private int _ID; // уникальный ID селектора
        private string _name; // имя селектора
        private DateTime _start_time; // время начала сектора
        private DateTime _stop_time; //время завершения селектора
        private int _real_duration; // фактическая продолжительность
        private int _real_participants_count; // фактическое кол-во участников
        private string _manager_name; // кто запустил селектор
        private int _seted_duration; // заданная продолжительность
        private int _seted_participants_count; // заданное кол-во участников

        public conference(string Date, string ID, string Name, string start_time, string stop_time, string real_duration,
            string real_participants_count, string seted_duration, string seted_participants_count, string manager_name = "Планировщик")
        {
            _date = Convert.ToDateTime(Date);
            _ID = Convert.ToInt32(ID);
            _name = Name;
            _start_time = Convert.ToDateTime(start_time);
            _stop_time = Convert.ToDateTime(stop_time);
            _real_duration = Convert.ToInt32(real_duration);
            _real_participants_count = Convert.ToInt32(real_participants_count);
            _manager_name = manager_name;
            _seted_duration = Convert.ToInt32(seted_duration);
            _seted_participants_count = Convert.ToInt32(seted_participants_count);
        }

    }
}
