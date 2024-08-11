using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp5
{
    internal class UserViewModel
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? Email { get; set; }

        public override string ToString()
        {
            return $"Имя: {Name} Фамилия: {Surname} Отчество: {Patronymic} Email: {Email}";
        }
    }
}
