using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.ConsoleDataGenerator.ReferenceData;

namespace MigrationTools.ConsoleDataGenerator
{

    public class MarkdownLoader
    {
        private string markdownPath = "../../docs/Reference/";

        public MarkdownLoader()
        {
     
        }
        public MarkdownLoader(string path)
        {
            markdownPath = path;
        }

        public MarkdownInfo GetMarkdownForTopic(ClassData classData, string topic)
        {
            MarkdownInfo markdownInfo = new MarkdownInfo();
            markdownInfo.Topic = topic;
            string relativePath = GetMarkdownTopicPath(classData, topic);
            markdownInfo.Path = relativePath.Replace("../../", "").Replace("\\", "/");
            markdownInfo.Exists = System.IO.File.Exists(relativePath);
            markdownInfo.Markdown = LoadMarkdown(relativePath);
            return markdownInfo;
        }

        public string GetMarkdownTopicPath(ClassData classData, string topic)
        {
            return Path.Combine(markdownPath, $"{classData.TypeName}/{classData.ClassName}-{topic}.md");
        }

        public string LoadMarkdown(string path)
        {
            string notes = "";
            if (System.IO.File.Exists(path))
            {
                notes = System.IO.File.ReadAllText(path);
            }
            return notes;
        }

       
    }
    public class MarkdownInfo
    {
        public string Topic { get; internal set; }
        public string Path { get; internal set; }
        public bool Exists { get; internal set; }
        public string Markdown { get; internal set; }
    }
}
