using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Protoacme.Models
{
    /// <summary>
    /// Serializable Ability
    /// </summary>
    /// <typeparam name="TParent">Parent class.</typeparam>
    public abstract class SerializableBase<TParent>
        where TParent : class
    {
        /// <summary>
        /// Inherit the ability to serialze to base64 string and save to file.
        /// </summary>
        public SerializableBase() { }

        /// <summary>
        /// Serialize the current object to a base64 string
        /// </summary>
        /// <returns>Base64 string</returns>
        public virtual string ToBase64String()
        {
            var sObj = JsonConvert.SerializeObject(this);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sObj));
        }

        /// <summary>
        /// Save as a serialized base64 string.
        /// </summary>
        /// <param name="filePath">File name and path</param>
        /// <remarks>Directory must already exist.</remarks>
        public virtual void SaveToFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");

            string directory = filePath.Replace(Path.GetFileName(filePath), "");

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("Directory must exist.");

            string base64Encrypted = ToBase64String();

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(base64Encrypted);
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Create instance from a base64 string
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static TParent FromBase64String(string base64String)
        {
            TParent parent = null;

            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException("base64String null or empty");

            byte[] buffer = Convert.FromBase64String(base64String);
            if (buffer.Length > 0)
            {
                parent = JsonConvert.DeserializeObject<TParent>(Encoding.UTF8.GetString(buffer));
            }

            return parent;
        }

        /// <summary>
        /// Create instance from base64 encoded file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static TParent FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            string base64Encrypted = string.Empty;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    base64Encrypted = sr.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(base64Encrypted))
                throw new FormatException("File content is empty");

            return FromBase64String(base64Encrypted);
        }
    }
}
