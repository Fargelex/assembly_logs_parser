using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
        private string _managerNameStartedConference;
        private uint _seanceID;
        private uint _subscriberID;
        public bool hasError { get; set; }
        public string errorMessage { get; set; }




        public LogFileLineClass(string inputLogFileLine)
        {
            hasError = false;
            Match confIDMatch = new Regex(@"VSPThread:CONF\(\d*-\d*").Match(inputLogFileLine); //"VSPThread:CONF(1-203" 
            if (confIDMatch.Success)
            {
                _confId = Convert.ToUInt32(confIDMatch.Value.Split('-')[1].Trim()); //делим строку  "VSPThread:CONF(1-203" через дефис и сохраняем ID селектора
            }

            // признаки запуска конференции
            
            //начало конференции по планировщику
            Match schedulerConfStartMatch = new Regex(@"[Rr]un conference: ClusterId=\d* Id=\d* SchemeId=\d*").Match(inputLogFileLine); // L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77
            if (schedulerConfStartMatch.Success)
            {
                //  L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77
                // делим по знаку равно, последний элемент массива - номер ID селектора
                _confId = Convert.ToUInt32(schedulerConfStartMatch.Value.Split('=').Last().Trim());
                _startedConferenceLine = true;
                _startTimeConference = getTimeFromLogFileLine(inputLogFileLine);
                //    valid_line = true;
            }

            
            if (inputLogFileLine.Contains("identified by phone=")) //L200[01.03.2023 00:00:47-681](ID:01-0208 VSPThread:CDISP)->SNC(3365447-1). Party ID=27 (Subscriber ID=4138) of conference ID=402 identified by phone=431760@10.8.5.18
            {
                Regex conf_ID_started_by_phone_regex = new Regex(@"conference id=\d*");
                Match conf_ID_started_by_phone_match = conf_ID_started_by_phone_regex.Match(inputLogFileLine.ToLower()); //строчка содержит признак начала конференции по телефону
                if (conf_ID_started_by_phone_match.Success)
                {
                    _confId = Convert.ToUInt32(conf_ID_started_by_phone_match.Value.Split('=')[1].Trim());
                }
                _startedConferenceLine = true;
                _startTimeConference = getTimeFromLogFileLine(inputLogFileLine);

                _managerNameStartedConference = inputLogFileLine.Split('=').Last(); //431760@10.8.5.18
                _managerNameStartedConference = _managerNameStartedConference.Split('@')[0];

                Match subscriberIDMatch = new Regex(@"Subscriber ID=\d*").Match(inputLogFileLine);
                if (subscriberIDMatch.Success)
                {
                    _subscriberID = Convert.ToUInt32(subscriberIDMatch.Value.Split('=').Last());
                }
                Match sncIDMatch = new Regex(@"->SNC\(\d*").Match(inputLogFileLine);
                if (sncIDMatch.Success)
                {
                    _seanceID = Convert.ToUInt32(sncIDMatch.Value.Split('(').Last());
                }

            }

            //строчка содержит признак начала конференции в ручную администратором
            if (inputLogFileLine.Contains("Login") && inputLogFileLine.Contains("started conference"))  //"L120[04.01.2021 16:58:31-318](ID:01-0208 VSPThread:CONF(1-203))->Login disp(1-666) started conference"
            {
                _startedConferenceLine = true;
                _startTimeConference = getTimeFromLogFileLine(inputLogFileLine);

                _managerNameStartedConference = inputLogFileLine.Split('>').Last(); //Login disp(1-666) started conference
                _managerNameStartedConference = _managerNameStartedConference.Split('(')[0];//Login disp
                _managerNameStartedConference = _managerNameStartedConference.Split(' ').Last();
            }


        } //public LogFileLineClass



        private DateTime getTimeFromLogFileLine(string inputLogFileLine)
        {
            string tmpString = inputLogFileLine.Split(']')[0];//  L120[19.02.2021 07:00:00-620
            tmpString = tmpString.Split('[')[1]; //19.02.2021 07:00:00-620
            tmpString = tmpString.Replace('-', '.');
            DateTime dt = new DateTime(2000, 1, 1, 00, 00, 33);
            try
            {
                dt = Convert.ToDateTime(tmpString);
                return dt;
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = String.Format("Ошибка в преобразовании даты из String в DateTime. Строка [{0}] | Текст Exeption [{1}]", inputLogFileLine, ex.Message);
                return dt;
              //  throw;
            }           
            

        } //getTimeFromLogFileLine

    } //internal class LogFileLineClass

} // namespace assembly_logs_parser.classes
