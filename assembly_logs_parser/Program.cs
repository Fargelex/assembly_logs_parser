using assembly_logs_parser.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace assembly_logs_parser
{
    internal class Program
    {

        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };
        static Dictionary<string, byte> scannedConferences = new Dictionary<string, byte> { };   // список обработанных селекторов

       static string scannedConferencesFolderName = "Селектора";
       static string statisticConferencesFolderName = "Статистика";


        static string processed_log_files = Path.GetFullPath("assemblylogsparser_temp_folder") + "\\processed_log_files.txt";

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            if (load_settings())
            {
                loadScannedConferences();
                scan_logs();
                //   scan_processed_log_files();
            }

            //  load_data_base();
            Console.WriteLine("Для выхода нажмите любую клавишу");
            Console.ReadKey();
        }

        private static void add_to_main_log(string text_log = "проверка", bool add_to_text_log = true)
        {
            //  string log_string = '[' + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]\t" + text_log.Trim();
            string log_string = $"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]\t {text_log.Trim()}";
            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(log_string);
            if (add_to_text_log)
            {
                File.AppendAllText($"{assemblyName}_log.log", log_string + "\r\n");
            }
        }

        private static bool load_settings()
        {
            bool no_errors = true;
            if (!Directory.Exists(scannedConferencesFolderName)) // папка в которую будут сохраняться селектора выбранные из лог файлов
            {
                add_to_main_log($"создаю папку '{scannedConferencesFolderName}' в " + Path.GetFullPath(scannedConferencesFolderName)) ;
                Directory.CreateDirectory(scannedConferencesFolderName);
            }
            if (!Directory.Exists(statisticConferencesFolderName)) // папка в которую будет сохраняться общая Статистика по конференциям за год
            {
                add_to_main_log($"создаю папку '{statisticConferencesFolderName}' в " + Path.GetFullPath(statisticConferencesFolderName));
                Directory.CreateDirectory(statisticConferencesFolderName);
            }

            if (!Directory.Exists("assemblylogsparser_temp_folder")) // папка в которую будет сохраняться рабочая инфа программы
            {
                add_to_main_log("создаю папку 'assemblylogsparser_temp_folder' в " + Path.GetFullPath("assemblylogsparser_temp_folder"));
                Directory.CreateDirectory("assemblylogsparser_temp_folder");

                add_to_main_log("создаю файл 'processed_log_files.txt' в " + processed_log_files, false);
                File.AppendAllText(processed_log_files, "# logs файлы ассамблеи, которые были обработаны программой" + "\r\n");
            }

            // List<string> fields_list = new List<string> { };
             if (File.Exists("assembly_logs_parser_settings.ini"))
            {
                add_to_main_log("info\tзагружаю настройки из файла - assembly_logs_parser_settings.ini");
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
                    add_to_main_log("директория [" + Path.GetFullPath(settings_dictionary["logs_path"].First() + "] указанная в файле [assembly_logs_parser_settings.ini] - не существует"));
                    no_errors = false;
                }


            }
            else
            {
                add_to_main_log("info\tфайл с настройками settings.ini отсутствует, создаю файл по умолчанию");
                string[] default_settings = new string[] {
                "[logs_path] путь к папке с файлами логов селекторов" ,
                    "logs",
                 "[logs_regex] маска для поиска лог файлов, по умолчанию 20210127_102717.log",
                    @"\d{8}_\d{6}.log",
                "[data_base] путь к базе данных",
                    @"db\data.rtdb"
                };
                File.WriteAllLines("assembly_logs_parser_settings.ini", default_settings);
                no_errors = load_settings();
            }
            return no_errors;
        }


        private static List<string> make_logs_files_paths_list()
        {
            //=================  формируем список путей до лог файлов [logs_files_paths_list] =======================
            string logs_files_folder_path = Path.GetFullPath(settings_dictionary["logs_path"].First()); // путь к файлам логов загружем из настроек
            add_to_main_log("читаю список логов из папки: " + logs_files_folder_path);
            List<string> logs_files_paths_list = new List<string>() { }; // здесь только файлы, попадающеие под маску 20210127_102717.log (таким образом в выборку не попадет файл output.log и любые другие с изменённым именем)
            try
            {
                string[] logs_files_paths_temp = Directory.GetFiles(logs_files_folder_path, "*.log"); // все подряд файлы *.log в директории logs_files_path
                
                foreach (string logs_file_path_temp in logs_files_paths_temp) // выбираем только те файлы, имя которых соответствует маске указанной в настройках раздела logs_regex
                {
                    Regex logs_filename_regex = new Regex(settings_dictionary["logs_regex"].First()); // log filename ex -  20210127_102717.log
                    Match logs_filename_match = logs_filename_regex.Match(Path.GetFileName(logs_file_path_temp));
                    if (logs_filename_match.Success)
                    {
                        //если имя файла из пути совпадает с маской, то сохраняем весь путь целиком до этого файла
                        logs_files_paths_list.Add(logs_file_path_temp);
                    }
                }
                if (logs_files_paths_list.Count == 0)
                {
                    add_to_main_log("в папке [" + logs_files_folder_path + "] отсутствуют файлы название которых соответствует маске [" + settings_dictionary["logs_regex"].First() + "], указанной в файле настроек [assembly_logs_parser_settings.ini]");
                }
                else
                    add_to_main_log("загружено [" + logs_files_paths_list.Count + "] файлов <ггггММдд_ччммсс.log>");
            }
            catch (Exception ex)
            {
                add_to_main_log("В папке logs отсутствуют файлы ");
             //   throw;
            }
            return logs_files_paths_list;
        }

        private static string get_time_from_log_line(string full_string)
        {
            string[] line_temp = full_string.Split(']');
            string time_string = line_temp[0]; //"L120[04.01.2021 16:58:31-318](ID:01-0208 VSPThread:CONF(1-203))->Login disp(1-666) started conference"                       
            time_string = time_string.Split('[')[1]; //" из строки L120[04.01.2021 16:58:31-318 получаем 04.01.2021 16:58:31-318
            time_string = time_string.Split('-')[0]; //убираем последние 4 символа 04.01.2021 16:58:31
            return time_string;
        }

        private static void loadScannedConferences()  // [40] 01.03.2023 15.04.00.txt
        {
            string statFilePath = Path.GetFullPath(statisticConferencesFolderName + "\\" + DateTime.Now.Year.ToString() + ".txt");
            if (File.Exists(statFilePath))
            {
                string[] statFileLines = File.ReadAllLines(statFilePath);
                for (int i = 1; i < statFileLines.Length; i++)
                {
                    string[] tmpArr = statFileLines[i].Split('\t');
                    string tmp = ($"[{tmpArr[2]}] {tmpArr[0]}.txt").Replace(':','.');// в файле статистики время через двоеточие, а в имени файла двоеточие заменено на точки, в scannedConferences добавляем имена файлов;
                    if (!scannedConferences.ContainsKey(tmp))
                    {
                        scannedConferences.Add(tmp, 0);
                    }
                }
            }
            add_to_main_log();
        }

        private static void scan_logs()
        {
            //   string[] start_conference = new string[] { "started conference", "run conference" }; // признаки запуска конференции
            string[] stop_conference = new string[] { "->State: Stop" }; // признаки завершения конференции

            Regex auto_conf_ID_regex = new Regex(@"[Rr]un conference: ClusterId=\d* Id=\d* SchemeId=\d*");  // признаки запуска конференции
            // L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77

            Regex conf_ID_regex = new Regex(@"VSPThread:CONF\(\d*-\d*");
            //"VSPThread:CONF(1-203"

            Regex client_seance_ID_fullstring_regex = new Regex(@"->Outgoing seance \d* come in to conference");
            Regex client_seance_ID_regex = new Regex(@" \d* ");

            Regex snc_ID_regex = new Regex(@"(snc|SNC)=\d*");
            //VSPThread:CONF(1-77).PARTY(2-0,1283,ФИО)->Outgoing seance 755349 come in to conference

            Dictionary<string, string> client_seance_ID_Dictionary = new Dictionary<string, string>(); // при подключении к конференции абоненту присваивается уникальный ID для этой конференции, словарь сохраняет соответствие <ID_сеанса, ID_конференции>
            Dictionary<string, Dictionary<int, Subscriber>> client_ID_Dictionary = new Dictionary<string, Dictionary<int, Subscriber>>(); // при подключении к конференции абоненту присваивается уникальный ID для этой конференции, словарь сохраняет соответствие <ID_сеанса, ID_конференции>


            int i = 0;

            List<string> logs_files_paths = make_logs_files_paths_list();

            Dictionary<string, conf_class> processed_conf_files_paths = new Dictionary<string, conf_class>();



            foreach (string logs_file_path in logs_files_paths) // сканируем каждый лог файл
            {
                i++;
                add_to_main_log(String.Format("сканирую {0} [{1}/{2}]", Path.GetFileName(logs_file_path), i, logs_files_paths.Count));
                string[] log_file_lines = File.ReadAllLines(logs_file_path);

                foreach (string log_file_line in log_file_lines) // сканируем каждую строчку из лог файла
                {
                    string conf_id = "";
                    bool started_conference_line = false;
                    Subscriber new_subscriber = new Subscriber();

                    //  bool valid_line = false; // строчка прошла проверки и должна быть записана в один из обработанных файлов

                    Match conf_ID_match = conf_ID_regex.Match(log_file_line);
                    //делим строку  "VSPThread:CONF(1-203" через дефис и сохраняем ID селектора
                    if (conf_ID_match.Success)
                    {
                        conf_id = conf_ID_match.Value.Split('-')[1].Trim();
                    }

                    //======= строчка содержит признак начала конференции по планировщику
                    Match auto_conf_ID_match = auto_conf_ID_regex.Match(log_file_line); //строчка содержит признак начала конференции по планировщику
                    if (auto_conf_ID_match.Success)
                    {
                        //  L120[19.02.2021 07:00:00-620](ID:01-0208 VSPThread:CONFPP)->Run conference: ClusterId=1 Id=19820 SchemeId=77
                        // делим по знаку равно, последний элемент массива - номер ID селектора
                        conf_id = auto_conf_ID_match.Value.Split('=').Last().Trim();
                        started_conference_line = true;
                        //    valid_line = true;
                    }

                    //L200[01.03.2023 00:00:47-681](ID:01-0208 VSPThread:CDISP)->SNC(3365447-1). Party ID=27 (Subscriber ID=4138) of conference ID=402 identified by phone=431760@10.8.5.18
                    if (log_file_line.Contains("identified by phone="))
                    {
                        Regex conf_ID_started_by_phone_regex = new Regex(@"conference id=\d*");
                        Match conf_ID_started_by_phone_match = conf_ID_started_by_phone_regex.Match(log_file_line.ToLower()); //строчка содержит признак начала конференции по телефону
                        if (conf_ID_started_by_phone_match.Success)
                        {
                            conf_id = conf_ID_started_by_phone_match.Value.Split('=')[1].Trim();
                        }
                        started_conference_line = true;
                    }

                    //строчка содержит признак начала конференции в ручную администратором
                    if (log_file_line.Contains("Login") && log_file_line.Contains("started conference"))  //"L120[04.01.2021 16:58:31-318](ID:01-0208 VSPThread:CONF(1-203))->Login disp(1-666) started conference"
                    {
                        started_conference_line = true;
                        //  valid_line = true;
                    }



                    // если строчка стартует конференцию
                    if (started_conference_line)
                    {
                        if (!processed_conf_files_paths.ContainsKey(conf_id)) // если в словаре нет ID стартовавшей конференции то добавляем
                        {
                            conf_class new_conf = new conf_class(conf_id);
                            if (new_conf.is_error)
                            {
                                add_to_main_log(new_conf.error_discription);
                                add_to_main_log("Строка в которой возникла ошибка: " + log_file_line);
                            }
                            else
                            {
                                processed_conf_files_paths.Add(conf_id, new_conf);
                            }


                        }

                        string conf_directory_path = scannedConferencesFolderName + "\\" + conf_id;
                        if (!Directory.Exists(conf_directory_path))
                        {
                            Directory.CreateDirectory(conf_directory_path);
                        }
                        string[] line_temp = log_file_line.Split(']');
                        string start_time_Line = get_time_from_log_line(log_file_line);

                        string start_time_Line_filename = $"[{conf_id}] {start_time_Line.Replace(':', '.')}.txt"; // start_time_Line.Replace(':', '.'); //04.01.2021 16.58.31

                        string pocessedFilePath = String.Format("{0}\\{1}", Path.GetFullPath(conf_directory_path), start_time_Line_filename);


                        // проверяем был ли такой селектор уже обработан, если был то строчки будут пропущены, т.к не будет сохранен путь файла в который нужно сохранять
                        if (!scannedConferences.ContainsKey(start_time_Line_filename))
                        {
                            processed_conf_files_paths[conf_id].start_time = start_time_Line;
                            processed_conf_files_paths[conf_id].pocessed_file_path = pocessedFilePath;


                            string manager_name = line_temp[1]; //  (ID:01-0208 VSPThread:CONF(1-203))->Login disp(1-666) started conference"
                            if (line_temp[1].Contains("started conference"))
                            {
                                manager_name = manager_name.Split('>')[1]; // Login disp(1-666) started conference"
                                manager_name = manager_name.Split('(')[0]; // Login disp
                                manager_name = manager_name.Split(' ')[1]; //disp
                                processed_conf_files_paths[conf_id].manager_name = manager_name;
                            }

                            //(ID:01-0208 VSPThread:CDISP)->SNC(3365447-1). Party ID=27 (Subscriber ID=4138) of conference ID=402 identified by phone=431760@10.8.5.18
                            if (line_temp[1].Contains("identified by phone="))
                            {
                                Regex conf_started_by_phone_regex = new Regex(@"identified by phone=\d*");
                                Match conf_started_by_phone_match = conf_started_by_phone_regex.Match(log_file_line.ToLower()); //строчка содержит признак начала конференции по телефону
                                if (conf_started_by_phone_match.Success)
                                {
                                    processed_conf_files_paths[conf_id].manager_name = conf_started_by_phone_match.Value.Split('=')[1];
                                }

                                string subs_id = line_temp[1].Split(')')[2]; // . Party ID=27 (Subscriber ID=4138
                                subs_id = subs_id.Split('=').Last();
                                try
                                {
                                    new_subscriber.SubscriberId = Convert.ToInt32(subs_id);
                                    new_subscriber.log_line.Add(log_file_line);
                                    if (!client_ID_Dictionary.ContainsKey(conf_id))
                                    {
                                        client_ID_Dictionary.Add(conf_id, new Dictionary<int, Subscriber> { });
                                    }

                                    if (!client_ID_Dictionary[conf_id].ContainsKey(new_subscriber.SubscriberId))
                                    {
                                        client_ID_Dictionary[conf_id].Add(new_subscriber.SubscriberId, new_subscriber);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    add_to_main_log(String.Format("Не удалось преобразовать SubscriberID в int, передаётся [{0}] из строки [{1}]", subs_id, line_temp[1]));
                                    throw;
                                }

                            }
                        }
                        else
                        {
                            add_to_main_log($"Селектор {start_time_Line_filename} уже сохранён");
                        }



                    }


                    // если строчка содержит ID сеанса абонента, который подключился к конференции
                    Match client_seance_ID_match = client_seance_ID_fullstring_regex.Match(log_file_line);
                    //VSPThread:CONF(1-77).PARTY(11-0,2973,ФИО)->Outgoing seance 755411 come in to conference
                    if (client_seance_ID_match.Success)
                    {
                        //  Subscriber new_subscriber = new Subscriber();
                        // Сохранить ID сеанса и ID абонента в классе Subscriber
                        Regex id_subscriber_regex = new Regex(@"PARTY\(\d*\-\d*\,\d*\,");
                        Match id_subscriber_match = id_subscriber_regex.Match(log_file_line);
                        if (id_subscriber_match.Success) // PARTY(11-0,2973,
                        {
                            string subscriber_id = id_subscriber_match.Value.Replace(',', ' ').Trim(); // PARTY(11-0 2973
                            subscriber_id = subscriber_id.Split(' ')[1];

                            new_subscriber.SubscriberId = Convert.ToInt32(subscriber_id);
                        }

                        // <из строчки Outgoing seance 755411 come in to conference> берём только ID сеанса
                        Match client_seance_ID_match_ = client_seance_ID_regex.Match(client_seance_ID_match.Value);
                        if (client_seance_ID_match_.Success)
                        {
                            //' 755411 '
                            if (!client_seance_ID_Dictionary.ContainsKey(client_seance_ID_match_.Value))
                            {
                                new_subscriber.SeanceId = Convert.ToInt32(client_seance_ID_match_.Value.Trim());
                                new_subscriber.log_line.Add(log_file_line);
                                if (!client_ID_Dictionary.ContainsKey(conf_id))
                                {
                                    client_ID_Dictionary.Add(conf_id, new Dictionary<int, Subscriber> { });
                                }

                                if (!client_ID_Dictionary[conf_id].ContainsKey(new_subscriber.SubscriberId))
                                {
                                    client_ID_Dictionary[conf_id].Add(new_subscriber.SubscriberId, new_subscriber);
                                }
                                else
                                {
                                    //   add_to_main_log(String.Format("абонент {0} повторно подключается к конференции {1} Строка [{2}]", new_subscriber.SubscriberId, conf_id, log_file_line));
                                    client_ID_Dictionary[conf_id][new_subscriber.SubscriberId].log_line.Add(log_file_line);
                                }

                                client_seance_ID_Dictionary.Add(client_seance_ID_match_.Value.Trim(), conf_id);
                            }
                            else
                            {
                                add_to_main_log(String.Format("Дублируется номер <{0}> сеанса в словаре активных сеансов абонента. Строка вызывающая ошибку: {1}", client_seance_ID_match_.Value, log_file_line));
                            }
                        }
                    }

                    Match snc_ID_match = snc_ID_regex.Match(log_file_line);
                    //строка содержит активность сеанса абонента SNC=3439821 или snc=3439821
                    if (snc_ID_match.Success)
                    {
                        // если ID сеанса есть в словаре, то получаем соответствующий ему ID селектора и эта строчка тоже попадёт в выборку
                        string clean_snc_id = snc_ID_match.Value.Split('=')[1]; // убираем из value лишнее 'SNC='
                        if (client_seance_ID_Dictionary.ContainsKey(clean_snc_id))
                        {
                            conf_id = client_seance_ID_Dictionary[clean_snc_id];
                        }
                    }


                    //если в словаре, где указаны пути обработанных селекторов, есть конференция с текущим ID 
                    if (processed_conf_files_paths.ContainsKey(conf_id))
                    {

                        //   string temp_conf_id_file = Path.GetFullPath("assemblylogsparser_temp_folder") + "\\" + conf_id + ".txt";
                        //  File.AppendAllText(processed_conf_files_paths[conf_id].pocessed_file_path, log_file_line + "\r\n");


                        if (processed_conf_files_paths[conf_id].pocessed_file_path != "")
                        {
                            if (File.Exists(processed_conf_files_paths[conf_id].pocessed_file_path))
                            {
                                File.AppendAllText(processed_conf_files_paths[conf_id].pocessed_file_path, log_file_line + "\r\n");
                            }
                            else
                            {
                                File.AppendAllText(processed_conf_files_paths[conf_id].pocessed_file_path, log_file_line + "\r\n");
                                add_to_main_log(String.Format("новый селектор [{0}]", processed_conf_files_paths[conf_id].pocessed_file_path));
                            }
                        }


                        // Определяем причину завершения конференции
                        /*
                         Завершение Планировщиком
                         L120[01.03.2023 08:29:30-367](ID:01-0208 VSPThread:CONFPP)->Conference ClusterId=1 Id=77512 SchemeId=140 will stop after 30 sec
                         L200[01.03.2023 08:29:30-367](ID:01-0208 VSPThread:CONF(1-140))->State: Stop                       
                        */
                        if (log_file_line.ToLower().Contains("will stop after ") && log_file_line.ToLower().Contains("->conference"))
                        {
                            string cnf_id_rom_line = log_file_line.Split('=').Last();
                            conf_id = cnf_id_rom_line.Split(' ').First();
                            processed_conf_files_paths[conf_id].stop_reason = "Завершено планировщиком";
                        }
                        /*
                        Введен код завершения
                        L200[01.03.2023 08:21:42-772](ID:01-0208 VSPThread:CONF(1-391).PARTY(1-0,874,Крылов А.В.))->DTMF: Conf stop signal
                        L200[01.03.2023 08:21:42-772](ID:01-0208 VSPThread:CONF(1-391).PARTY(1-0,874,Крылов А.В.))->Stoping conference: Executing...
                        L200[01.03.2023 08:21:42-772](ID:01-0208 VSPThread:CONF(1-391))->State: Stop */
                        if (log_file_line.Contains("->DTMF: Conf stop signal"))
                        {
                            processed_conf_files_paths[conf_id].stop_reason = "Введен код завершения";
                        }

                        /*  Остановил Супервизор
                        L120[01.03.2023 12:11:50-436](ID:01-0208 VSPThread:CONF(1-83))->Login Karkachev(1-10340) stoped conference
                        L200[01.03.2023 12:11:50-436](ID:01-0208 VSPThread:CONF(1-83))->State: Stop
                        */
                        if (log_file_line.Contains("stoped conference") && log_file_line.Contains("->Login"))
                        {
                            string supervisor_name = log_file_line.Split('(')[2];
                            supervisor_name = supervisor_name.Split(' ').Last();
                            processed_conf_files_paths[conf_id].stop_reason = "Остановил супервизор: " + supervisor_name;
                        }

                        /* Планировщик не смог запустить. Превышены лимиты
                          VSPThread:CONF(1-546))->Conference 1-546 limits exceed: CHAlloc=16/15, DSPAlloc=18/17
                         */
                        if (log_file_line.Contains("limits exceed: ") && log_file_line.Contains("->Conference"))
                        {
                            processed_conf_files_paths[conf_id].stop_reason = "Конференция не запустилась, превышен лимит " + log_file_line.Split(':').Last();
                        }



                        //если строчка содержит признак завершения конференции, очищаем путь к файлу записи, для создания нового.
                        if (log_file_line.ToLower().Contains(stop_conference[0].ToLower()))
                        {
                            // добавляем в список обработанных конференций
                            string pocessedFileName = Path.GetFileName(processed_conf_files_paths[conf_id].pocessed_file_path);
                            if (!scannedConferences.ContainsKey(pocessedFileName))
                            {
                                scannedConferences.Add(pocessedFileName, 0);
                            }

                            processed_conf_files_paths[conf_id].pocessed_file_path = "";

                            string stop_time_Line = get_time_from_log_line(log_file_line);

                            processed_conf_files_paths[conf_id].stop_time = stop_time_Line;

                            //  string start_time_Line_filename = stop_time_Line.Replace(':', '.'); //04.01.2021 16.58.31
                            if (client_ID_Dictionary.ContainsKey(conf_id))
                            {
                                processed_conf_files_paths[conf_id].real_participants_count = client_ID_Dictionary[conf_id].Count;
                            }
                            else
                                processed_conf_files_paths[conf_id].real_participants_count = 0;

                            string year_statistic_file_path = Path.GetFullPath(statisticConferencesFolderName + "\\" + processed_conf_files_paths[conf_id].year + ".txt");
                            if (!File.Exists(year_statistic_file_path))
                            {
                                File.AppendAllText(year_statistic_file_path, "Запуск\tЗавершение\tID селектора\tЗапустил\tФактическая продолжительность\tЗаданная продолжительность\tФактическое кол-во участников\tЗаданное кол-во участников\tПричина завершения\r\n");
                            }
                            File.AppendAllText(year_statistic_file_path, String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\r\n",
                                processed_conf_files_paths[conf_id].start_time,
                                processed_conf_files_paths[conf_id].stop_time,
                                processed_conf_files_paths[conf_id].ID,
                                processed_conf_files_paths[conf_id].manager_name,
                                processed_conf_files_paths[conf_id].real_duration,
                                processed_conf_files_paths[conf_id].seted_duration,
                                processed_conf_files_paths[conf_id].real_participants_count,
                                processed_conf_files_paths[conf_id].seted_participants_count,
                                processed_conf_files_paths[conf_id].stop_reason));
                            client_ID_Dictionary[conf_id] = new Dictionary<int, Subscriber> { };

                            //   client_ID_Dictionary[conf_id].;


                            //   processed_conf_files_paths[conf_id].pocessed_file_path = String.Format("{0}\\[{1}] {2}.txt", Path.GetFullPath(conf_directory_path), conf_id, start_time_Line_filename);

                        }

                        //если текущая строка содержит данные о параметрах созданной конференции
                        if (log_file_line.Contains("collection requested")) // L200[04.01.2021 16:00:00-335](ID:01-0208 VSPThread:CONF(1-64))->Conference 1-64 collection requested NP_MAX_CH=29, NP_MAX_DSP=31, duartion=5400 sec
                        {
                            string[] line_temp_2 = log_file_line.Split(']');
                            string start_time_Line = line_temp_2[0]; // L200[04.01.2021 16:00:00-335                    
                            start_time_Line = start_time_Line.Split('[')[1]; //" из строки L120[04.01.2021 16:58:31-318 получаем 04.01.2021 16:58:31-318
                            start_time_Line = start_time_Line.Split('-')[0]; //убираем последние 4 символа 04.01.2021 16:58:31

                            // время строки с параметрами должно быть равно времени старта конференции, это будет означать что эти параметры пренадлежат именно этому селектору
                            if (processed_conf_files_paths[conf_id].check_start_time(start_time_Line))
                            {
                                string conf_params_line = line_temp_2[1]; //  (ID:01-0208 VSPThread:CONF(1-64))->Conference 1-64 collection requested NP_MAX_CH=29, NP_MAX_DSP=31, duartion=5400 sec
                                //conf_params_line = conf_params_line.Split(' ')[1];
                                processed_conf_files_paths[conf_id].save_seted_params = conf_params_line;
                                if (processed_conf_files_paths[conf_id].is_error)
                                {
                                    add_to_main_log(processed_conf_files_paths[conf_id].error_discription);
                                }
                            }

                        }

                    }
                }
            }
            add_to_main_log("Готово", false);

        }


        #region Отдельные функции

        /*
        private static void scan_logs_only()
        {           
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

            
        }


        

        private static void scan_processed_log_files()
        {
            string[] start_conference = new string[] { "started conference", "run conference" }; // признаки запуска конференции
            string[] stop_conference = new string[] { "->State: Stop" }; // признаки завершения конференции


            string[] processed_log_files_paths = Directory.GetFiles(Path.GetFullPath("assemblylogsparser_temp_folder"));

            foreach (string processed_log_file_path in processed_log_files_paths)
            {
                // ID конференции из имени файла, без расширения .txt
                string conf_id_from_processed_log_file = Path.GetFileName(processed_log_file_path).Split('.')[0];
                //Создаем папку с номером селектора, в которую будут сохраняться обработанные отдельные конференции
                string conf_directory_path = "Селектора\\" + conf_id_from_processed_log_file;
                if (!Directory.Exists(conf_directory_path))
                {
                    Directory.CreateDirectory(conf_directory_path);
                }

                //загружаем файл с определённой конференцией
                string[] processed_log_file_lines = File.ReadAllLines(processed_log_file_path);
                string new_conf_file_path = ""; // путь к файлу в который будут писаться все последующий строчки пока не попадётся признак завершения конференции

                foreach (string processed_log_file_line in processed_log_file_lines) // читаем файл построчно
                {
                    foreach (var item in start_conference) //проверяем, содержит ли строка один из признаков начала конференции
                    {
                        if (processed_log_file_line.ToLower().IndexOf(item) > -1) // если содержит создаём файл в который будем писать все следующие строчки
                        {
                            new_conf_file_path = "";
                            string firstLine = processed_log_file_line.Split(']')[0]; //"L120[04.01.2021 16:58:31-318](ID:01-0208 VSPThread:CONF(1-203))->Login disp(1-666) started conference"
                            firstLine = firstLine.Split('[')[1]; //"L120[04.01.2021 16:58:31-318 -> 04.01.2021 16:58:31-318
                            firstLine = firstLine.Substring(0, firstLine.Length - 4); //04.01.2021 16:58:31
                                                                                      //     string firstLineForStat = firstLine;
                            firstLine = firstLine.Replace(':', '.'); //04.01.2021 16.58.31
                            new_conf_file_path = String.Format("{0}\\[{1}] {2}.txt", Path.GetFullPath(conf_directory_path), conf_id_from_processed_log_file, firstLine);

                        }
                    }
                    if (new_conf_file_path != "")
                    {
                        if (File.Exists(new_conf_file_path))
                        {
                            File.AppendAllText(new_conf_file_path, processed_log_file_line + "\r\n");
                        }
                        else
                        {
                            File.AppendAllText(new_conf_file_path, processed_log_file_line + "\r\n");
                            add_to_main_log(String.Format("создаю файл для сохранения нового селектора [{0}]", new_conf_file_path));
                        }

                    }
                    //если строчка содержит признак завершения конференции, очищаем путь к файлу записи, для создания нового.
                    if (processed_log_file_line.ToLower().Contains(stop_conference[0].ToLower()))
                    {
                        new_conf_file_path = "";
                    }
                }
            }

            add_to_main_log("готово", false);

        }

        */
        #endregion



    }

}


