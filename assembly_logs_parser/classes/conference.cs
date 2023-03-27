using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    /*
       [VSP_CONF_SCHEMES]
           CLUSTER_ID=int
           ID=int
           NAME=string (ex. - Название селектора)
           IDENTIFY_CODE =int
           NP_MAX_CH=int
           NP_MAX_DSP=int
           NP_MAX_TIME=int
       */
    internal class conference
    {
        private int _clasterID = 1; // уникальный ID кластера
        private int _ID; // уникальный ID селектора (задаётся автоматически, нельзя поменять)
        private string _name; // имя селектора
        private int _identify_code; // код идентификации (можно задать вручную, можно менять) (IDENTIFY_CODE)
        private int _participants_count; // кол-во участников (NP_MAX_CH)
        private int _can_speak_participants_count; // могут говорить (NP_MAX_DSP)
        private int _duration; // продолжительность (NP_MAX_TIME)

        public conference(List<string> values_list)
        {
            _clasterID = Convert.ToInt32(values_list[0].Split('=')[1]);
            _ID = Convert.ToInt32(values_list[1].Split('=')[1]);
            _name = values_list[2].Split('=')[1];
            _identify_code = Convert.ToInt32(values_list[3].Split('=')[1]);
            _participants_count = Convert.ToInt32(values_list[4].Split('=')[1]);
            _can_speak_participants_count = Convert.ToInt32(values_list[5].Split('=')[1]);
            _duration = Convert.ToInt32(values_list[6].Split('=')[1]);
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