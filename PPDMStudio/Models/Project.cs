using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPDMStudio.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool UseWindowsAuthentication { get; set; } = true;

        public string BuildConnectionString(string? password = null)
        {
            if (UseWindowsAuthentication)
                return $"Server={Server};Database={Database};Trusted_Connection=True;TrustServerCertificate=True;";
            else
                return $"Server={Server};Database={Database};User Id={Username};Password={password};TrustServerCertificate=True;";
        }
    }
}
