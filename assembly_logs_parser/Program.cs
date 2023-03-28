using assembly_logs_parser.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            IDENTIFY_CODE =int
            NP_MAX_CH=int
            NP_MAX_DSP=int
            NP_MAX_TIME=int
        */
        #endregion

        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };


        static Dictionary<int, conference> conferences = new Dictionary<int, conference> { };


        static string data_base_filename = "";
        static string processed_log_files = Path.GetFullPath("assemblylogsparser_temp_folder") + "\\processed_log_files.txt";

        static void Main(string[] args)
        {
            if (load_settings())
            {
                scan_logs();
            }
            
            //  load_data_base();
            
            Console.ReadKey();
        }

        private static void load_data_base()
        {
            Dictionary<string, List<List<string>>> data_base_dictionary = new Dictionary<string, List<List<string>>> { };

            string[] data_rtdb = File.ReadAllLines(Path.GetFullPath(settings_dictionary["data_base"].First()));
            string last_key = "";
            List<string> data = new List<string>(); // значения для одной записи
            foreach (string db_string in data_rtdb)
            {
                if (db_string != "") // записи в БД разделены пустой строкой
                {
                    if (db_string[0] == '[') // название таблицы в квадратных скобках
                    {
                        last_key = db_string.Split(']')[0].Replace('[', ' ').Trim(); // запоминаем название таблицы и делаем его ключом в словаре
                        if (!data_base_dictionary.ContainsKey(last_key)) // если такого ключа нет, добавлем ключ и пустой список значений
                        {
                            data_base_dictionary.Add(last_key, new List<List<string>> { });
                        }
                    }
                    else
                    {
                        data.Add(db_string.Trim());
                        //      data_base_dictionary[last_key].Add(db_string.Trim());
                    }
                }
                else
                {
                    data_base_dictionary[last_key].Add(data);
                    data = new List<string>();
                }
            }

            foreach (List<string> item in data_base_dictionary["VSP_CONF_SCHEMES"])
            {
                conference conf = new conference(item);
                if (!conferences.ContainsKey(conf.ID))
                {
                    conferences.Add(conf.ID, conf);
                }
                else
                {
                    add_to_main_log("в словаре конференций одинаковые ID: [" + conf.ID.ToString() + " " + conf.Name + "] и [" + conferences[conf.ID].ID.ToString() + " " + conferences[conf.ID].Name);
                }
            }
        }



        private static void add_to_main_log(string text_log, bool add_to_text_log = true)
        {
            string log_string = '[' + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]\t" + text_log.Trim();
            Console.WriteLine(log_string);
            if (add_to_text_log)
            {
                File.AppendAllText("assembly_logs_parser_log.log", log_string + "\r\n");
            }
        }

        private static bool load_settings()
        {
            bool no_errors = true;
            if (!Directory.Exists("селектора")) // папка в которую будут сохраняться селектора выбранные из лог файлов
            {
                add_to_main_log("создаю папку 'селектора' в " + Path.GetFullPath("селектора"));
                Directory.CreateDirectory("селектора");
            }
            if (!Directory.Exists("assemblylogsparser_temp_folder")) // папка в которую будет сохраняться рабочая инфа программы
            {
                add_to_main_log("создаю папку 'assemblylogsparser_temp_folder' в " + Path.GetFullPath("assemblylogsparser_temp_folder"));
                Directory.CreateDirectory("assemblylogsparser_temp_folder");
                
                add_to_main_log("создаю файл 'processed_log_files.txt' в " + processed_log_files, false);
                File.AppendAllText(processed_log_files, "# logs файлы ассамблеи, которые были обработаны программой" + "\r\n");
            }

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
                    add_to_main_log("директория " + Path.GetFullPath(settings_dictionary["logs_path"].First() + " указанная в файле [assembly_logs_parser_settings.ini] - не существует"));
                    no_errors = false;
                }

                if (!File.Exists(settings_dictionary["data_base"].First()))
                {
                    add_to_main_log("в файле assembly_logs_parser_settings.ini указан путь к папке в которой отсутстует файл базой данных - data.rtdb");
                    add_to_main_log(Path.GetFullPath(settings_dictionary["data_base"].First()), false);
                    no_errors = false;
                }

                data_base_filename = Path.GetFileName(settings_dictionary["data_base"].First());

            }
            else
            {
                //  add_to_main_log("info\tфайл с настройками settings.ini отсутствует, создаю файл по умолчанию");
                string[] default_settings = new string[] {
                "[logs_path] путь к папке с файлами логов селекторов" ,
                    "logs",
                 "[logs_regex] маска для поиска лог файлов, по умолчанию 20210127_102717.log",
                    @"\d{8}_\d{6}.log",
                "[data_base] путь к базе данных",
                    @"db\data.rtdb"
                };
                File.WriteAllLines("assembly_logs_parser_settings.ini", default_settings);
                load_settings();
            }
            return no_errors;
        }


        private static void scan_logs()
        {
            string[] start_conference = new string[] { "started conference", "run conference", "load conference" }; // признаки запуска конференции

            //=================  формируем список лог файлов для обработки [assembly_logs_files_paths] =======================
            string logs_files_path = Path.GetFullPath(settings_dictionary["logs_path"].First()); // путь к файлам логов загружем из настроек
            add_to_main_log("читаю список логов из папки: " + logs_files_path);
            string[] assembly_logs_files_paths_temp = Directory.GetFiles(logs_files_path, "*.log"); // все подряд файлы *.log в директории

            List<string> assembly_logs_files_paths = new List<string>() { }; // здесь только файлы, попадающеие под маску 20210127_102717.log (таким образом в выборку не попадет файл output.log и любые другие с изменённым именем)

            foreach (string assembly_logs_file_path_temp in assembly_logs_files_paths_temp) // выбираем только те файлы, имя которых соответствует маске указанной в настройках раздела logs_regex
            {
                Regex logs_filename_regex = new Regex(settings_dictionary["logs_regex"].First()); // log filename ex -  20210127_102717.log
                Match logs_filename_match = logs_filename_regex.Match(Path.GetFileName(assembly_logs_file_path_temp));
                if (logs_filename_match.Success)
                {
                    //если имя файла из пути совпадает с маской, то сохраняем весь путь целиком до этого файла
                    assembly_logs_files_paths.Add(assembly_logs_file_path_temp);
                }
            }
            if (assembly_logs_files_paths.Count == 0)
            {
                add_to_main_log("в папке [" + logs_files_path + "]\r\n отсутствуют файлы название которых соответствует маске [" + settings_dictionary["logs_regex"].First() + "], указанной в файле настроек [assembly_logs_parser_settings.ini]");
            }
            else
                add_to_main_log("загружено [" + assembly_logs_files_paths.Count + "] файлов <yyyyMMdd_hhmmss.log>");

            //==================== сканируем каждый файл построчно ====================================================================
            Regex auto_conf_ID_regex = new Regex(@"[Rr]un conference: ClusterId=\d* Id=\d* SchemeId=\d*");
            // L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77

            Regex conf_ID_regex = new Regex(@"VSPThread:CONF\(\d*-\d*");
            //"VSPThread:CONF(1-203"

            Regex client_seance_ID_fullstring_regex = new Regex(@"->Outgoing seance \d* come in to conference");
            Regex client_seance_ID_regex = new Regex(@" \d* ");

            Regex snc_ID_regex = new Regex(@"(snc|SNC)=\d*"); 
            //VSPThread:CONF(1-77).PARTY(2-0,1283,Мастер бр.№2)->Outgoing seance 755349 come in to conference

            Dictionary<string, string> client_seance_ID_Dictionary = new Dictionary<string, string>(); // при подключении к конференции абоненту присваивается уникальный ID для этой конференции, словарь сохраняет соответствие <ID_абонента, ID_конференции>

            int i = 0;
            foreach (string assembly_logs_file_path in assembly_logs_files_paths)
            {
                i++;                
                add_to_main_log(String.Format("сканирую {0} [{1}/{2}]", Path.GetFileName(assembly_logs_file_path), i, assembly_logs_files_paths.Count));
                string[] assembly_log_file_lines = File.ReadAllLines(assembly_logs_file_path);

                foreach (string assembly_log_file_line in assembly_log_file_lines)
                {
                    string conf_id = "";

                    Match auto_conf_ID_match = auto_conf_ID_regex.Match(assembly_log_file_line);
                    if (auto_conf_ID_match.Success)
                    {
                        //  L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77
                        // делим по знаку равно, последний элемент массива - номер ID селектора
                        conf_id = auto_conf_ID_match.Value.Split('=').Last().Trim();
                    }

                    Match conf_ID_match = conf_ID_regex.Match(assembly_log_file_line);
                    //делим строку  "VSPThread:CONF(1-203" через дефис и сохраняем ID селектора
                    if (conf_ID_match.Success)
                    {
                        conf_id = conf_ID_match.Value.Split('-')[1].Trim();
                    }
                    Match client_seance_ID_match = client_seance_ID_fullstring_regex.Match(assembly_log_file_line);
                    //VSPThread:CONF(1-77).PARTY(11-0,2973,ФИО)->Outgoing seance 755411 come in to conference
                    if (client_seance_ID_match.Success)
                    {
                        // <из строчки Outgoing seance 755411 come in to conference> берём только ID сеанса
                        Match client_seance_ID_match_ = client_seance_ID_regex.Match(client_seance_ID_match.Value);
                        if (client_seance_ID_match_.Success)
                        {
                            //' 755411 '
                            if (!client_seance_ID_Dictionary.ContainsKey(client_seance_ID_match_.Value))
                            {
                                client_seance_ID_Dictionary.Add(client_seance_ID_match_.Value.Trim(), conf_id);
                            }
                            else
                            {
                                add_to_main_log(String.Format("Дублируется номер <{0}> сеанса в словаре активных сеансов абонента. Строка вызывающая ошибку: {1}", client_seance_ID_match_.Value, assembly_log_file_line));
                            }
                        }             
                    }

                    Match snc_ID_match = snc_ID_regex.Match(assembly_log_file_line);
                    //строка содержит SNC=3439821 или snc=3439821
                    if (snc_ID_match.Success)
                    {
                        // если ID сеанса есть в словаре, то получаем соответствующий ему ID селектора и эта строчка тоже попадёт в выборку
                        string clean_snc_id = snc_ID_match.Value.Split('=')[1]; // убираем из value лишнее 'SNC='
                        if (client_seance_ID_Dictionary.ContainsKey(clean_snc_id))
                        {
                            conf_id = client_seance_ID_Dictionary[clean_snc_id];
                        }
                    }


                    if (conf_id!="")
                    {
                        string temp_conf_id_file = Path.GetFullPath("assemblylogsparser_temp_folder") + "\\" + conf_id + ".txt";
                        File.AppendAllText(temp_conf_id_file, assembly_log_file_line + "\r\n");
                    }
                    
                }

            }

            add_to_main_log("готово", false);
        }





    }

}

