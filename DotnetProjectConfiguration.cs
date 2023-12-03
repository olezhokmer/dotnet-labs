namespace DotnetProject.Configuration {
    public class ProjectConfiguration
    {
        public string JwtSecret { get; set; }
        public string JwtAudience { get; set; }
        public string JwtIssuer { get; set; }
        public string DbConnectionString { get; set; }
    }
}