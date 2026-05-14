using PPDMStudio.Models;
using System.IO;
using System.Text.Json;

namespace PPDMStudio.Services
{
    public interface IProjectService
    {
        List<Project> Projects { get; }
        Project? SelectedProject { get; }
        string? GetPassword(Guid projectId);
        void SelectProject(Project project);
        void SaveProject(Project project, string? password = null);
        void DeleteProject(Guid projectId);
        void SaveLastWellList(Guid projectId, int wellListId);
        int? GetLastWellList(Guid projectId);
        event Action? OnChange;
    }

    public class ProjectService : IProjectService
    {
        private readonly string _dataFolder;
        private readonly string _projectsFile;
        private readonly string _settingsFile;

        public List<Project> Projects { get; private set; } = new();
        public Project? SelectedProject { get; private set; }
        public event Action? OnChange;

        public ProjectService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PPDMStudio");

            _projectsFile = Path.Combine(_dataFolder, "projects.json");
            _settingsFile = Path.Combine(_dataFolder, "settings.json");

            Directory.CreateDirectory(_dataFolder);
            LoadProjects();
            LoadLastSelected();
        }

        private void LoadProjects()
        {
            if (!File.Exists(_projectsFile)) return;

            var json = File.ReadAllText(_projectsFile);
            Projects = JsonSerializer.Deserialize<List<Project>>(json) ?? new();
        }

        private void LoadLastSelected()
        {
            if (!File.Exists(_settingsFile) || !Projects.Any()) return;

            var settings = LoadSettings();

            if (settings?.LastProjectId == null) return;

            var last = Projects.FirstOrDefault(p => p.Id == settings.LastProjectId);
            SelectedProject = last ?? Projects.First();
        }

        private void SaveProjects()
        {
            var json = JsonSerializer.Serialize(Projects, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_projectsFile, json);
        }

        // Load settings from file
        private AppSettings LoadSettings()
        {
            if (!File.Exists(_settingsFile)) return new AppSettings();
            var json = File.ReadAllText(_settingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        // Save full settings object
        private void SaveSettings(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsFile, json);
        }

        // Save just the selected project
        private void SaveSettings()
        {
            if (SelectedProject == null) return;
            var settings = LoadSettings();
            settings.LastProjectId = SelectedProject.Id;
            SaveSettings(settings);
        }

        public void SelectProject(Project project)
        {
            SelectedProject = project;
            SaveSettings();
            NotifyStateChanged();
        }

        public void SaveProject(Project project, string? password = null)
        {
            var existing = Projects.FirstOrDefault(p => p.Id == project.Id);
            if (existing == null)
                Projects.Add(project);
            else
            {
                var index = Projects.IndexOf(existing);
                Projects[index] = project;
            }

            if (!project.UseWindowsAuthentication && password != null)
                SavePassword(project.Id, password);

            SaveProjects();
            NotifyStateChanged();
        }

        public void DeleteProject(Guid projectId)
        {
            Projects.RemoveAll(p => p.Id == projectId);
            DeletePassword(projectId);

            if (SelectedProject?.Id == projectId)
                SelectedProject = Projects.FirstOrDefault();

            SaveProjects();
            SaveSettings();
            NotifyStateChanged();
        }

        public void SaveLastWellList(Guid projectId, int wellListId)
        {
            var settings = LoadSettings();
            settings.LastWellListPerProject[projectId.ToString()] = wellListId;
            SaveSettings(settings);
        }

        public int? GetLastWellList(Guid projectId)
        {
            var settings = LoadSettings();
            if (settings.LastWellListPerProject.TryGetValue(projectId.ToString(), out var id))
                return id;
            return null;
        }

        public string? GetPassword(Guid projectId)
        {
            try
            {
                using var cred = new CredentialManagement.Credential();
                cred.Target = $"PPDMStudio_{projectId}";
                cred.Load();
                return cred.Password;
            }
            catch { return null; }
        }

        private void SavePassword(Guid projectId, string password)
        {
            using var cred = new CredentialManagement.Credential();
            cred.Target = $"PPDMStudio_{projectId}";
            cred.Password = password;
            cred.Username = "PPDMStudio";
            cred.Type = CredentialManagement.CredentialType.Generic;
            cred.PersistanceType = CredentialManagement.PersistanceType.LocalComputer;
            cred.Save();
        }

        private void DeletePassword(Guid projectId)
        {
            try
            {
                using var cred = new CredentialManagement.Credential();
                cred.Target = $"PPDMStudio_{projectId}";
                cred.Delete();
            }
            catch { }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        private class AppSettings
        {
            public Guid? LastProjectId { get; set; }
            public Dictionary<string, int> LastWellListPerProject { get; set; } = new();
        }
    }
}