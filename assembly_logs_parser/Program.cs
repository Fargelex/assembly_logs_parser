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
        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };
        static string data_base_filename = "";

        static void Main(string[] args)
        {
            load_settings();
            Console.ReadKey();  
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
