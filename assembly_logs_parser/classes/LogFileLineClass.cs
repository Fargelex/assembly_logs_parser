using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    internal class LogFileLineClass
    {    

        private uint _confId;
        private bool _startedConferenceLine = false;
        private string _fullLogFileLine;
        private DateTime _startTimeConference;




        public LogFileLineClass(string inputLogFileLine)
        {
            Match confIDMatch = new Regex(@"VSPThread:CONF\(\d*-\d*").Match(inputLogFileLine); //"VSPThread:CONF(1-203" 
            if (confIDMatch.Success)
            {
                _confId = Convert.ToUInt32(confIDMatch.Value.Split('-')[1].Trim()); //делим строку  "VSPThread:CONF(1-203" через дефис и сохраняем ID селектора
            }

            // признаки запуска конференции
            // L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77

            Match schedulerConfStartMatch = new Regex(@"[Rr]un conference: ClusterId=\d* Id=\d* SchemeId=\d*").Match(inputLogFileLine); //начало конференции по планировщику
            if (schedulerConfStartMatch.Success)
            {
                //  L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77
                // делим по знаку равно, последний элемент массива - номер ID селектора
                _confId = Convert.ToUInt32(schedulerConfStartMatch.Value.Split('=').Last().Trim());
                _startedConferenceLine = true;
                //    valid_line = true;
            }

            //L200[01.03.2023 00:00:47-681](ID:01-0208 VSPThread:CDISP)->SNC(3365447-1). Party ID=27 (Subscriber ID=4138) of conference ID=402 identified by phone=431760@10.8.5.18
            if (inputLogFileLine.Contains("identified by phone="))
            {
                Regex conf_ID_started_by_phone_regex = new Regex(@"conference id=\d*");
                Match conf_ID_started_by_phone_match = conf_ID_started_by_phone_regex.Match(inputLogFileLine.ToLower()); //строчка содержит признак начала конференции по телефону
                if (conf_ID_started_by_phone_match.Success)
                {
                    conf_id = conf_ID_started_by_phone_match.Value.Split('=')[1].Trim();
                }
                started_conference_line = true;
            }


        } //public LogFileLineClass

    } //internal class LogFileLineClass

} // namespace assembly_logs_parser.classes
