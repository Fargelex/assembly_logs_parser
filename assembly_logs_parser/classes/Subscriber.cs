using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembly_logs_parser.classes
{
    // время подключения к конференции
    // время отключения от конференции
    // общая продолжительность нахождения в конференции
    // кол-во подключений к конференции и время каждого подключения/отключения
    // флаг завершения
    // включен\отключен микрофон и время события
    // начало разговора-конец разговора


    internal class Subscriber : IEquatable<Subscriber>
    {

        public string SubscriberName { get; set; }
        public int SubscriberId { get; set; }
        public int SeanceId { get; set; }

        public List<string> log_line { get; set; }


        public Subscriber()
        {
            log_line = new List<string> { };
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Subscriber objAsPart = obj as Subscriber;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override string ToString()
        {
            return SubscriberName;
        }

        public override int GetHashCode()
        {
            return SubscriberId;
        }

        public bool Equals(Subscriber other)
        {
            if (other == null) return false;
            return (this.SubscriberId.Equals(other.SubscriberId));
        }
    }
}
