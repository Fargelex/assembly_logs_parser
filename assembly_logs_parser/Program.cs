using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser
{
    internal class Program
    {

        #region Структура БД ассамблеи
        /* 
        [STAT_VSP_CONF] - статистика конференции
            SEANCE_NO=int
            CLUSTER_ID=int
            SCHEME_ID=int
            START_TYPE_ID=int
            DT_BEGIN=timestamp (ex. - 1671868800)
            DT_END=timestamp (ex. - 1671877734)
            DT_COLLECTION=timestamp (ex. - 1671868822)
            STARTUSER_ID#
            STARTUSER_NAME#
            IDENTIFY_CODE=int
            FULL_DURATION=int
            WORK_DURATION=int
            CLUSTER_NAME=Новый кластер
            SCHEME_NAME=string (ex. - Название селектора)
            CONTRACT_NO=
            RESERVED_CH=int
            RESERVED_DSP=int
            RESERVED_TIME=int
            IS_PLANCONF=int
            FINISH_REASON=int
            FINISH_USER_ID#
            FINISH_USER_NAME# 
        */

        /*
        [STAT_VSP_CONF_PLAN] - планировщик
            CLUSTER_ID=int
            ID=int
            MODIFY_NO=int
            DT_ACTION=timestamp (ex. - 1671953100)
            DT_BEGIN=timestamp (ex. - 1671953100)
            DT_END=timestamp (ex. - 1671954600)
            DSP_RES_ID=int
            CH_CNT=int
            DSPRES_CNT=int
            SCHEME_ID=int
            AUTO_START=int
            SERIA_ID=int
            IS_MODIFIED_IN_SERIA=int
            CLUSTER_NAME=Новый кластер
            SCHEME_NAME=string (ex. - Название селектора)
            ACTION_TYPE=int
            TASK_ID=int
            USER_LOGIN#
            STAT_SEANCE_NO = int
            STAT_RESULT=int
            ACTUAL_CH_CNT=int
            ACTUAL_DSPRES_CNT=int
        */
        /*
        [VSP_CONF_SCHEMES]
            CLUSTER_ID=int
            ID=int
            NAME=string (ex. - Название селектора)
            IDENTIFY_CODE = 01
            NP_MAX_CH=int
            NP_MAX_DSP=int
            NP_MAX_TIME=int
        */
        #endregion

        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };
        static Dictionary<string, List<string>> data_base_dictionary = new Dictionary<string, List<string>> { };

        static Dictionary<string, List<string>> VSP_CONF_SCHEMES = new Dictionary<string, List<string>> { };
        static string data_base_filename = "";

        static void Main(string[] args)
        {
            load_settings();
            load_data_base();
            Console.ReadKey();
        }

        private static void load_data_base()
        {
            string[] data_rtdb = File.ReadAllLines(Path.GetFullPath(settings_dictionary["data_base"].First()));
            string last_key = "";
            foreach (string db_string in data_rtdb)
            {
                if (db_string != "")
                {
                    if (db_string[0] == '[')
                    {
                        last_key = db_string.Split(']')[0].Replace('[', ' ').Trim();
                        if (!data_base_dictionary.ContainsKey(last_key))
                        {
                            data_base_dictionary.Add(last_key, new List<string> { });
                        }
                    }
                    else
                    {
                        data_base_dictionary[last_key].Add(db_string.Trim());
                    }
                }
                else
                {
                    data_base_dictionary[last_key].Add(db_string.Trim());
                }
            }
        }


        private static void load_VSP_CONF_SCHEMES()
        {
            foreach (string item in data_base_dictionary["VSP_CONF_SCHEMES"])
            {

            }
        }


            private static void add_to_main_log(string text_log, bool add_to_text_log = true)
        {
            string log_string = '[' + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]\t" + text_log.Trim();
            Console.WriteLine(log_string);
            if (add_to_text_log)
            {
                File.AppendAllText("assembly_logs_parser.log", log_string + "\r\n");
            }
        }

        private static void load_settings()
        {
            List<string> fields_list = new List<string> { };
            if (File.Exists("assembly_logs_parser_settings.ini"))
            {
                string[] settings_file_lines = File.ReadAllLines("assembly_logs_parser_settings.ini");
                string last_key = "";
                foreach (string settings_file_line in settings_file_lines)
                {
                    if (settings_file_line[0] == '[')
                    {
                        last_key = settings_file_line.Split(']')[0].Replace('[', ' ').Trim();
                        if (!settings_dictionary.ContainsKey(last_key))
                        {
                            settings_dictionary.Add(last_key, new List<string> { });
                        }
                    }
                    else
                    {
                        settings_dictionary[last_key].Add(settings_file_line.Trim());
                    }
                }
                if (!Directory.Exists(settings_dictionary["logs_path"].First()))
                {
                    add_to_main_log("в файле assembly_logs_parser_settings.ini не корректно указан путь к папке с файлами логов");
                    add_to_main_log(Path.GetFullPath(settings_dictionary["logs_path"].First()), false);
                }

                if (!File.Exists(settings_dictionary["data_base"].First()))
                {
                    add_to_main_log("в файле assembly_logs_parser_settings.ini указан путь к папке в которой отсутстует файл базой данных - data.rtdb");
                    add_to_main_log(Path.GetFullPath(settings_dictionary["data_base"].First()), false);
                }

                data_base_filename = Path.GetFileName(settings_dictionary["data_base"].First());

            }
            else
            {
                //  add_to_main_log("info\tфайл с настройками settings.ini отсутствует, создаю файл по умолчанию");
                string[] default_settings = new string[] {
                "[logs_path] путь к папке с файлами логов селекторов" ,
                    "logs",
                "[data_base] путь к базе данных",
                    @"db\data.rtdb",
                };
                File.WriteAllLines("assembly_logs_parser_settings.ini", default_settings);
                load_settings();
            }
        }

    }
}
