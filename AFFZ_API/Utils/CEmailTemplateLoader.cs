using Newtonsoft.Json;

namespace AFFZ_API.Utils
{

    public class EmailTemplateLoader
    {
        //private readonly Dictionary<string, EmailTemplate> _templates;

        private readonly Dictionary<string, object> _templates;

        public EmailTemplateLoader(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Email templates file not found.", filePath);
            }

            var jsonContent = File.ReadAllText(filePath);

            // Deserialize into a dictionary with object type to handle different structures
            var rawTemplates = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent)
                               ?? throw new InvalidOperationException("Failed to load email templates.");

            _templates = new Dictionary<string, object>();

            foreach (var kvp in rawTemplates)
            {
                if (kvp.Key == "StatusNotifications")
                {
                    // Deserialize nested structure
                    var statusTemplates = JsonConvert.DeserializeObject<Dictionary<string, EmailTemplate>>(kvp.Value.ToString());
                    _templates[kvp.Key] = new StatusNotification { Templates = statusTemplates };
                }
                else
                {
                    // Deserialize directly
                    var template = JsonConvert.DeserializeObject<EmailTemplate>(kvp.Value.ToString());
                    _templates[kvp.Key] = template;
                }
            }
        }

        public T GetTemplate<T>(string key)
        {
            return _templates.ContainsKey(key) ? (T)_templates[key] : default;
        }

        public EmailTemplate GetEmailTemplate(string templateName)
        {
            if (_templates.ContainsKey(templateName))
            {
                return (EmailTemplate)_templates[templateName]; // Explicit cast
            }
            throw new KeyNotFoundException($"Template '{templateName}' not found.");
        }

        public EmailTemplate GetStatusNotificationTemplate(string statusId)
        {
            if (_templates.TryGetValue("StatusNotifications", out var statusData))
            {
                //var statusTemplates = JsonConvert.DeserializeObject<Dictionary<string, EmailTemplate>>(statusData);

                if (((StatusNotification)statusData).Templates != null && ((StatusNotification)statusData).Templates.TryGetValue(statusId, out var statusTemplate))
                {
                    return statusTemplate;
                }
            }

            throw new KeyNotFoundException($"Status notification template for ID '{statusId}' not found.");
        }
    }

    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class StatusNotification
    {
        public Dictionary<string, EmailTemplate> Templates { get; set; }
    }
}

