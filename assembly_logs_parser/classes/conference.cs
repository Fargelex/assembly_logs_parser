using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    internal class conference
    {
        private int _clasterID = 1; // уникальный ID кластера
        private int _ID; // уникальный ID селектора (задаётся автоматически, нельзя поменять)
        private string _name; // имя селектора
        private int _identify_code; // код идентификации (можно задать вручную, можно менять) (IDENTIFY_CODE)
        private int _participants_count; // кол-во участников (NP_MAX_CH)
        private int _can_speak_participants_count; // могут говорить (NP_MAX_DSP)
        private int _duration; // продолжительность (NP_MAX_TIME)

        public conference(int clasterID, int ID, string name, int identify_code, int participants_count, int can_speak_participants_count, int duration)
        {
            _clasterID = clasterID;
            _ID = ID;
            _name = name;
            _identify_code = identify_code;
            _participants_count = participants_count;
            _can_speak_participants_count = can_speak_participants_count;
            _duration = duration;
        }

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Participants_count
        {
            get { return _participants_count; }
            set { _participants_count = value; }
        }

        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
    }

}